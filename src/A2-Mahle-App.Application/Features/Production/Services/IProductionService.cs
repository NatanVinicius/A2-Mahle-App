using ProductionAlias = A2MahleApp.Domain.Features.Production.Entities.Production;

namespace A2MahleApp.Application.Features.Production.Services;

public interface IProductionService
{
    ProductionAlias CurrentProduction { get; }

    event EventHandler<ProductionAlias>? ProductionChanged;

    Task InitializeAsync(CancellationToken cancellationToken = default);
}
