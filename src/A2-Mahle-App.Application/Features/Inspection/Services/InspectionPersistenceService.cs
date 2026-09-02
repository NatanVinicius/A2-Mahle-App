namespace A2MahleApp.Application.Features.Inspection.Services;

public sealed class InspectionPersistenceService : IDisposable
{
    private readonly IInspectionService _inspectionService;
    private readonly IInspectionRepository _repository;
    private readonly IInspectionEvidenceStorage _evidenceStorage;

    public InspectionPersistenceService(
        IInspectionService inspectionService,
        IInspectionRepository repository,
        IInspectionEvidenceStorage evidenceStorage)
    {
        _inspectionService = inspectionService;
        _repository = repository;
        _evidenceStorage = evidenceStorage;
        _inspectionService.InspectionCompleted += OnInspectionCompleted;
    }

    private async void OnInspectionCompleted(
        object? sender,
        Domain.Features.Inspection.Entities.Inspection inspection)
    {
        if (inspection.Status == Domain.Features.Inspection.Enums.InspectionStatus.Rejected)
        {
            inspection.EvidenceImagePath = await _evidenceStorage.SaveEvidenceAsync(inspection.Image);
        }
        else
        {
            inspection.EvidenceImagePath = null;
        }

        await _repository.AddAsync(inspection);
    }

    public void Dispose()
    {
        _inspectionService.InspectionCompleted -= OnInspectionCompleted;
    }
}
