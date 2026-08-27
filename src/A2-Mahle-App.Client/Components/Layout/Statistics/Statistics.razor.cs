using A2MahleApp.Application.Features.Inspection.Contracts;
using A2MahleApp.Application.Features.Inspection.Services;
using A2MahleApp.Application.Features.Production.Services;
using A2MahleApp.Domain.Features.Inspection.Enums;
using InspectionEntity = A2MahleApp.Domain.Features.Inspection.Entities.Inspection;

using Microsoft.AspNetCore.Components;

namespace A2MahleApp.Client.Components.Layout.Statistics;

public partial class Statistics : IDisposable
{
    private int _produced;
    private int _approved;
    private int _rejected;
    private InspectionStatus? _lastStatus;
    private int _cycleTimeMilliseconds;
    private ConnectionState _connectionState = ConnectionState.Disconnected;

    [Inject]
    private IInspectionService InspectionService { get; set; } = null!;

    [Inject]
    private IProductionService ProductionService { get; set; } = null!;

    protected string LastInspectionText => _lastStatus switch
    {
        InspectionStatus.Approved => "Aprovada",
        InspectionStatus.Rejected => "Reprovada",
        _ => "-"
    };

    protected string RejectRate =>
        _produced == 0 ? "0%" : $"{_rejected * 100.0 / _produced:0.#}%";

    protected string CycleTime => $"{_cycleTimeMilliseconds} ms";

    protected string Availability =>
        _connectionState == ConnectionState.Connected ? "100%" : "0%";

    protected string ModeText =>
        _connectionState == ConnectionState.Connected ? "Running" : "Stopped";

    protected override void OnInitialized()
    {
        _produced = ProductionService.CurrentProduction.Produced;
        _approved = ProductionService.CurrentProduction.Approved;
        _rejected = ProductionService.CurrentProduction.Rejected;

        InspectionEntity? currentInspection = InspectionService.CurrentInspection;
        if (currentInspection is not null)
        {
            _lastStatus = currentInspection.Status;
            _cycleTimeMilliseconds = currentInspection.CycleTimeMilliseconds;
        }

        _connectionState = InspectionService.ConnectionState;

        ProductionService.ProductionChanged += OnProductionChanged;
        InspectionService.InspectionCompleted += OnInspectionCompleted;
        InspectionService.ConnectionStateChanged += OnConnectionStateChanged;
    }

    private void OnProductionChanged(
        object? sender,
        A2MahleApp.Domain.Features.Production.Entities.Production production)
    {
        _produced = production.Produced;
        _approved = production.Approved;
        _rejected = production.Rejected;
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnInspectionCompleted(object? sender, InspectionEntity inspection)
    {
        _lastStatus = inspection.Status;
        _cycleTimeMilliseconds = inspection.CycleTimeMilliseconds;
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnConnectionStateChanged(object? sender, ConnectionState state)
    {
        _connectionState = state;
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        ProductionService.ProductionChanged -= OnProductionChanged;
        InspectionService.InspectionCompleted -= OnInspectionCompleted;
        InspectionService.ConnectionStateChanged -= OnConnectionStateChanged;
    }
}
