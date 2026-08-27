using A2MahleApp.Application.Features.Inspection.Services;
using InspectionEntity = A2MahleApp.Domain.Features.Inspection.Entities.Inspection;

using Microsoft.AspNetCore.Components;

namespace A2MahleApp.Client.Components.Pages.HomePage;

public partial class HomePage : IDisposable
{
    private string _imageSource = "Assets/Images/testimage.bmp";

    [Inject]
    private IInspectionService InspectionService { get; set; } = null!;

    protected override void OnInitialized()
    {
        InspectionService.InspectionCompleted += OnInspectionCompleted;

        if (InspectionService.CurrentInspection is not null)
        {
            _imageSource = ToImageSource(InspectionService.CurrentInspection);
        }
    }

    private void OnInspectionCompleted(object? sender, InspectionEntity inspection)
    {
        _imageSource = ToImageSource(inspection);
        _ = InvokeAsync(StateHasChanged);
    }

    private static string ToImageSource(InspectionEntity inspection)
    {
        return $"data:image/bmp;base64,{Convert.ToBase64String(inspection.Image)}";
    }

    public void Dispose()
    {
        InspectionService.InspectionCompleted -= OnInspectionCompleted;
    }
}
