using A2MahleApp.Application.Features.Inspection.Contracts;
using A2MahleApp.Application.Features.Inspection.Models;
using A2MahleApp.Domain.Features.Inspection.Enums;

namespace A2MahleApp.Application.Features.Inspection.Services;

public interface IVisionSensorService
{
    ConnectionState ConnectionState { get; }

    event EventHandler<ConnectionState>? ConnectionStateChanged;

    event EventHandler<byte[]>? ImageReceived;

    event EventHandler<InspectionResult>? ResultReceived;

    Task ConnectAsync(CancellationToken cancellationToken = default);

    Task DisconnectAsync();

    Task ReconnectAsync(CancellationToken cancellationToken = default);
}
