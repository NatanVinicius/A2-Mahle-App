using A2MahleApp.Application.Features.History.Models;
using A2MahleApp.Application.Features.History.Services;
using A2MahleApp.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace A2MahleApp.Infrastructure.Features.History.Repositories;

public sealed class HistoryRepository : IHistoryRepository
{
    private readonly IDbContextFactory<MahleDbContext> _contextFactory;

    public HistoryRepository(IDbContextFactory<MahleDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<ProductionHistoryItem>> GetProductionsAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        await using MahleDbContext context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        DateTime start = date.Date;
        DateTime end = start.AddDays(1);

        return await context.Productions.AsNoTracking()
            .Where(x => x.Date >= start && x.Date < end)
            .OrderByDescending(x => x.Date)
            .Select(x => new ProductionHistoryItem { Date = x.Date, Produced = x.Produced, Approved = x.Approved, Rejected = x.Rejected })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InspectionHistoryItem>> GetInspectionsAsync(DateTime date, HistoryJudgmentFilter judgment, CancellationToken cancellationToken = default)
    {
        await using MahleDbContext context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        DateTime start = date.Date;
        DateTime end = start.AddDays(1);

        IQueryable<Domain.Features.Inspection.Entities.Inspection> query = context.Inspections.AsNoTracking()
            .Where(x => x.DateTime >= start && x.DateTime < end);

        if (judgment == HistoryJudgmentFilter.Approved)
        {
            query = query.Where(x => x.Status == Domain.Features.Inspection.Enums.InspectionStatus.Approved);
        }
        else if (judgment == HistoryJudgmentFilter.Rejected)
        {
            query = query.Where(x => x.Status == Domain.Features.Inspection.Enums.InspectionStatus.Rejected);
        }

        return await query.OrderByDescending(x => x.DateTime)
            .Select(x => new InspectionHistoryItem
            {
                DateTime = x.DateTime,
                Status = x.Status,
                CycleTimeMilliseconds = x.CycleTimeMilliseconds,
                EvidenceImagePath = x.EvidenceImagePath
            })
            .ToListAsync(cancellationToken);
    }
}
