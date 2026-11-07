using System.Text;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Infrastructure.Services;

namespace QuoteMaster.IntegrationTests;

public sealed class QuotationPdfTests
{
    [Fact]
    public async Task GeneratesReadableMultiPagePdfWithCompanyAndQuotationContent()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Evaluation;
        var quotation = new QuotationDto(3, "ORC-2026-000003", 8, "Cliente Exemplo", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), 100m, 0m, 100m,
            QuoteMaster.Domain.Enums.QuotationStatus.Draft, string.Join(" ", Enumerable.Repeat("Condição comercial detalhada para análise do cliente.", 250)),
            Enumerable.Range(1, 100).Select(i => new QuotationItemDto(i, null, $"Serviço de demonstração {i}", 1, 1, 0, 1)).ToArray());
        var company = new CompanySettingsDto(1, "Empresa Demonstração", "Fantasia Exemplo", null, null, null, null, null, 7, null);
        var bytes = await new QuotationPdfGenerator().GenerateAsync(quotation, company, null);

        Assert.StartsWith("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
        var pdfType = System.Reflection.Assembly.Load("UglyToad.PdfPig").GetType("UglyToad.PdfPig.PdfDocument")!;
        var candidates = pdfType.GetMethods().Where(m => m.Name == "Open").ToArray();
        Assert.NotEmpty(candidates);
        var open = candidates.First(m => m.GetParameters()[0].ParameterType == typeof(byte[]));
        var document = open.Invoke(null, [bytes, null])!;
        try
        {
        Assert.True((int)pdfType.GetProperty("NumberOfPages")!.GetValue(document)! > 1);
        var pages = (System.Collections.IEnumerable)pdfType.GetMethods().First(m => m.Name == "GetPages" && m.GetParameters().Length == 0).Invoke(document, null)!;
        var text = string.Join(" ", pages.Cast<object>().Select(page => (string)page.GetType().GetProperty("Text")!.GetValue(page)!));
        Assert.Contains("ORC-2026-000003", text);
        Assert.Contains("Cliente Exemplo", text);
        Assert.Contains("Fantasia Exemplo", text);
        Assert.Contains("Serviço de demonstração", text);
        }
        finally { (document as IDisposable)?.Dispose(); }
    }
}


