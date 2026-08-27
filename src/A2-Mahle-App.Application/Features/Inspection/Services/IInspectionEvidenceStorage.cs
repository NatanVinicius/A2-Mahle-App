namespace A2MahleApp.Application.Features.Inspection.Services;

public interface IInspectionEvidenceStorage
{
    Task<string> SaveEvidenceAsync(byte[] image, CancellationToken cancellationToken = default);

    Task OpenEvidenceFolderAsync(string evidenceImagePath, CancellationToken cancellationToken = default);
}
