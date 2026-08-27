using A2MahleApp.Application.Features.History.Models;
using A2MahleApp.Application.Features.History.Services;

using Microsoft.AspNetCore.Components;

namespace A2MahleApp.Client.Components.Pages.HistoryPage;

public partial class HistoryPage
{
    private DateTime _selectedDate = DateTime.Today;
    private HistoryViewMode _viewMode = HistoryViewMode.Production;
    private HistoryJudgmentFilter _judgment = HistoryJudgmentFilter.All;
    private IReadOnlyList<ProductionHistoryItem> _productions = [];
    private IReadOnlyList<InspectionHistoryItem> _inspections = [];
    private bool _loading;

    [Inject]
    private IHistoryService HistoryService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;

        try
        {
            if (_viewMode == HistoryViewMode.Production)
            {
                _productions = await HistoryService.GetProductionsAsync(_selectedDate);
                _inspections = [];
            }
            else
            {
                _inspections = await HistoryService.GetInspectionsAsync(_selectedDate, _judgment);
                _productions = [];
            }
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task OnDateChanged(ChangeEventArgs args)
    {
        if (DateTime.TryParse(args.Value?.ToString(), out DateTime date))
        {
            _selectedDate = date.Date;
            await LoadAsync();
        }
    }

    private async Task OnJudgmentChanged(ChangeEventArgs args)
    {
        _judgment = args.Value?.ToString() switch
        {
            "Approved" => HistoryJudgmentFilter.Approved,
            "Rejected" => HistoryJudgmentFilter.Rejected,
            _ => HistoryJudgmentFilter.All
        };

        if (_viewMode == HistoryViewMode.Inspection)
        {
            await LoadAsync();
        }
    }

    private async Task OnViewModeChanged(HistoryViewMode mode)
    {
        _viewMode = mode;
        await LoadAsync();
    }

    private static string JudgmentText(A2MahleApp.Domain.Features.Inspection.Enums.InspectionStatus status) =>
        status == A2MahleApp.Domain.Features.Inspection.Enums.InspectionStatus.Approved
            ? "Aprovada"
            : "Reprovada";

    private static string JudgmentCss(A2MahleApp.Domain.Features.Inspection.Enums.InspectionStatus status) =>
        status == A2MahleApp.Domain.Features.Inspection.Enums.InspectionStatus.Approved
            ? "text-green-500"
            : "text-red-500";

    private async Task OpenEvidenceAsync(InspectionHistoryItem inspection)
    {
        if (string.IsNullOrWhiteSpace(inspection.EvidenceImagePath))
        {
            return;
        }

        await HistoryService.OpenEvidenceFolderAsync(inspection.EvidenceImagePath);
    }
}
