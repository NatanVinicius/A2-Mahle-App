using A2MahleApp.Application.Features.Export;
using A2MahleApp.Application.Features.History.Models;
using A2MahleApp.Domain.Features.Inspection.Enums;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace A2MahleApp.Infrastructure.Features.Export;

public sealed class HistoryPdfExportService : IHistoryExportPdfService
{
    private const string ApprovedColor = "#22c55e";
    private const string RejectedColor = "#ef4444";
    private const string NoRejectColor = "#b0b0b0";
    private const string RejectRateColor = "#ff6a00";
    private const float PageMargin = 20f;

    public async Task<byte[]> ExportProductionsAsync(
        ProductionHistoryItem? production,
        DateTime? date,
        byte[]? reportImage = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        byte[]? logoBytes = LoadLogoBytes();

        return await Task.Run(() =>
        {
            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(PageMargin);
                    page.DefaultTextStyle(text => text.FontSize(10));

                    page.Header().Column(column =>
                    {
                        if (logoBytes is not null)
                        {
                            column.Item().Width(120).Image(logoBytes);
                        }

                        column.Item().PaddingTop(10).AlignCenter().Text("RELATÓRIO DE PRODUÇÃO").Bold().FontSize(18);
                        column.Item().PaddingTop(15).Text($"Data: {(date is null ? "Todas" : date.Value.ToString("dd/MM/yyyy"))}");
                        column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingTop(30).Column(column =>
                    {
                        BuildProductionTable(column, production);

                        if (reportImage is { Length: > 0 })
                        {
                            column.Item().PaddingTop(20).AlignCenter().Width(520).Image(reportImage);
                        }

                        if (production is not null)
                        {
                            int approved = Math.Max(0, production.Approved);
                            int rejected = Math.Max(0, production.Rejected);
                            int total = approved + rejected;

                            if (total > 0 && reportImage is null)
                            {
                                BuildProductionCharts(column, production);
                            }
                        }
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                        column.Item().PaddingTop(6).AlignCenter().Text("A2 Vision Experts").FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf();
        });
    }

    public async Task<byte[]> ExportInspectionsAsync(
        IReadOnlyCollection<InspectionHistoryItem> inspections,
        DateTime? date,
        byte[]? reportImage = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        byte[]? logoBytes = LoadLogoBytes();

        return await Task.Run(() =>
        {
            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(PageMargin);
                    page.DefaultTextStyle(text => text.FontSize(10));

                    page.Header().Column(column =>
                    {
                        if (logoBytes is not null)
                        {
                            column.Item().Width(120).Image(logoBytes);
                        }

                        column.Item().PaddingTop(10).AlignCenter().Text("RELATÓRIO DE INSPEÇÕES").Bold().FontSize(18);
                        column.Item().PaddingTop(15).Text($"Data: {(date is null ? "Todas" : date.Value.ToString("dd/MM/yyyy"))}");
                        column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    });

                    page.Content().PaddingTop(30).Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Data/Hora").Bold();
                                header.Cell().Element(HeaderCell).Text("Julgamento").Bold();
                                header.Cell().Element(HeaderCell).AlignRight().Text("Ciclo").Bold();
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Imagens").Bold();
                            });

                            if (inspections.Count == 0)
                            {
                                table.Cell().ColumnSpan(4).Element(DataCell).AlignCenter().Text("Nenhuma inspeção encontrada");
                            }
                            else
                            {
                                foreach (InspectionHistoryItem inspection in inspections)
                                {
                                    table.Cell().Element(DataCell).Text(inspection.DateTime.ToString("dd/MM/yyyy HH:mm:ss"));
                                    table.Cell().Element(DataCell).Text(GetJudgmentText(inspection.Status));
                                    table.Cell().Element(DataCell).AlignRight().Text(GetCycleTimeText(inspection.CycleTimeMilliseconds));
                                    table.Cell().Element(DataCell).AlignCenter().Text(string.IsNullOrWhiteSpace(inspection.EvidenceImagePath) ? "—" : "Sim");
                                }
                            }
                        });

                        if (reportImage is { Length: > 0 })
                        {
                            column.Item().PaddingTop(20).AlignCenter().Width(520).Image(reportImage);
                        }
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                        column.Item().PaddingTop(6).AlignCenter().Text("A2 Vision Experts").FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                });
            }).GeneratePdf();
        });
    }

    private static byte[]? LoadLogoBytes()
    {
        const string resourceName = "A2MahleApp.Infrastructure.Features.Export.Assets.history-logo.png";

        using Stream? stream = typeof(HistoryPdfExportService).Assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return null;
        }

        using MemoryStream memory = new();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private static void BuildProductionTable(ColumnDescriptor column, ProductionHistoryItem? production)
    {
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Data").Bold();
                header.Cell().Element(HeaderCell).AlignRight().Text("Produzidas").Bold();
                header.Cell().Element(HeaderCell).AlignRight().Text("Aprovadas").Bold();
                header.Cell().Element(HeaderCell).AlignRight().Text("Reprovadas").Bold();
                header.Cell().Element(HeaderCell).AlignRight().Text("Taxa de Rejeito").Bold();
            });

            if (production is null)
            {
                table.Cell().ColumnSpan(5).Element(DataCell).AlignCenter().Text("Nenhum dado encontrado");
                return;
            }

            table.Cell().Element(DataCell).Text(production.Date.ToString("dd/MM/yyyy"));
            table.Cell().Element(DataCell).AlignRight().Text(production.Produced.ToString());
            table.Cell().Element(DataCell).AlignRight().Text(production.Approved.ToString());
            table.Cell().Element(DataCell).AlignRight().Text(production.Rejected.ToString());
            table.Cell().Element(DataCell).AlignRight().Text($"{production.RejectRate:F2}%");
        });
    }

    private static void BuildProductionCharts(ColumnDescriptor column, ProductionHistoryItem production)
    {
        int approved = Math.Max(0, production.Approved);
        int rejected = Math.Max(0, production.Rejected);
        int total = approved + rejected;

        if (total <= 0)
        {
            return;
        }

        double approvedPercentage = (double)approved / total * 100.0;
        double rejectedPercentage = (double)rejected / total * 100.0;
        double rejectRate = Math.Clamp(Convert.ToDouble(production.RejectRate), 0.0, 100.0);
        double noRejectRate = 100.0 - rejectRate;

        string productionDonutSvg = CreateProductionDonutSvg(180, 180, approvedPercentage, rejectedPercentage);
        string rejectRateDonutSvg = CreateRejectRateDonutSvg(180, 180, rejectRate, noRejectRate);

        column.Item().PaddingTop(35).Row(row =>
        {
            row.RelativeItem().Column(chartColumn =>
            {
                chartColumn.Item().AlignCenter().Row(chartRow =>
                {
                    chartRow.ConstantItem(90).AlignMiddle().Column(legend =>
                    {
                        legend.Item().Row(legendRow =>
                        {
                            legendRow.ConstantItem(8).Height(8).Background(ApprovedColor);
                            legendRow.RelativeItem().PaddingLeft(6).Text($"Aprovadas: {approved}").FontSize(8);
                        });

                        legend.Item().PaddingTop(10).Row(legendRow =>
                        {
                            legendRow.ConstantItem(8).Height(8).Background(RejectedColor);
                            legendRow.RelativeItem().PaddingLeft(6).Text($"Reprovadas: {rejected}").FontSize(8);
                        });
                    });

                    chartRow.ConstantItem(180).Height(180).Layers(layers =>
                    {
                        layers.PrimaryLayer().Svg(productionDonutSvg);
                        layers.Layer().AlignCenter().AlignMiddle().Column(center =>
                        {
                            center.Item().AlignCenter().Text("Produção").FontSize(9).FontColor(Colors.Grey.Darken1);
                            center.Item().AlignCenter().Text(total.ToString()).Bold().FontSize(20);
                        });
                    });
                });
            });

            row.RelativeItem().Column(chartColumn =>
            {
                chartColumn.Item().AlignCenter().Row(chartRow =>
                {
                    chartRow.ConstantItem(90).AlignMiddle().Column(legend =>
                    {
                        legend.Item().Row(legendRow =>
                        {
                            legendRow.ConstantItem(8).Height(8).Background(NoRejectColor);
                            legendRow.RelativeItem().PaddingLeft(6).Text($"Sem rejeito: {noRejectRate:F1}%").FontSize(8);
                        });

                        legend.Item().PaddingTop(10).Row(legendRow =>
                        {
                            legendRow.ConstantItem(8).Height(8).Background(RejectRateColor);
                            legendRow.RelativeItem().PaddingLeft(6).Text($"Rejeito: {rejectRate:F1}%").FontSize(8);
                        });
                    });

                    chartRow.ConstantItem(180).Height(180).Layers(layers =>
                    {
                        layers.PrimaryLayer().Svg(rejectRateDonutSvg);
                        layers.Layer().AlignCenter().AlignMiddle().Column(center =>
                        {
                            center.Item().AlignCenter().Text("Taxa de Rejeito").FontSize(9).FontColor(Colors.Grey.Darken1);
                            center.Item().AlignCenter().Text($"{rejectRate:F1}%").Bold().FontSize(20);
                        });
                    });
                });
            });
        });
    }

    private static string CreateProductionDonutSvg(double width, double height, double approvedPercentage, double rejectedPercentage)
    {
        return CreateDonutSvg(width, height, approvedPercentage, rejectedPercentage, ApprovedColor, RejectedColor);
    }

    private static string CreateRejectRateDonutSvg(double width, double height, double rejectRate, double noRejectRate)
    {
        return CreateDonutSvg(width, height, noRejectRate, rejectRate, NoRejectColor, RejectRateColor);
    }

    private static string CreateDonutSvg(double width, double height, double firstPercentage, double secondPercentage, string firstColor, string secondColor)
    {
        firstPercentage = Math.Clamp(firstPercentage, 0.0, 100.0);
        secondPercentage = Math.Clamp(secondPercentage, 0.0, 100.0);

        double totalPercentage = firstPercentage + secondPercentage;
        if (totalPercentage <= 0.0)
        {
            return string.Empty;
        }

        double centerX = width / 2.0;
        double centerY = height / 2.0;
        double radius = Math.Min(width, height) / 2.0 - 10.0;
        double circumference = 2.0 * Math.PI * radius;
        double firstDashLength = (firstPercentage / 100.0) * circumference;
        double secondDashLength = (secondPercentage / 100.0) * circumference;

        return $"""
<svg xmlns="http://www.w3.org/2000/svg" width="{width:F0}" height="{height:F0}" viewBox="0 0 {width:F0} {height:F0}">
    <circle cx="{centerX:F2}" cy="{centerY:F2}" r="{radius:F2}" fill="none" stroke="#E5E7EB" stroke-width="18" stroke-linecap="butt" />
    <circle cx="{centerX:F2}" cy="{centerY:F2}" r="{radius:F2}" fill="none" stroke="{firstColor}" stroke-width="18" stroke-linecap="butt" stroke-dasharray="{firstDashLength:F2} {circumference:F2}" stroke-dashoffset="0" transform="rotate(-90 {centerX:F2} {centerY:F2})" />
    <circle cx="{centerX:F2}" cy="{centerY:F2}" r="{radius:F2}" fill="none" stroke="{secondColor}" stroke-width="18" stroke-linecap="butt" stroke-dasharray="{secondDashLength:F2} {circumference:F2}" stroke-dashoffset="{circumference - firstDashLength:F2}" transform="rotate(-90 {centerX:F2} {centerY:F2})" />
</svg>
""";
    }

    private static string GetJudgmentText(InspectionStatus status)
    {
        return status switch
        {
            InspectionStatus.Approved => "Aprovada",
            InspectionStatus.Rejected => "Reprovada",
            _ => "—"
        };
    }

    private static string GetCycleTimeText(int cycleTime)
    {
        return $"{cycleTime} ms";
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container.Background(Colors.Grey.Lighten3).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(8).PaddingHorizontal(10);
    }

    private static IContainer DataCell(IContainer container)
    {
        return container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(8).PaddingHorizontal(10);
    }
}

