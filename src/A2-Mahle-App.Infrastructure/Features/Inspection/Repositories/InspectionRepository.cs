using A2MahleApp.Application.Features.Inspection.Services;
using InspectionEntity = A2MahleApp.Domain.Features.Inspection.Entities.Inspection;
using A2MahleApp.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace A2MahleApp.Infrastructure.Features.Inspection.Repositories;

public sealed class InspectionRepository : IInspectionRepository
{
    private readonly IDbContextFactory<MahleDbContext> _contextFactory;

    public InspectionRepository(IDbContextFactory<MahleDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task AddAsync(
        InspectionEntity inspection,
        CancellationToken cancellationToken = default)
    {
        await using MahleDbContext context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        context.Inspections.Add(inspection);
        await context.SaveChangesAsync(cancellationToken);
    }
}
