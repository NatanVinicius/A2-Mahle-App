using A2MahleApp.Application.Features.Inspection.Services;
using A2MahleApp.Domain.Features.Inspection.Enums;

using ProductionAlias = A2MahleApp.Domain.Features.Production.Entities.Production;

namespace A2MahleApp.Application.Features.Production.Services;

public sealed class ProductionService : IProductionService, IDisposable
{
    private readonly IProductionRepository _repository;
    private readonly IInspectionService _inspectionService;
    private readonly SemaphoreSlim _stateLock = new(1, 1);
    private readonly System.Threading.Timer _dayChangeTimer;
    private bool _initialized;
    private bool _disposed;

    public ProductionAlias CurrentProduction { get; private set; } = CreateEmptyProduction(DateTime.Today);

    public event EventHandler<ProductionAlias>? ProductionChanged;

    public ProductionService(
        IProductionRepository repository,
        IInspectionService inspectionService)
    {
        _repository = repository;
        _inspectionService = inspectionService;
        _inspectionService.InspectionCompleted += OnInspectionCompleted;
        _dayChangeTimer = new System.Threading.Timer(
            OnDayChangeTimerElapsed,
            state: null,
            dueTime: Timeout.InfiniteTimeSpan,
            period: Timeout.InfiniteTimeSpan);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _stateLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            CurrentProduction = await GetOrCreateProductionAsync(DateTime.Today, cancellationToken);
            _initialized = true;
            ScheduleNextDayChange();
        }
        finally
        {
            _stateLock.Release();
        }

        ProductionChanged?.Invoke(this, CurrentProduction);
    }

    private async void OnInspectionCompleted(object? sender, Domain.Features.Inspection.Entities.Inspection inspection)
    {
        if (!_initialized || _disposed)
        {
            return;
        }

        bool currentProductionChanged = false;
        ProductionAlias production;

        await _stateLock.WaitAsync();
        try
        {
            DateTime inspectionDate = inspection.DateTime.Date;

            if (inspectionDate > CurrentProduction.Date.Date)
            {
                CurrentProduction = await GetOrCreateProductionAsync(inspectionDate);
                currentProductionChanged = true;
            }

            production = inspectionDate == CurrentProduction.Date.Date
                ? CurrentProduction
                : await GetOrCreateProductionAsync(inspectionDate);

            production.Produced++;

            if (inspection.Status == InspectionStatus.Approved)
            {
                production.Approved++;
            }
            else
            {
                production.Rejected++;
            }

            await _repository.SaveAsync(production);
        }
        finally
        {
            _stateLock.Release();
        }

        if (currentProductionChanged || production.Date.Date == CurrentProduction.Date.Date)
        {
            ProductionChanged?.Invoke(this, CurrentProduction);
        }
    }

    private async void OnDayChangeTimerElapsed(object? state)
    {
        if (_disposed)
        {
            return;
        }

        bool currentProductionChanged = false;

        try
        {
            await _stateLock.WaitAsync();
            try
            {
                if (!_initialized)
                {
                    return;
                }

                DateTime today = DateTime.Today;
                if (CurrentProduction.Date.Date != today)
                {
                    CurrentProduction = await GetOrCreateProductionAsync(today);
                    currentProductionChanged = true;
                }
            }
            finally
            {
                _stateLock.Release();
            }
        }
        catch (ObjectDisposedException) when (_disposed)
        {
            return;
        }
        finally
        {
            ScheduleNextDayChange();
        }

        if (currentProductionChanged)
        {
            ProductionChanged?.Invoke(this, CurrentProduction);
        }
    }

    private async Task<ProductionAlias> GetOrCreateProductionAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        ProductionAlias? production = await _repository.GetByDateAsync(date, cancellationToken);
        if (production is not null)
        {
            return production;
        }

        ProductionAlias newProduction = CreateEmptyProduction(date);
        await _repository.SaveAsync(newProduction, cancellationToken);
        return newProduction;
    }

    private void ScheduleNextDayChange()
    {
        if (_disposed)
        {
            return;
        }

        DateTime now = DateTime.Now;
        DateTime nextDay = now.Date.AddDays(1);
        _dayChangeTimer.Change(nextDay - now, Timeout.InfiniteTimeSpan);
    }

    private static ProductionAlias CreateEmptyProduction(DateTime date) => new()
    {
        Date = date.Date,
        Produced = 0,
        Approved = 0,
        Rejected = 0
    };

    public void Dispose()
    {
        _disposed = true;
        _inspectionService.InspectionCompleted -= OnInspectionCompleted;
        _dayChangeTimer.Dispose();
        _stateLock.Dispose();
    }
}
