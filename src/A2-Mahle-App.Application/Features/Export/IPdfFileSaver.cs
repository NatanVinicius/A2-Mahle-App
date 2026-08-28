namespace A2MahleApp.Application.Features.Export;

public interface IPdfFileSaver
{
    Task SaveAsync(
        byte[] content,
        string suggestedFileName);
}
