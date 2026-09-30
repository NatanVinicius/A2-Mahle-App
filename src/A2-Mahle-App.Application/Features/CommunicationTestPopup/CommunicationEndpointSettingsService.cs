using Microsoft.Extensions.Options;

public sealed class CommunicationEndpointSettingsService : ICommunicationEndpointSettingsService
{
    private readonly ICommunicationEndpointSettingsStore _store;
    private ComunicationTestServiceModel _current;

    public CommunicationEndpointSettingsService(
        ICommunicationEndpointSettingsStore store,
        IOptions<ComunicationTestServiceModel> defaults)
    {
        _store = store;

        ComunicationTestServiceModel fallback = new()
        {
            Host = defaults.Value.Host?.Trim() ?? string.Empty,
            IV4 = defaults.Value.IV4?.Trim() ?? string.Empty
        };

        ComunicationTestServiceModel? persisted = _store.Load();

        _current = persisted is null
            ? fallback
            : new ComunicationTestServiceModel
            {
                Host = string.IsNullOrWhiteSpace(persisted.Host) ? fallback.Host : persisted.Host.Trim(),
                IV4 = string.IsNullOrWhiteSpace(persisted.IV4) ? fallback.IV4 : persisted.IV4.Trim()
            };
    }

    public ComunicationTestServiceModel Current =>
        new()
        {
            Host = _current.Host,
            IV4 = _current.IV4
        };

    public event Action? OnChange;

    public void Update(string host, string iv4)
    {
        string normalizedHost = host.Trim();
        string normalizedIv4 = iv4.Trim();

        _current = new ComunicationTestServiceModel
        {
            Host = normalizedHost,
            IV4 = normalizedIv4
        };

        _store.Save(_current);

        OnChange?.Invoke();
    }
}
