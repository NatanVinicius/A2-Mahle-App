using System.Timers;

using Microsoft.AspNetCore.Components;

using Timer = System.Timers.Timer;
namespace A2MahleApp.Client.Components.Layout.Header;

public partial class Header : IDisposable
{
    private readonly Timer _timer = new(1000);

    [Inject]
    private IPopupService PopupService { get; set; } = null!;

    protected string CurrentTime =>
      DateTime.Now.ToString("HH:mm:ss");

    protected string CurrentDate =>
        DateTime.Now.ToString("dd/MM/yyyy");


    protected override void OnInitialized()
    {
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    private void OnTimerElapsed(
        object? sender,
        ElapsedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void OpenCommunicationTest()
    {
        PopupService.OpenCommunicationTest();
    }

    public void Dispose()
    {
    }
}
