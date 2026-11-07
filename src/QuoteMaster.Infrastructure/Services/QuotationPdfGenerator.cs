using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Application.Interfaces;

namespace QuoteMaster.Infrastructure.Services;

public sealed class QuotationPdfGenerator : IQuotationPdfGenerator
{
    public Task<byte[]> GenerateAsync(QuotationDto quotation, CompanySettingsDto company, CompanyLogoDto? logo, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var pt = CultureInfo.GetCultureInfo("pt-BR");
        var pdf = Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4); page.Margin(36); page.DefaultTextStyle(x => x.FontSize(9).FontColor("233044"));
            page.Header().Column(header =>
            {
                header.Item().Row(row =>
                {
                    if (logo is not null) row.ConstantItem(70).Height(52).Image(logo.Content).FitArea();
                    row.RelativeItem().Column(c => { c.Item().Text(company.TradeName ?? company.CompanyName).Bold().FontSize(17).FontColor("263C74"); c.Item().Text(string.Join("  •  ", new[] { company.Document, company.Email, company.Phone, company.Address }.Where(s => !string.IsNullOrWhiteSpace(s)))); });
                });
                header.Item().PaddingTop(12).LineHorizontal(1).LineColor("DDE3EC");
            });
            page.Content().PaddingVertical(18).Column(column =>
            {
                column.Spacing(12);
                column.Item().Row(row => { row.RelativeItem().Text("ORÇAMENTO").Bold().FontSize(20); row.ConstantItem(190).AlignRight().Text(quotation.Number).Bold().FontSize(14); });
                column.Item().Row(row => { row.RelativeItem().Text($"Emitido em: {quotation.CreatedAt.ToLocalTime():dd/MM/yyyy}"); row.RelativeItem().Text($"Válido até: {quotation.ValidUntil:dd/MM/yyyy}"); });
                column.Item().Background("F4F6FA").Padding(10).Column(c => { c.Item().Text("CLIENTE").Bold().FontSize(8).FontColor("65718A"); c.Item().Text(quotation.ClientName).Bold().FontSize(12); });
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols => { cols.RelativeColumn(4); cols.RelativeColumn(1); cols.RelativeColumn(1.4f); cols.RelativeColumn(1.4f); });
                    table.Header(h => { HeaderCell(h, "Descrição"); HeaderCell(h, "Qtd."); HeaderCell(h, "Unitário"); HeaderCell(h, "Total"); });
                    foreach (var item in quotation.Items)
                    { BodyCell(table, item.Description); BodyCell(table, item.Quantity.ToString("0.###", pt), true); BodyCell(table, item.UnitPrice.ToString("C", pt), true); BodyCell(table, item.Total.ToString("C", pt), true); }
                });
                column.Item().ShowEntire().AlignRight().Width(260).Column(c =>
                { c.Item().Text($"Subtotal: {quotation.Subtotal.ToString("C", pt)}").AlignRight(); c.Item().Text($"Desconto: {quotation.Discount.ToString("C", pt)}").AlignRight(); c.Item().PaddingTop(5).BorderTop(1).BorderColor("DDE3EC").Text($"TOTAL: {quotation.Total.ToString("C", pt)}").Bold().FontSize(14).AlignRight(); });
                if (!string.IsNullOrWhiteSpace(quotation.Notes)) column.Item().PaddingTop(12).Column(c => { c.Item().Text("OBSERVAÇÕES E CONDIÇÕES").Bold().FontSize(9); c.Item().Text(quotation.Notes); });
                column.Item().PaddingTop(35).Row(row => { row.RelativeItem().PaddingHorizontal(18).Column(c => { c.Item().LineHorizontal(1); c.Item().AlignCenter().Text(company.TradeName ?? company.CompanyName); }); row.ConstantItem(28); row.RelativeItem().PaddingHorizontal(18).Column(c => { c.Item().LineHorizontal(1); c.Item().AlignCenter().Text(quotation.ClientName); }); });
            });
            page.Footer().AlignCenter().Text(text => { text.Span("QuoteMaster  •  Página "); text.CurrentPageNumber(); text.Span(" de "); text.TotalPages(); });
        })).GeneratePdf();
        return Task.FromResult(pdf);
    }

    private static void HeaderCell(TableCellDescriptor cells, string text) => cells.Cell().Background("263C74").Padding(7).Text(text).Bold().FontColor(Colors.White);
    private static void BodyCell(TableDescriptor table, string text, bool right = false)
    {
        if (right) table.Cell().BorderBottom(1).BorderColor("E8EBF1").Padding(7).AlignRight().Text(text);
        else table.Cell().BorderBottom(1).BorderColor("E8EBF1").Padding(7).Text(text);
    }
}
