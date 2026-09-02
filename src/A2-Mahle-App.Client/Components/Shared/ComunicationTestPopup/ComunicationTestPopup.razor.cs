using Microsoft.AspNetCore.Components;

using System.Net;

using A2MahleApp.Application.Features.Inspection.Services;

namespace A2MahleApp.Client.Components.Shared.ComunicationTestPopup;

public partial class ComunicationTestPopup
{
  [Inject]
  private IPopupService PopupService { get; set; } = default!;

  [Inject]
  private ICommunicationEndpointSettingsService CommunicationEndpointSettingsService { get; set; } = default!;

  [Inject]
  private ICommunicationTestService CommunicationTestService { get; set; } = default!;

  [Inject]
  private IVisionSensorService VisionSensorService { get; set; } = default!;

  private bool? HostIsConnected;
  private bool? IV4IsConnected;

  private bool IsLoading;
  private bool IsSaving;

  private string HostInput = string.Empty;
  private string IV4Input = string.Empty;
  private string? FeedbackMessage;

  protected override void OnInitialized()
  {
    PopupService.OnChange += HandlePopupChanged;
    LoadSettingsIntoInputs();
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
      FeedbackMessage = null;

      HostIsConnected =
          await CommunicationTestService.PingAsync(HostInput.Trim());

      IV4IsConnected =
          await CommunicationTestService.PingAsync(IV4Input.Trim());
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

  private async Task SaveSettingsAsync()
  {
    if (IsLoading || IsSaving)
    {
      return;
    }

    string host = HostInput.Trim();
    string iv4 = IV4Input.Trim();

    if (!IPAddress.TryParse(host, out _))
    {
      FeedbackMessage = "Informe um IP válido para HOST.";
      await InvokeAsync(StateHasChanged);
      return;
    }

    if (!IPAddress.TryParse(iv4, out _))
    {
      FeedbackMessage = "Informe um IP válido para IV4/CV-X.";
      await InvokeAsync(StateHasChanged);
      return;
    }

    try
    {
      IsSaving = true;
      FeedbackMessage = null;

      CommunicationEndpointSettingsService.Update(host, iv4);
      await VisionSensorService.ReconnectAsync();

      FeedbackMessage = "Configurações salvas com sucesso.";
    }
    catch (Exception ex)
    {
      FeedbackMessage = $"Falha ao salvar configurações: {ex.Message}";
    }
    finally
    {
      IsSaving = false;
      await InvokeAsync(StateHasChanged);
    }
  }

  private void LoadSettingsIntoInputs()
  {
    ComunicationTestServiceModel settings = CommunicationEndpointSettingsService.Current;
    HostInput = settings.Host;
    IV4Input = settings.IV4;
  }

  private void HandlePopupChanged()
  {
    if (PopupService.IsCommunicationTestOpen)
    {
      LoadSettingsIntoInputs();
      FeedbackMessage = null;
      ResetCommunication();
    }

    _ = InvokeAsync(StateHasChanged);
  }

  private void OnClose()
  {
    if (IsLoading || IsSaving)
    {
      return;
    }

    ResetCommunication();
    FeedbackMessage = null;

    PopupService.CloseCommunicationTest();
  }

  public void Dispose()
  {
    PopupService.OnChange -= HandlePopupChanged;
  }
}
