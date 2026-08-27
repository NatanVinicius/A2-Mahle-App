namespace A2MahleApp.Application.Features.Inspection.Services;

public sealed class InspectionPersistenceService : IDisposable
{
    private readonly IInspectionService _inspectionService;
    private readonly IInspectionRepository _repository;

    public InspectionPersistenceService(
        IInspectionService inspectionService,
        IInspectionRepository repository)
    {
        _inspectionService = inspectionService;
        _repository = repository;
        _inspectionService.InspectionCompleted += OnInspectionCompleted;
    }

    private async void OnInspectionCompleted(
        object? sender,
        Domain.Features.Inspection.Entities.Inspection inspection)
    {
        await _repository.AddAsync(inspection);
    }

    public void Dispose()
    {
        _inspectionService.InspectionCompleted -= OnInspectionCompleted;
    }
}
