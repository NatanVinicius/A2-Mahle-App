using A2MahleApp.Application.Features.History.Services;
using A2MahleApp.Application.Features.Inspection.Correlation;
using A2MahleApp.Application.Features.Inspection.Services;
using A2MahleApp.Application.Features.Inspection.Wiring;
using A2MahleApp.Application.Features.Production.Services;

using Microsoft.Extensions.DependencyInjection;

namespace A2MahleApp.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IPopupService, PopupService>();
        services.AddSingleton<ICommunicationTestService, CommunicationTestService>();
        services.AddSingleton<ICommunicationEndpointSettingsStore, NoOpCommunicationEndpointSettingsStore>();
        services.AddSingleton<ICommunicationEndpointSettingsService, CommunicationEndpointSettingsService>();

        services.AddSingleton<InspectionCorrelation>();
        services.AddSingleton<IInspectionService, InspectionService>();
        services.AddSingleton<InspectionPersistenceService>();
        services.AddSingleton<IProductionService, ProductionService>();
        services.AddSingleton<IHistoryService, HistoryService>();
        services.AddSingleton<InspectionWiring>();

        return services;
    }
}
