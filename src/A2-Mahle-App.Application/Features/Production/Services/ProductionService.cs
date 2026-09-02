using A2MahleApp.Application.Features.Inspection.Services;
using A2MahleApp.Domain.Features.Inspection.Enums;

using ProductionAlias = A2MahleApp.Domain.Features.Production.Entities.Production;

namespace A2MahleApp.Application.Features.Production.Services;

public sealed class ProductionService : IProductionService, IDisposable
{
    private readonly IProductionRepository _repository;
    private readonly IInspectionService _inspectionService;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private bool _initialized;

    public ProductionAlias CurrentProduction { get; private set; } = CreateEmptyProduction();

    public event EventHandler<ProductionAlias>? ProductionChanged;

    public ProductionService(
        IProductionRepository repository,
        IInspectionService inspectionService)
    {
        _repository = repository;
        _inspectionService = inspectionService;
        _inspectionService.InspectionCompleted += OnInspectionCompleted;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        DateTime today = DateTime.Today;
        ProductionAlias? production = await _repository.GetByDateAsync(today, cancellationToken);

        CurrentProduction = production ?? CreateEmptyProduction();

        if (production is null)
        {
            await _repository.SaveAsync(CurrentProduction, cancellationToken);
        }

        _initialized = true;
        ProductionChanged?.Invoke(this, CurrentProduction);
    }

    private async void OnInspectionCompleted(object? sender, Domain.Features.Inspection.Entities.Inspection inspection)
    {
        if (!_initialized)
        {
            return;
        }

        CurrentProduction.Produced++;

        if (inspection.Status == InspectionStatus.Approved)
        {
            CurrentProduction.Approved++;
        }
        else
        {
            CurrentProduction.Rejected++;
        }

        ProductionChanged?.Invoke(this, CurrentProduction);

        await _saveLock.WaitAsync();
        try
        {
            await _repository.SaveAsync(CurrentProduction);
        }
        finally
        {
            _saveLock.Release();
        }
    }

    private static ProductionAlias CreateEmptyProduction() => new()
    {
        Date = DateTime.Today,
        Produced = 0,
        Approved = 0,
        Rejected = 0
    };

    public void Dispose()
    {
        _inspectionService.InspectionCompleted -= OnInspectionCompleted;
        _saveLock.Dispose();
    }
}
