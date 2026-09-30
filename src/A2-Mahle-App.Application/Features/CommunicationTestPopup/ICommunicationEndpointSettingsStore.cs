public interface ICommunicationEndpointSettingsStore
{
    ComunicationTestServiceModel? Load();

    void Save(ComunicationTestServiceModel settings);
}
