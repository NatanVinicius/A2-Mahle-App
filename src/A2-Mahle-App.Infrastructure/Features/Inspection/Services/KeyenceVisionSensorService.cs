using System.Net;

using A2MahleApp.Application.Features.Inspection.Contracts;
using A2MahleApp.Application.Features.Inspection.Models;
using A2MahleApp.Application.Features.Inspection.Services;
using A2MahleApp.Domain.Features.Inspection.Enums;

using Keyence.IV4.Sdk;
using Microsoft.Extensions.Logging;

namespace A2MahleApp.Infrastructure.Features.Inspection.Services;

public sealed class KeyenceVisionSensorService : IVisionSensorService
{
    private const int SensorPort = 63000;
    private const int ImageWidth = 1280;
    private const int ImageHeight = 960;
    private const int BytesPerPixel = 3;
    private static readonly TimeSpan TickTackInterval = TimeSpan.FromMilliseconds(30);

    private readonly ICommunicationEndpointSettingsService _communicationEndpointSettingsService;
    private readonly ILogger<KeyenceVisionSensorService> _logger;
    private readonly object _sync = new();
    private VisionSensorStore? _store;
    private IVisionSensor? _sensor;
    private CancellationTokenSource? _tickTackCancellation;
    private Task? _tickTackTask;

    public KeyenceVisionSensorService(
        ICommunicationEndpointSettingsService communicationEndpointSettingsService,
        ILogger<KeyenceVisionSensorService> logger)
    {
        _communicationEndpointSettingsService = communicationEndpointSettingsService;
        _logger = logger;
    }

    public ConnectionState ConnectionState { get; private set; } = ConnectionState.Disconnected;

    public event EventHandler<ConnectionState>? ConnectionStateChanged;

    public event EventHandler<byte[]>? ImageReceived;

    public event EventHandler<InspectionResult>? ResultReceived;

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (ConnectionState is ConnectionState.Connected or ConnectionState.Connecting)
        {
            return Task.CompletedTask;
        }

        cancellationToken.ThrowIfCancellationRequested();

        ComunicationTestServiceModel settings = _communicationEndpointSettingsService.Current;

        if (!IPAddress.TryParse(settings.Host, out IPAddress? startPoint) ||
            !IPAddress.TryParse(settings.IV4, out IPAddress? sensorIp))
        {
            _logger.LogWarning(
                "Não foi possível conectar ao sensor IV4: IP inválido (Host='{Host}', IV4='{IV4}').",
                settings.Host,
                settings.IV4);

            SetConnectionState(ConnectionState.Disconnected);
            return Task.CompletedTask;
        }

        _logger.LogInformation(
            "Conectando ao sensor IV4 {SensorIp}:{Port} (StartPoint {StartPoint}).",
            sensorIp,
            SensorPort,
            startPoint);

        SetConnectionState(ConnectionState.Connecting);

        VisionSensorStore? store = null;

        try
        {
            store = new VisionSensorStore();
            VisionSensorStore.StartPoint = startPoint;

            IVisionSensor sensor = store.Create(sensorIp, SensorPort);
            sensor.ImageAcquired += OnImageAcquired;
            sensor.ResultUpdated += OnResultUpdated;
            sensor.EventEnable = true;

            _store = store;
            _sensor = sensor;
        }
        catch (Exception exception) when (
            exception is ApplicationException or
            ConnectionLostException or
            InvalidOperationException or
            ArgumentException)
        {
            _logger.LogError(
                exception,
                "Falha ao conectar ao sensor IV4 {SensorIp}:{Port}.",
                sensorIp,
                SensorPort);

            store?.Dispose();
            SetConnectionState(ConnectionState.Disconnected);
            return Task.CompletedTask;
        }

        _logger.LogInformation("Conectado ao sensor IV4 {SensorIp}:{Port}.", sensorIp, SensorPort);

        SetConnectionState(ConnectionState.Connected);
        StartTickTack();

