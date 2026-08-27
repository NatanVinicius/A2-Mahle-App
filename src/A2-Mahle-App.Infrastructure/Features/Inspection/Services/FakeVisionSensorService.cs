using System.Reflection;

using A2MahleApp.Application.Features.Inspection.Contracts;
using A2MahleApp.Application.Features.Inspection.Models;
using A2MahleApp.Application.Features.Inspection.Services;
using A2MahleApp.Domain.Features.Inspection.Enums;

namespace A2MahleApp.Infrastructure.Features.Inspection.Services;

public sealed class FakeVisionSensorService : IVisionSensorService
{
    private static readonly TimeSpan InspectionInterval = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan ResultDelay = TimeSpan.FromMilliseconds(100);

    private CancellationTokenSource? _simulationCancellation;
    private Task? _simulationTask;

    public ConnectionState ConnectionState { get; private set; } = ConnectionState.Disconnected;

    public event EventHandler<ConnectionState>? ConnectionStateChanged;

    public event EventHandler<byte[]>? ImageReceived;

    public event EventHandler<InspectionResult>? ResultReceived;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (ConnectionState is ConnectionState.Connected or ConnectionState.Connecting)
        {
            return;
        }

        SetConnectionState(ConnectionState.Connecting);

        await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        SetConnectionState(ConnectionState.Connected);
        StartSimulation();
    }

    public async Task DisconnectAsync()
    {
        _simulationCancellation?.Cancel();

        if (_simulationTask is not null)
        {
            try
            {
                await _simulationTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        _simulationTask = null;
        _simulationCancellation?.Dispose();
        _simulationCancellation = null;

        SetConnectionState(ConnectionState.Disconnected);
    }

    public async Task ReconnectAsync(CancellationToken cancellationToken = default)
    {
        SetConnectionState(ConnectionState.Reconnecting);

        _simulationCancellation?.Cancel();

        if (_simulationTask is not null)
        {
            try
            {
                await _simulationTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        _simulationTask = null;
        _simulationCancellation?.Dispose();
        _simulationCancellation = null;

        await ConnectAsync(cancellationToken);
    }

    private void StartSimulation()
    {
        _simulationCancellation?.Cancel();
        _simulationCancellation?.Dispose();

        _simulationCancellation = new CancellationTokenSource();
        _simulationTask = SimulateInspectionsAsync(_simulationCancellation.Token);
    }

    private async Task SimulateInspectionsAsync(CancellationToken cancellationToken)
    {
        bool approved = true;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(InspectionInterval, cancellationToken);

                if (cancellationToken.IsCancellationRequested ||
                    ConnectionState != ConnectionState.Connected)
                {
                    continue;
                }

                byte[] image = LoadFakeImage();
                SimulateImage(image);

                await Task.Delay(ResultDelay, cancellationToken);

                if (approved)
                {
                    SimulateApprovedResult();
                }
                else
                {
                    SimulateRejectedResult();
                }

                approved = !approved;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private void SimulateImage(byte[] image)
    {
        if (ConnectionState != ConnectionState.Connected)
        {
            return;
        }

        ImageReceived?.Invoke(this, image);
    }

    private void SimulateApprovedResult()
    {
        SimulateResult(InspectionStatus.Approved);
    }

    private void SimulateRejectedResult()
    {
        SimulateResult(InspectionStatus.Rejected);
    }

    private void SimulateResult(InspectionStatus status)
    {
        if (ConnectionState != ConnectionState.Connected)
        {
            return;
        }

        ResultReceived?.Invoke(this, new InspectionResult
        {
            Status = status,
            CycleTimeMilliseconds = status == InspectionStatus.Approved ? 120 : 150
        });
    }

    private static byte[] LoadFakeImage()
    {
        const string resourceName = "A2MahleApp.Infrastructure.FakeData.testimage.bmp";

        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded fake image '{resourceName}' was not found.");

        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }

    private void SetConnectionState(ConnectionState state)
    {
        if (ConnectionState == state)
        {
            return;
        }

        ConnectionState = state;
        ConnectionStateChanged?.Invoke(this, state);
    }
}
