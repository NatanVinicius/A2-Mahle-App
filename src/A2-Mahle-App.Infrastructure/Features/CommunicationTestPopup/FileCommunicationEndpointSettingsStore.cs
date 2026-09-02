using System.Text.Json;

namespace A2MahleApp.Infrastructure.Features.CommunicationTestPopup;

public sealed class FileCommunicationEndpointSettingsStore : ICommunicationEndpointSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _settingsFilePath;
    private readonly object _sync = new();

    public FileCommunicationEndpointSettingsStore()
    {
        string settingsDirectory = Path.Combine(AppContext.BaseDirectory, "Data", "settings");
        Directory.CreateDirectory(settingsDirectory);

        _settingsFilePath = Path.Combine(settingsDirectory, "communication-endpoints.json");
    }

    public ComunicationTestServiceModel? Load()
    {
        lock (_sync)
        {
            if (!File.Exists(_settingsFilePath))
            {
                return null;
            }

            string json = File.ReadAllText(_settingsFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ComunicationTestServiceModel>(json, JsonOptions);
        }
    }

    public void Save(ComunicationTestServiceModel settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        lock (_sync)
        {
            string json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);
        }
    }
}
