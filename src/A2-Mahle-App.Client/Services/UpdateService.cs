using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;

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
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(120);
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
            "Starting update check. CurrentVersion={CurrentVersion}, TimeoutSeconds={TimeoutSeconds}",
            CurrentVersion,
            CheckTimeout.TotalSeconds);
        SetState(UpdateState.Checking);

        try
        {
            Task<UpdateInfo?> checkTask = _updateManager.CheckForUpdatesAsync();

            Task completedTask = await Task.WhenAny(
                checkTask,
                Task.Delay(CheckTimeout));

            if (completedTask != checkTask)
            {
                _ = ObserveTimedOutCheckCompletionAsync(checkTask, stopwatch.ElapsedMilliseconds);
                _ = ProbeUpdateEndpointsAsync();

                ErrorMessage = "A verificação está demorando mais que o esperado e continuará em segundo plano.";
                SetState(UpdateState.Error);
                stopwatch.Stop();
                _logger.LogWarning(
                    "Update check timed out. ElapsedMs={ElapsedMs}, TimeoutSeconds={TimeoutSeconds}",
                    stopwatch.ElapsedMilliseconds,
                    CheckTimeout.TotalSeconds);
                await Task.Delay(MessageDisplayDuration);
                SetState(UpdateState.Idle);
                return;
            }

            _update = await checkTask;
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

    private async Task ObserveTimedOutCheckCompletionAsync(Task<UpdateInfo?> checkTask, long timeoutElapsedMs)
    {
        Stopwatch tailStopwatch = Stopwatch.StartNew();

        try
        {
            UpdateInfo? lateResult = await checkTask;
            tailStopwatch.Stop();
            long totalElapsedMs = timeoutElapsedMs + tailStopwatch.ElapsedMilliseconds;

            if (lateResult is null)
            {
                _logger.LogWarning(
                    "Timed-out update check eventually completed with NO update. InitialTimeoutElapsedMs={TimeoutElapsedMs}, AdditionalElapsedMs={AdditionalElapsedMs}, TotalElapsedMs={TotalElapsedMs}",
                    timeoutElapsedMs,
                    tailStopwatch.ElapsedMilliseconds,
                    totalElapsedMs);
                return;
            }

            NewVersion = lateResult.TargetFullRelease.Version.ToString();
            _logger.LogWarning(
                "Timed-out update check eventually completed with update available. InitialTimeoutElapsedMs={TimeoutElapsedMs}, AdditionalElapsedMs={AdditionalElapsedMs}, TotalElapsedMs={TotalElapsedMs}, NewVersion={NewVersion}",
                timeoutElapsedMs,
                tailStopwatch.ElapsedMilliseconds,
                totalElapsedMs,
                NewVersion);

            await DownloadAndApplyUpdateAsync(lateResult);
        }
        catch (Exception ex)
        {
            tailStopwatch.Stop();
            _logger.LogDebug(
                ex,
                "Timed-out update check eventually failed. InitialTimeoutElapsedMs={TimeoutElapsedMs}, AdditionalElapsedMs={AdditionalElapsedMs}, TotalElapsedMs={TotalElapsedMs}",
                timeoutElapsedMs,
                tailStopwatch.ElapsedMilliseconds,
                timeoutElapsedMs + tailStopwatch.ElapsedMilliseconds);
        }
    }

    private async Task ProbeUpdateEndpointsAsync()
    {
        try
        {
            using HttpClient httpClient = new()
            {
                Timeout = CheckTimeout
            };

            httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("A2MahleApp", CurrentVersion));

            const string latestReleaseApiUrl =
                "https://api.github.com/repos/NatanVinicius/A2-Mahle-App/releases/latest";

            Stopwatch latestApiStopwatch = Stopwatch.StartNew();
            HttpResponseMessage latestResponse = await httpClient.GetAsync(latestReleaseApiUrl);
            latestApiStopwatch.Stop();

            _logger.LogWarning(
                "Update endpoint probe: releases/latest status={StatusCode} elapsedMs={ElapsedMs}",
                (int)latestResponse.StatusCode,
                latestApiStopwatch.ElapsedMilliseconds);

            latestResponse.EnsureSuccessStatusCode();

            await using Stream latestStream = await latestResponse.Content.ReadAsStreamAsync();
            using JsonDocument latestJson = await JsonDocument.ParseAsync(latestStream);

            string? releasesWinJsonUrl = null;
            string? releasesFileUrl = null;

            foreach (JsonElement asset in latestJson.RootElement.GetProperty("assets").EnumerateArray())
            {
                string? assetName = asset.GetProperty("name").GetString();
                string? assetUrl = asset.GetProperty("browser_download_url").GetString();

                if (string.Equals(assetName, "releases.win.json", StringComparison.OrdinalIgnoreCase))
                {
                    releasesWinJsonUrl = assetUrl;
                }

                if (string.Equals(assetName, "RELEASES", StringComparison.OrdinalIgnoreCase))
                {
                    releasesFileUrl = assetUrl;
                }
            }

            if (!string.IsNullOrWhiteSpace(releasesWinJsonUrl))
            {
                await ProbeSingleEndpointAsync(httpClient, "releases.win.json", releasesWinJsonUrl);
            }
            else
            {
                _logger.LogWarning("Update endpoint probe: releases.win.json asset not found in latest release.");
            }

            if (!string.IsNullOrWhiteSpace(releasesFileUrl))
            {
                await ProbeSingleEndpointAsync(httpClient, "RELEASES", releasesFileUrl);
            }
            else
            {
                _logger.LogWarning("Update endpoint probe: RELEASES asset not found in latest release.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Update endpoint probe failed.");
        }
    }

    private async Task ProbeSingleEndpointAsync(HttpClient httpClient, string assetName, string url)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        using HttpResponseMessage response = await httpClient.GetAsync(url);
        string body = await response.Content.ReadAsStringAsync();
        stopwatch.Stop();

        _logger.LogWarning(
            "Update endpoint probe: asset={AssetName} status={StatusCode} elapsedMs={ElapsedMs} contentLength={ContentLength}",
            assetName,
            (int)response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            body.Length);
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
