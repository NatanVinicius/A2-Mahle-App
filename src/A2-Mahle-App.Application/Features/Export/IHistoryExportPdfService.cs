using A2MahleApp.Application.Features.History.Models;

namespace A2MahleApp.Application.Features.Export;

public interface IHistoryExportPdfService
{
    Task<byte[]> ExportProductionsAsync(
        ProductionHistoryItem? production,
        DateTime? date,
        byte[]? reportImage = null);

    Task<byte[]> ExportInspectionsAsync(
        IReadOnlyCollection<InspectionHistoryItem> inspections,
        DateTime? date,
        byte[]? reportImage = null);
}
