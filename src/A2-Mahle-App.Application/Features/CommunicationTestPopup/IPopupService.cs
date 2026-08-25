

public interface IPopupService
{
  public bool IsCommunicationTestOpen { get; }

  public event Action? OnChange;

  public void OpenCommunicationTest();

  public void CloseCommunicationTest();
}
