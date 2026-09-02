using Microsoft.Extensions.Logging;

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
    private readonly ILogger<UpdateService> _logger;
    private UpdateInfo? _update;
    private int _checkStarted;

    public UpdateService(ILogger<UpdateService> logger)
    {
        _logger = logger;

        _updateManager = new UpdateManager(
            new GithubSource(
                "https://github.com/NatanVinicius/A2-Mahle-App",
                null,
                false));

        CurrentVersion = _updateManager.CurrentVersion?.ToString() ?? "Desconhecida";

        _logger.LogInformation(
            "Update service initialized. Current version: {CurrentVersion}",
            CurrentVersion);
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
        {
            _logger.LogDebug("Update check skipped because another check has already started.");
            return;
        }

        _logger.LogInformation("Starting update check.");
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
                SetState(UpdateState.Offline);
                _logger.LogWarning(
                    "Update check timed out after {TimeoutSeconds}s.",
                    CheckTimeout.TotalSeconds);
                await Task.Delay(MessageDisplayDuration);
                SetState(UpdateState.Idle);
                return;
            }

            _update = await checkTask;

            if (_update is null)
            {
                _logger.LogInformation("No updates available.");
                SetState(UpdateState.NoUpdate);
                await Task.Delay(MessageDisplayDuration);
                SetState(UpdateState.Idle);
                return;
            }

            NewVersion = _update.TargetFullRelease.Version.ToString();
            _logger.LogInformation(
                "Update found. Current version: {CurrentVersion}. New version: {NewVersion}",
                CurrentVersion,
                NewVersion);
            await DownloadAndApplyUpdateAsync(_update);
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.Message;
            SetState(UpdateState.Error);
            _logger.LogWarning(ex, "Update check failed due to HTTP/network error.");
            await Task.Delay(MessageDisplayDuration);
            SetState(UpdateState.Idle);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            SetState(UpdateState.Error);
            _logger.LogError(ex, "Unexpected error while checking/applying updates.");
            await Task.Delay(MessageDisplayDuration);
            SetState(UpdateState.Idle);
        }
    }

    private async Task DownloadAndApplyUpdateAsync(UpdateInfo update)
    {
        _logger.LogInformation(
            "Starting update download for version {NewVersion}.",
            update.TargetFullRelease.Version.ToString());

        SetState(UpdateState.Downloading);

        await _updateManager.DownloadUpdatesAsync(
            update,
            progress =>
            {
                Progress = progress;
                if (progress % 25 == 0)
                {
                    _logger.LogInformation("Update download progress: {ProgressPercent}%.", progress);
                }

                NotifyStateChanged();
            });

        SetState(UpdateState.Installing);
        _logger.LogInformation(
            "Update downloaded successfully. Applying version {NewVersion} and restarting app.",
            update.TargetFullRelease.Version.ToString());

        _updateManager.ApplyUpdatesAndRestart(
            update.TargetFullRelease);
    }

    private async Task ObserveFailedCheckAsync(Task checkTask)
    {
        try
        {
            await checkTask;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(
                ex,
                "Update check completed with failure after timeout; failure observed to avoid unobserved task exceptions.");
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
