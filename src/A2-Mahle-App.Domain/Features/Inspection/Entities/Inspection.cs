using A2MahleApp.Domain.Features.Inspection.Enums;

namespace A2MahleApp.Domain.Features.Inspection.Entities;

public sealed class Inspection
{
    public int Id { get; set; }

    public DateTime DateTime { get; set; }

    public required byte[] Image { get; set; }

    public required InspectionStatus Status { get; set; }

    public int CycleTimeMilliseconds { get; set; }
}
