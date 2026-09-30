using A2MahleApp.Application.Features.Inspection.Contracts;
using InspectionEntity = A2MahleApp.Domain.Features.Inspection.Entities.Inspection;

namespace A2MahleApp.Application.Features.Inspection.Services;

public interface IInspectionService
{
    ConnectionState ConnectionState { get; }

    InspectionEntity? CurrentInspection { get; }

    event EventHandler<ConnectionState>? ConnectionStateChanged;

    event EventHandler<InspectionEntity>? InspectionCompleted;

    void PublishConnectionState(ConnectionState state);

    void Publish(InspectionEntity inspection);
}