        return Task.CompletedTask;
    }

    public async Task DisconnectAsync()
    {
        await StopTickTackAsync();
        ReleaseSdkResources();
        SetConnectionState(ConnectionState.Disconnected);

        _logger.LogInformation("Sensor IV4 desconectado.");
    }

    public async Task ReconnectAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reconectando ao sensor IV4.");

        SetConnectionState(ConnectionState.Reconnecting);

        await StopTickTackAsync();
        ReleaseSdkResources();

        await ConnectAsync(cancellationToken);
    }

    private void StartTickTack()
    {
        IVisionSensor? sensor = _sensor;
        if (sensor is null)
        {
            return;
        }

        CancellationTokenSource cancellation = new();

        lock (_sync)
        {
            _tickTackCancellation = cancellation;
            _tickTackTask = Task.Run(() => TickTackLoopAsync(sensor, cancellation.Token));
        }
    }

    private async Task TickTackLoopAsync(IVisionSensor sensor, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                sensor.TickTack();

                await Task.Delay(TickTackInterval, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (ConnectionLostException exception)
        {
            _logger.LogError(exception, "Conexão com o sensor IV4 perdida durante TickTack.");
            HandleConnectionLost();
        }
    }

    private void HandleConnectionLost()
    {
        ReleaseSdkResources();
        SetConnectionState(ConnectionState.Disconnected);
    }

    private void OnImageAcquired(object? sender, ImageAcquiredEventArgs eventArgs)
    {
        try
        {
            if (eventArgs.LiveImage is null)
            {
                _logger.LogWarning("O sensor IV4 informou uma imagem nula.");
                return;
            }

            byte[] image = ConvertToBmp(eventArgs.LiveImage.ByteData);
            ImageReceived?.Invoke(this, image);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Falha ao processar a imagem recebida do sensor IV4.");
        }
    }

    private void OnResultUpdated(object? sender, ToolResultUpdatedEventArgs eventArgs)
    {
        try
        {
            ResultReceived?.Invoke(this, new InspectionResult
            {
                Status = eventArgs.TotalStatusResult
                    ? InspectionStatus.Approved
                    : InspectionStatus.Rejected
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Falha ao processar o resultado recebido do sensor IV4.");
        }
    }

    private static byte[] ConvertToBmp(byte[] bgrData)
    {
        int sourceRowSize = ImageWidth * BytesPerPixel;
        int paddedRowSize = (sourceRowSize + 3) & ~3;
        int pixelDataSize = paddedRowSize * ImageHeight;
        int fileSize = 54 + pixelDataSize;

        if (bgrData.Length < sourceRowSize * ImageHeight)
        {
            throw new InvalidOperationException(
                $"A imagem recebida do sensor IV4 possui {bgrData.Length} bytes; " +
                $"eram esperados pelo menos {sourceRowSize * ImageHeight}.");
        }

        byte[] bmp = new byte[fileSize];

        bmp[0] = (byte)'B';
        bmp[1] = (byte)'M';
        WriteInt32(bmp, 2, fileSize);
        WriteInt32(bmp, 10, 54);
        WriteInt32(bmp, 14, 40);
        WriteInt32(bmp, 18, ImageWidth);
        WriteInt32(bmp, 22, ImageHeight);
        WriteInt16(bmp, 26, 1);
        WriteInt16(bmp, 28, 24);
        WriteInt32(bmp, 34, pixelDataSize);

        for (int sourceRow = 0; sourceRow < ImageHeight; sourceRow++)
        {
            int destinationRow = ImageHeight - 1 - sourceRow;
            int sourceOffset = sourceRow * sourceRowSize;
            int destinationOffset = 54 + destinationRow * paddedRowSize;

            Buffer.BlockCopy(bgrData, sourceOffset, bmp, destinationOffset, sourceRowSize);
        }

        return bmp;
    }

    private static void WriteInt16(byte[] buffer, int offset, short value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
    }

    private static void WriteInt32(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        buffer[offset + 2] = (byte)(value >> 16);
        buffer[offset + 3] = (byte)(value >> 24);
    }

    private async Task StopTickTackAsync()
    {
        CancellationTokenSource? cancellation;
        Task? task;

        lock (_sync)
        {
            cancellation = _tickTackCancellation;
            task = _tickTackTask;

            _tickTackCancellation = null;
            _tickTackTask = null;
        }

        cancellation?.Cancel();

        if (task is not null)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
            }
            catch (ConnectionLostException)
            {
            }
        }

        cancellation?.Dispose();
    }

    private void ReleaseSdkResources()
    {
        lock (_sync)
        {
            if (_sensor is not null)
            {
                _sensor.ImageAcquired -= OnImageAcquired;
                _sensor.ResultUpdated -= OnResultUpdated;
            }

            _sensor?.Dispose();
            _sensor = null;

            _store?.Dispose();
            _store = null;
        }
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
