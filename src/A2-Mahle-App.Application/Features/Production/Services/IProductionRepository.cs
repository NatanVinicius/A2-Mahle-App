using ProductionAlias = A2MahleApp.Domain.Features.Production.Entities.Production;

namespace A2MahleApp.Application.Features.Production.Services;

public interface IProductionRepository
{
    Task<ProductionAlias?> GetByDateAsync(
        DateTime date,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        ProductionAlias production,
        CancellationToken cancellationToken = default);
}