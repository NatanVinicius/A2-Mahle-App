public interface ICommunicationTestService
{
  public Task<bool> PingAsync(string ip);
}
