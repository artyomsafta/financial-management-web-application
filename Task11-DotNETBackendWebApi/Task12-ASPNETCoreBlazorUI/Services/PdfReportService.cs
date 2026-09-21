using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.Models.DTOs;

namespace Task12_ASPNETCoreBlazorUI.Services;

public class PdfReportService
{
    public byte[] GeneratePeriodReport(ReportDto report, string periodTitle = "Period Report")
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text(periodTitle).FontSize(18).Bold().FontColor(Colors.Grey.Darken1);
                        row.ConstantItem(140).AlignRight().Text($"Generated: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().PaddingBottom(15).Row(row =>
                    {
                        row.RelativeItem().Element(c => SummaryCard(c, "Total Income", $"{report.TotalIncome:N2}", "#2E7D32"));
                        row.ConstantItem(10);
                        row.RelativeItem().Element(c => SummaryCard(c, "Total Expenses", $"{report.TotalExpenses:N2}", "#C62828"));
                        row.ConstantItem(10);

                        var netColor = report.NetResult >= 0 ? "#2E7D32" : "#C62828";
                        row.RelativeItem().Element(c => SummaryCard(c, "Net Result", $"{report.NetResult:N2}", netColor));
                    });

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(110); // Date
                            columns.RelativeColumn(2);   // Type
                            columns.RelativeColumn(2);   // Wallet
                            columns.RelativeColumn(1);   // Amount
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("Date").Bold();
                            header.Cell().Element(HeaderStyle).Text("Type").Bold();
                            header.Cell().Element(HeaderStyle).Text("Wallet").Bold();
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Amount").Bold();

                            static IContainer HeaderStyle(IContainer container) =>
                                container.Background("#F5F5F5")
                                         .PaddingVertical(5)
                                         .PaddingHorizontal(4)
                                         .BorderBottom(1)
                                         .BorderColor("#BDBDBD");
                        });

                        foreach (var op in report.Operations)
                        {
                            var amountColor = op.Amount >= 0 ? "#2E7D32" : "#C62828";

                            table.Cell().Element(RowStyle).Text(op.Date.ToString("dd.MM.yyyy HH:mm"));
                            table.Cell().Element(RowStyle).Text(op.Type?.Name ?? "-");
                            table.Cell().Element(RowStyle).Text(op.Wallet?.Name ?? "-");
                            table.Cell().Element(RowStyle).AlignRight().Text($"{op.Amount:N2}").FontColor(amountColor);

                            static IContainer RowStyle(IContainer container) =>
                                container.BorderBottom(0.5f)
                                         .BorderColor("#E0E0E0")
                                         .PaddingVertical(4)
                                         .PaddingHorizontal(4);
                        }
                    });
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private void SummaryCard(IContainer container, string title, string value, string valueColorHex)
    {
        container
            .Border(1)
            .BorderColor("#E0E0E0")
            .Background("#FAFAFA")
            .Padding(8)
            .Column(col =>
            {
                col.Item().Text(title).FontSize(9).FontColor("#757575");
                col.Item().PaddingTop(2).Text(value).FontSize(12).Bold().FontColor(valueColorHex);
            });
    }
}
