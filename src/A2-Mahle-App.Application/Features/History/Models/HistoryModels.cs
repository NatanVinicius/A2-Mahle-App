using A2MahleApp.Domain.Features.Inspection.Enums;

namespace A2MahleApp.Application.Features.History.Models;

public enum HistoryViewMode
{
    Production,
    Inspection
}

public enum HistoryJudgmentFilter
{
    All,
    Approved,
    Rejected
}

public sealed class ProductionHistoryItem
{
    public DateTime Date { get; init; }
    public int Produced { get; init; }
    public int Approved { get; init; }
    public int Rejected { get; init; }
    public double RejectRate => Produced == 0 ? 0 : Rejected * 100.0 / Produced;
}

public sealed class InspectionHistoryItem
{
    public DateTime DateTime { get; init; }
    public InspectionStatus Status { get; init; }
    public int CycleTimeMilliseconds { get; init; }
    public byte[] Image { get; init; } = [];
}
