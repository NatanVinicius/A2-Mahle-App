using A2MahleApp.Application.Features.Export;

using A2MahleApp.Application.Features.History.Models;
using A2MahleApp.Application.Features.History.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace A2MahleApp.Client.Components.Pages.HistoryPage;

public partial class HistoryPage
{
    private DateTime _selectedDate = DateTime.Today;
    private HistoryViewMode _viewMode = HistoryViewMode.Production;
    private HistoryJudgmentFilter _judgment = HistoryJudgmentFilter.All;
    private IReadOnlyList<ProductionHistoryItem> _productions = [];
    private IReadOnlyList<InspectionHistoryItem> _inspections = [];
    private bool _loading;
    private string? _errorMessage;
    private bool _shouldRenderProductionChart;
    private bool _isExportingPdf;

    [Inject]
    private IHistoryService HistoryService { get; set; } = null!;
    [Inject]
    public required IJSRuntime JSRuntime { get; set; }
    [Inject]
    private IHistoryExportPdfService HistoryExportPdfService { get; set; } = default!;

    [Inject]
    private IPdfFileSaver PdfFileSaver { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _shouldRenderProductionChart = false;

        try
        {
            if (_viewMode == HistoryViewMode.Production)
            {
                _productions =
                    await HistoryService.GetProductionsAsync(_selectedDate);

                _inspections = [];

                _shouldRenderProductionChart = _productions.Count > 0;
            }
            else
            {
                _inspections =
                    await HistoryService.GetInspectionsAsync(
                        _selectedDate,
                        _judgment);

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

    protected override async Task OnAfterRenderAsync(
      bool firstRender)
    {
        if (!_shouldRenderProductionChart)
        {
            return;
        }

        _shouldRenderProductionChart = false;

        if (_viewMode != HistoryViewMode.Production ||
            _productions.Count == 0)
        {
            return;
        }

        await RenderProductionChartAsync();
        await RenderRejectRateChartAsync();
    }

    private async Task RenderProductionChartAsync()
    {
        if (_productions.Count == 0)
        {
            return;
        }

        try
        {
            await JSRuntime.InvokeVoidAsync(
                "historyChart.render",
                "production-chart",
                _productions[0].Approved,
                _productions[0].Rejected,
                _productions[0].RejectRate);
        }
        catch (JSException exception)
        {
            Console.Error.WriteLine($"Error rendering production chart: {exception.Message}");
        }
    }

    private async Task RenderRejectRateChartAsync()
    {
        if (_productions.Count == 0)
        {
            return;
        }

        try
        {
            await JSRuntime.InvokeVoidAsync(
                "historyChart.renderRejectRate",
                "reject-rate-chart",
                _productions[0].RejectRate);
        }
        catch (JSException exception)
        {
            Console.Error.WriteLine(
                $"Error rendering reject rate chart: {exception.Message}");
        }
    }

    private async Task OnExportPdfClick()
    {
        if (_isExportingPdf)
        {
            return;
        }

        try
        {
            _isExportingPdf = true;
            _errorMessage = null;

            StateHasChanged();
            await Task.Yield();

            string exportHtml = await JSRuntime.InvokeAsync<string>(
                "historyChart.getExportHtml",
                "pdf-export-area");

            byte[] pdfBytes;
            string fileName;

            if (_viewMode == HistoryViewMode.Production)
            {
                pdfBytes =
                    await HistoryExportPdfService
                        .ExportProductionsAsync(
                            exportHtml);

                fileName =
                    $"Historico_Producao_{_selectedDate:yyyy-MM-dd}.pdf";
            }
            else
            {
                pdfBytes =
                    await HistoryExportPdfService
                        .ExportInspectionsAsync(
                            exportHtml);

                fileName =
                    $"Historico_Inspecoes_{_selectedDate:yyyy-MM-dd}.pdf";
            }

            await PdfFileSaver.SaveAsync(
                pdfBytes,
                fileName);
        }
        catch (OperationCanceledException)
        {
            // Usuário cancelou o diálogo "Salvar como".
        }
        catch (Exception exception)
        {
            _errorMessage =
                $"Não foi possível exportar o PDF: {exception.Message}";
        }
        finally
        {
            _isExportingPdf = false;
        }
    }


}
public enum HistoryViewMode
{
    Production,
    Inspection
}