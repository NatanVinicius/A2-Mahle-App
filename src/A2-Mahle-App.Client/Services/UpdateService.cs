using Velopack;
using Velopack.Sources;

namespace A2MahleApp.Client.Services;

public enum UpdateState
{
    Idle,
    Checking,
    UpdateAvailable,
    Downloading,
    Ready,
    Installing,
    Offline,
    Error
}

public sealed class UpdateService
{
    private readonly UpdateManager _updateManager;
    private UpdateInfo? _update;

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

    public async Task CheckForUpdateAsync()
    {
        State = UpdateState.Checking;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            _update = await _updateManager.CheckForUpdatesAsync();

            if (_update is null)
            {
                State = UpdateState.Idle;
                NotifyStateChanged();
                return;
            }

            NewVersion = _update.TargetFullRelease.Version.ToString();
            State = UpdateState.UpdateAvailable;
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.Message;
            State = UpdateState.Offline;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = UpdateState.Error;
        }

        NotifyStateChanged();
    }

    public async Task DownloadUpdateAsync()
    {
        if (_update is null)
            return;

        State = UpdateState.Downloading;
        Progress = 0;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            await _updateManager.DownloadUpdatesAsync(
                _update,
                progress =>
                {
                    Progress = progress;
                    NotifyStateChanged();
                });

            State = UpdateState.Ready;
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.Message;
            State = UpdateState.Offline;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            State = UpdateState.Error;
        }

        NotifyStateChanged();
    }

    public void InstallUpdate()
    {
        if (_update is null)
            return;

        State = UpdateState.Installing;
        NotifyStateChanged();

        _updateManager.ApplyUpdatesAndRestart(
            _update.TargetFullRelease);
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
