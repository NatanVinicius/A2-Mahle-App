using A2MahleApp.Application.Features.Inspection.Contracts;
using InspectionEntity = A2MahleApp.Domain.Features.Inspection.Entities.Inspection;

namespace A2MahleApp.Application.Features.Inspection.Services;

public sealed class InspectionService : IInspectionService
{
    public ConnectionState ConnectionState { get; private set; } = ConnectionState.Disconnected;

    public InspectionEntity? CurrentInspection { get; private set; }

    public event EventHandler<ConnectionState>? ConnectionStateChanged;

    public event EventHandler<InspectionEntity>? InspectionCompleted;

    public void PublishConnectionState(ConnectionState state)
    {
        ConnectionState = state;
        ConnectionStateChanged?.Invoke(this, state);
    }

    public void Publish(InspectionEntity inspection)
    {
        CurrentInspection = inspection;
        InspectionCompleted?.Invoke(this, inspection);
    }
}
