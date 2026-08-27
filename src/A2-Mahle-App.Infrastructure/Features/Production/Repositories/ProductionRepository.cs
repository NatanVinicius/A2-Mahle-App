using A2MahleApp.Application.Features.Production.Services;
using ProductionAlias = A2MahleApp.Domain.Features.Production.Entities.Production;
using A2MahleApp.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace A2MahleApp.Infrastructure.Features.Production.Repositories;

public sealed class ProductionRepository : IProductionRepository
{
    private readonly IDbContextFactory<MahleDbContext> _contextFactory;
    private Task? _databaseInitialization;

    public ProductionRepository(IDbContextFactory<MahleDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ProductionAlias?> GetByDateAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);

        await using MahleDbContext context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Productions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Date == date.Date, cancellationToken);
    }

    public async Task SaveAsync(
        ProductionAlias production,
        CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);

        await using MahleDbContext context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        ProductionAlias? existing = await context.Productions
            .SingleOrDefaultAsync(x => x.Date == production.Date.Date, cancellationToken);

        if (existing is null)
        {
            context.Productions.Add(production);
        }
        else
        {
            existing.Produced = production.Produced;
            existing.Approved = production.Approved;
            existing.Rejected = production.Rejected;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private Task EnsureDatabaseAsync(CancellationToken cancellationToken)
    {
        if (_databaseInitialization is null)
        {
            _databaseInitialization = InitializeDatabaseAsync(cancellationToken);
        }

        return _databaseInitialization;
    }

    private async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        await using MahleDbContext context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.EnsureCreatedAsync(cancellationToken);
    }
}
