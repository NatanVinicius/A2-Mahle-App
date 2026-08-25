using System.Net.NetworkInformation;

public class CommunicationTestService : ICommunicationTestService
{

  public async Task<bool> PingAsync(string ip)
  {
    try
    {
      using var ping = new Ping();

      var reply = await ping.SendPingAsync(ip, 1000);

      if (reply.Status == IPStatus.Success)
      {

        return true;
      }

      return false;
    }
    catch (Exception)
    {
      return false;
    }
  }
}
