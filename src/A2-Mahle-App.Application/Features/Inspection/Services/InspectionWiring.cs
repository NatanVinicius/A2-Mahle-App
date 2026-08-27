using A2MahleApp.Application.Features.Inspection.Correlation;
using A2MahleApp.Application.Features.Inspection.Services;
using InspectionEntity = A2MahleApp.Domain.Features.Inspection.Entities.Inspection;

namespace A2MahleApp.Application.Features.Inspection.Wiring;

public sealed class InspectionWiring
{
    private readonly IVisionSensorService _sensorService;
    private readonly InspectionCorrelation _correlation;
    private readonly IInspectionService _inspectionService;
    private bool _connected;

    public InspectionWiring(
        IVisionSensorService sensorService,
        InspectionCorrelation correlation,
        IInspectionService inspectionService)
    {
        _sensorService = sensorService;
        _correlation = correlation;
        _inspectionService = inspectionService;
    }

    public void Connect()
    {
        if (_connected)
        {
            return;
        }

        _sensorService.ConnectionStateChanged += OnConnectionStateChanged;
        _sensorService.ImageReceived += OnImageReceived;
        _sensorService.ResultReceived += OnResultReceived;
        _correlation.InspectionCompleted += OnInspectionCompleted;

        _inspectionService.PublishConnectionState(_sensorService.ConnectionState);
        _connected = true;
    }

    private void OnConnectionStateChanged(object? sender, Contracts.ConnectionState state)
    {
        _inspectionService.PublishConnectionState(state);
    }

    private void OnImageReceived(object? sender, byte[] image)
    {
        _correlation.ReceiveImage(image);
    }

    private void OnResultReceived(object? sender, Application.Features.Inspection.Models.InspectionResult result)
    {
        _correlation.ReceiveResult(result);
    }

    private void OnInspectionCompleted(object? sender, InspectionEntity inspection)
    {
        _inspectionService.Publish(inspection);
    }
}
