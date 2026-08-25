

public sealed class PopupService : IPopupService
{
  public bool IsCommunicationTestOpen { get; private set; }

  public event Action? OnChange;

  public void OpenCommunicationTest()
  {
    if (IsCommunicationTestOpen)
    {
      return;
    }

    IsCommunicationTestOpen = true;
    OnChange?.Invoke();
  }

  public void CloseCommunicationTest()
  {
    if (!IsCommunicationTestOpen)
    {
      return;
    }

    IsCommunicationTestOpen = false;
    OnChange?.Invoke();
  }
}
