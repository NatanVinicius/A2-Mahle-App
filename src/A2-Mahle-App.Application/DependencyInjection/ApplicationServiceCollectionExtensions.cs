using Microsoft.Extensions.DependencyInjection;

namespace A2MahleApp.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
  public static IServiceCollection AddApplication(
      this IServiceCollection services)
  {
    services.AddSingleton<IPopupService, PopupService>();
    services.AddSingleton<ICommunicationTestService, CommunicationTestService>();
    return services;
  }
}
