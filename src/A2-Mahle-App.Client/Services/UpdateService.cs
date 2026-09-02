using Velopack;
using Velopack.Sources;

namespace A2MahleApp.Client.Services;

public enum UpdateState
{
    Idle,
    Checking,
    NoUpdate,
    Downloading,
    Installing,
    Offline,
    Error
}

public sealed class UpdateService
{
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan MessageDisplayDuration = TimeSpan.FromSeconds(2);

    private readonly UpdateManager _updateManager;
    private UpdateInfo? _update;
    private int _checkStarted;

    public UpdateService()
    {
        _updateManager = new UpdateManager(
            new GithubSource(
                "https://github.com/NatanVinicius/A2-Mahle-App",
                null,
                false));

        CurrentVersion = _updateManager.CurrentVersion?.ToString() ?? "Desconhecida";
    }

    public UpdateState State { get; private set; } = UpdateState.Idle;

    public string CurrentVersion { get; }

    public string? NewVersion { get; private set; }

    public int Progress { get; private set; }

    public string? ErrorMessage { get; private set; }

    public event EventHandler? StateChanged;

    public async Task CheckAndApplyUpdateAsync()
    {
        if (Interlocked.Exchange(ref _checkStarted, 1) != 0)
            return;

        SetState(UpdateState.Checking);

        try
        {
            Task<UpdateInfo?> checkTask = _updateManager.CheckForUpdatesAsync();

            Task completedTask = await Task.WhenAny(
                checkTask,
                Task.Delay(CheckTimeout));

            if (completedTask != checkTask)
            {
                _ = ObserveFailedCheckAsync(checkTask);

                ErrorMessage = "Não foi possível verificar atualizações no tempo limite.";
                await Task.Delay(MessageDisplayDuration);
                SetState(UpdateState.Offline);
                return;
            }

            _update = await checkTask;

            if (_update is null)
            {
                SetState(UpdateState.NoUpdate);
                await Task.Delay(MessageDisplayDuration);
                SetState(UpdateState.Idle);
                return;
            }

            NewVersion = _update.TargetFullRelease.Version.ToString();
            await DownloadAndApplyUpdateAsync(_update);
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.Message;
            await Task.Delay(MessageDisplayDuration);
            SetState(UpdateState.Offline);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            SetState(UpdateState.Error);
            await Task.Delay(MessageDisplayDuration);
            SetState(UpdateState.Idle);
        }
    }

    private async Task DownloadAndApplyUpdateAsync(UpdateInfo update)
    {
        SetState(UpdateState.Downloading);

        await _updateManager.DownloadUpdatesAsync(
            update,
            progress =>
            {
                Progress = progress;
                NotifyStateChanged();
            });

        SetState(UpdateState.Installing);

        _updateManager.ApplyUpdatesAndRestart(
            update.TargetFullRelease);
    }

    private static async Task ObserveFailedCheckAsync(Task checkTask)
    {
        try
        {
            await checkTask;
        }
        catch
        {
            // The check already timed out; its eventual failure must not become unobserved.
        }
    }

    private void SetState(UpdateState state)
    {
        State = state;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
