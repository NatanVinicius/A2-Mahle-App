using A2MahleApp.Application.Features.History.Services;
using A2MahleApp.Application.Features.Inspection.Services;
using A2MahleApp.Application.Features.Production.Services;
using A2MahleApp.Infrastructure.Features.History.Repositories;
using A2MahleApp.Infrastructure.Features.Inspection.Repositories;
using A2MahleApp.Infrastructure.Features.Inspection.Services;
using A2MahleApp.Infrastructure.Features.Production.Repositories;
using A2MahleApp.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace A2MahleApp.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{

    public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        string databaseDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "db");

        Directory.CreateDirectory(databaseDirectory);

        string databasePath = Path.Combine(
            databaseDirectory,
            "mahle.db");

        string connectionString = $"Data Source={databasePath}";

        services.AddDbContextFactory<MahleDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddSingleton<IVisionSensorService, FakeVisionSensorService>();
        services.AddSingleton<IInspectionEvidenceStorage, InspectionEvidenceStorage>();
        services.AddSingleton<IProductionRepository, ProductionRepository>();
        services.AddSingleton<IInspectionRepository, InspectionRepository>();
        services.AddSingleton<IHistoryRepository, HistoryRepository>();

        return services;
    }
}
