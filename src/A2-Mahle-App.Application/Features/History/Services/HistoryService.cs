using A2MahleApp.Application.Features.History.Models;
using A2MahleApp.Application.Features.Inspection.Services;

namespace A2MahleApp.Application.Features.History.Services;

public sealed class HistoryService : IHistoryService
{
    private readonly IHistoryRepository _repository;
    private readonly IInspectionEvidenceStorage _evidenceStorage;

    public HistoryService(
        IHistoryRepository repository,
        IInspectionEvidenceStorage evidenceStorage)
    {
        _repository = repository;
        _evidenceStorage = evidenceStorage;
    }

    public Task<IReadOnlyList<ProductionHistoryItem>> GetProductionsAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetProductionsAsync(date, cancellationToken);
    }

    public Task<IReadOnlyList<InspectionHistoryItem>> GetInspectionsAsync(
        DateTime date,
        HistoryJudgmentFilter judgment,
        CancellationToken cancellationToken = default)
    {
        return _repository.GetInspectionsAsync(date, judgment, cancellationToken);
    }

    public Task OpenEvidenceFolderAsync(string evidenceImagePath, CancellationToken cancellationToken = default)
    {
        return _evidenceStorage.OpenEvidenceFolderAsync(evidenceImagePath, cancellationToken);
    }
}
