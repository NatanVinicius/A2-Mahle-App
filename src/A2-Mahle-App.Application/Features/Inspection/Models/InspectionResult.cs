using A2MahleApp.Domain.Features.Inspection.Enums;

namespace A2MahleApp.Application.Features.Inspection.Models;

public sealed class InspectionResult
{
    public required InspectionStatus Status { get; init; }

    public int CycleTimeMilliseconds { get; init; }
}
