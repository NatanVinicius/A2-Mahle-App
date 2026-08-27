using A2MahleApp.Application.Features.History.Models;

namespace A2MahleApp.Application.Features.History.Services;

public sealed class HistoryService : IHistoryService
{
    private readonly IHistoryRepository _repository;

    public HistoryService(IHistoryRepository repository)
    {
        _repository = repository;
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
}
