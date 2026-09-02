namespace A2MahleApp.Application.Features.Export;

public interface IHistoryExportPdfService
{
    Task<byte[]> ExportProductionsAsync(
        string htmlContent,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportInspectionsAsync(
        string htmlContent,
        CancellationToken cancellationToken = default);
}
