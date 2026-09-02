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
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(10);

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

        DebugLog("Check iniciado.");

        try
        {
            DebugLog("Chamando CheckForUpdatesAsync().");

            Task<UpdateInfo?> checkTask =
                _updateManager.CheckForUpdatesAsync();

            DebugLog("Task de CheckForUpdatesAsync criada.");

            Task completedTask = await Task.WhenAny(
                checkTask,
                Task.Delay(CheckTimeout));

            if (completedTask != checkTask)
            {
                DebugLog("TIMEOUT: CheckForUpdatesAsync não terminou em 10 segundos.");

                State = UpdateState.Offline;
                ErrorMessage = "Não foi possível verificar atualizações no tempo limite.";

                NotifyStateChanged();
                return;
            }

            DebugLog("CheckForUpdatesAsync terminou.");

            _update = await checkTask;

            if (_update is null)
            {
                DebugLog("Nenhuma atualização encontrada.");

                State = UpdateState.Idle;
                NotifyStateChanged();
                return;
            }

            NewVersion = _update.TargetFullRelease.Version.ToString();

            DebugLog($"Atualização encontrada: {NewVersion}.");

            State = UpdateState.UpdateAvailable;
        }
        catch (HttpRequestException ex)
        {
            DebugLog($"HttpRequestException: {ex}");

            ErrorMessage = ex.Message;
            State = UpdateState.Offline;
        }
        catch (Exception ex)
        {
            DebugLog($"Exception: {ex}");

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

    private static void DebugLog(string message)
    {
        try
        {
            string directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "A2-Mahle-App");

            Directory.CreateDirectory(directory);

            string path = Path.Combine(directory, "update-debug.log");

            File.AppendAllText(
                path,
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}{Environment.NewLine}");
        }
        catch
        {
            // O log de diagnóstico nunca pode afetar a aplicação.
        }
    }
}