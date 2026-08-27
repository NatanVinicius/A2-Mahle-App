using A2MahleApp.Application.Features.Inspection.Contracts;
using A2MahleApp.Application.Features.Inspection.Services;

using System.Timers;

using Microsoft.AspNetCore.Components;

using Timer = System.Timers.Timer;

namespace A2MahleApp.Client.Components.Layout.Header;

public partial class Header : IDisposable
{
    private readonly Timer _timer = new(1000);
    private ConnectionState _connectionState = ConnectionState.Disconnected;

    [Inject]
    private IPopupService PopupService { get; set; } = null!;

    [Inject]
    private IInspectionService InspectionService { get; set; } = null!;

    protected string CurrentTime =>
        DateTime.Now.ToString("HH:mm:ss");

    protected string CurrentDate =>
        DateTime.Now.ToString("dd/MM/yyyy");

    protected string ConnectionText =>
        _connectionState switch
        {
            ConnectionState.Connected => "Conectado",
            ConnectionState.Connecting => "Conectando",
            ConnectionState.Reconnecting => "Reconectando",
            _ => "Desconectado"
        };

    protected string ConnectionIndicatorCss =>
        _connectionState switch
        {
            ConnectionState.Connected => "bg-green-500",
            ConnectionState.Connecting or ConnectionState.Reconnecting => "bg-yellow-500",
            _ => "bg-red-500"
        };

    protected override void OnInitialized()
    {
        _connectionState = InspectionService.ConnectionState;
        InspectionService.ConnectionStateChanged += OnConnectionStateChanged;

        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    private void OnConnectionStateChanged(
        object? sender,
        ConnectionState state)
    {
        _connectionState = state;
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnTimerElapsed(
        object? sender,
        ElapsedEventArgs e)
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private void OpenCommunicationTest()
    {
        PopupService.OpenCommunicationTest();
    }

    public void Dispose()
    {
        InspectionService.ConnectionStateChanged -= OnConnectionStateChanged;
        _timer.Stop();
        _timer.Elapsed -= OnTimerElapsed;
        _timer.Dispose();
    }
}
