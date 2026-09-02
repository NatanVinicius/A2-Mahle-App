using A2MahleApp.Application.Features.History.Models;

namespace A2MahleApp.Application.Features.History.Services;

public interface IHistoryService
{
    Task<IReadOnlyList<ProductionHistoryItem>> GetProductionsAsync(DateTime date, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InspectionHistoryItem>> GetInspectionsAsync(
        DateTime date,
        HistoryJudgmentFilter judgment,
        CancellationToken cancellationToken = default);

    Task OpenEvidenceFolderAsync(string evidenceImagePath, CancellationToken cancellationToken = default);
}
