using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace A2MahleApp.Client.Components.Shared.ComunicationTestPopup;

public partial class ComunicationTestPopup
{
  [Inject]
  private IPopupService PopupService { get; set; } = default!;

  [Inject]
  private IOptions<ComunicationTestServiceModel> ComunicationTestServiceModel { get; set; } = default!;

  [Inject]
  private ICommunicationTestService CommunicationTestService { get; set; } = default!;

  private bool? HostIsConnected;
  private bool? IV4IsConnected;

  private bool IsLoading;

  protected override void OnInitialized()
  {
    PopupService.OnChange += HandlePopupChanged;
  }

  private async Task TestConnections()
  {
    if (IsLoading)
    {
      return;
    }

    IsLoading = true;
    ResetCommunication();

    await InvokeAsync(StateHasChanged);

    try
    {
      var settings = ComunicationTestServiceModel.Value;

      HostIsConnected =
          await CommunicationTestService.PingAsync(settings.Host);

      IV4IsConnected =
          await CommunicationTestService.PingAsync(settings.IV4);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
    }
    finally
    {
      IsLoading = false;

      // Renderiza os resultados depois que TODOS terminaram
      await InvokeAsync(StateHasChanged);
    }
  }

  private static string GetIconType(bool? status)
  {
    return status switch
    {
      true => "text-green-500",
      false => "text-red-500",
      _ => "text-primary"
    };
  }

  private void ResetCommunication()
  {
    HostIsConnected = null;
    IV4IsConnected = null;
  }

  private void HandlePopupChanged()
  {
    _ = InvokeAsync(StateHasChanged);
  }

  private void OnClose()
  {
    if (IsLoading)
    {
      return;
    }

    ResetCommunication();

    PopupService.CloseCommunicationTest();
  }

  public void Dispose()
  {
    PopupService.OnChange -= HandlePopupChanged;
  }
}
