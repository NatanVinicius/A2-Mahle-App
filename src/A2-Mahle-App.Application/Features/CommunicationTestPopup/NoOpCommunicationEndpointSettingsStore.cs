public sealed class NoOpCommunicationEndpointSettingsStore : ICommunicationEndpointSettingsStore
{
    public ComunicationTestServiceModel? Load()
    {
        return null;
    }

    public void Save(ComunicationTestServiceModel settings)
    {
    }
}
