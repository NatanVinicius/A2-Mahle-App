using A2MahleApp.Application.Features.Inspection.Services;
using A2MahleApp.Application.Features.Inspection.Wiring;
using A2MahleApp.Application.Features.Production.Services;

namespace A2MahleApp.Client;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly InspectionWiring _inspectionWiring;
    private readonly IVisionSensorService _visionSensorService;
    private readonly IProductionService _productionService;
    private readonly InspectionPersistenceService _inspectionPersistenceService;

    public App(
        InspectionWiring inspectionWiring,
        IVisionSensorService visionSensorService,
        IProductionService productionService,
        InspectionPersistenceService inspectionPersistenceService)
    {
        InitializeComponent();

        _inspectionWiring = inspectionWiring;
        _visionSensorService = visionSensorService;
        _productionService = productionService;
        _inspectionPersistenceService = inspectionPersistenceService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        _ = InitializeInspectionAsync();

        Window window = new(new MainPage())
        {
            Title = "A2 Gestamp App"
        };

#if WINDOWS
        window.Width = 1280;
        window.Height = 800;
#endif

        window.Destroying += async (_, _) =>
        {
            try
            {
                await _visionSensorService.DisconnectAsync();
            }
            catch
            {
                // Ignora qualquer erro no encerramento.
            }
        };

        return window;
    }

    private async Task InitializeInspectionAsync()
    {
        await _productionService.InitializeAsync();
        _inspectionWiring.Connect();
        await _visionSensorService.ConnectAsync();
    }
}
