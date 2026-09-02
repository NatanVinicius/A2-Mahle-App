using System.Diagnostics;

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

        Stopwatch stopwatch = Stopwatch.StartNew();
        _logger.LogInformation(
            "Starting update check. CurrentVersion={CurrentVersion}",
            CurrentVersion);
        SetState(UpdateState.Checking);

        try
        {
            _update = await _updateManager.CheckForUpdatesAsync();
            stopwatch.Stop();
            _logger.LogInformation(
                "Update check request completed. ElapsedMs={ElapsedMs}",
                stopwatch.ElapsedMilliseconds);

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
            ErrorMessage = "Não foi possível acessar o servidor de atualização.";
            SetState(UpdateState.Offline);
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
        Stopwatch downloadStopwatch = Stopwatch.StartNew();
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
                else
                {
                    _logger.LogDebug("Update download progress tick: {ProgressPercent}%.", progress);
                }

                NotifyStateChanged();
            });

        SetState(UpdateState.Installing);
        downloadStopwatch.Stop();
        _logger.LogInformation(
            "Update downloaded successfully. NewVersion={NewVersion}, ElapsedMs={ElapsedMs}. Applying and restarting app.",
            update.TargetFullRelease.Version.ToString(),
            downloadStopwatch.ElapsedMilliseconds);

        _updateManager.ApplyUpdatesAndRestart(
            update.TargetFullRelease);
    }

    private void SetState(UpdateState state)
    {
        if (State != state)
        {
            _logger.LogInformation("Update state transition: {FromState} -> {ToState}", State, state);
        }

        State = state;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
