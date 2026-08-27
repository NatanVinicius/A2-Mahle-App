using System.Diagnostics;

using A2MahleApp.Application.Features.Inspection.Services;

namespace A2MahleApp.Infrastructure.Features.Inspection.Services;

public sealed class InspectionEvidenceStorage : IInspectionEvidenceStorage
{
    private static readonly string EvidenceRootRelativePath = Path.Combine("Assets", "Images", "Evidences");

    public async Task<string> SaveEvidenceAsync(byte[] image, CancellationToken cancellationToken = default)
    {
        string timestamp = $"{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}";
        string relativeDirectory = Path.Combine(EvidenceRootRelativePath, timestamp);
        string absoluteDirectory = Path.Combine(AppContext.BaseDirectory, relativeDirectory);

        Directory.CreateDirectory(absoluteDirectory);

        string absoluteFilePath = Path.Combine(absoluteDirectory, "image.jpg");
        await File.WriteAllBytesAsync(absoluteFilePath, image, cancellationToken);

        return Path.Combine(relativeDirectory, "image.jpg").Replace('\\', '/');
    }

    public Task OpenEvidenceFolderAsync(string evidenceImagePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(evidenceImagePath))
        {
            throw new ArgumentException("Evidence path must be informed.", nameof(evidenceImagePath));
        }

        cancellationToken.ThrowIfCancellationRequested();

        string absoluteImagePath = Path.Combine(
            AppContext.BaseDirectory,
            evidenceImagePath.Replace('/', Path.DirectorySeparatorChar));

        string? absoluteDirectory = Path.GetDirectoryName(absoluteImagePath);

        if (string.IsNullOrWhiteSpace(absoluteDirectory) || !Directory.Exists(absoluteDirectory))
        {
            throw new DirectoryNotFoundException($"Evidence directory was not found: '{evidenceImagePath}'.");
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{absoluteDirectory}\"",
            UseShellExecute = true
        });

        return Task.CompletedTask;
    }
}
