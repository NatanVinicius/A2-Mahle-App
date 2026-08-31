public interface ICommunicationEndpointSettingsService
{
    ComunicationTestServiceModel Current { get; }

    event Action? OnChange;

    void Update(string host, string iv4);
}
