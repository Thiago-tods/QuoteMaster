using Microsoft.EntityFrameworkCore;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Application.Interfaces;
using QuoteMaster.Domain.Entities;
using QuoteMaster.Infrastructure.Data;
using QuestPDF.Fluent;

namespace QuoteMaster.Infrastructure.Services;

public sealed class CompanySettingsService(ApplicationDbContext db, string logoDirectory) : ICompanySettingsService
{
    private const int MaxLogoBytes = 2 * 1024 * 1024;

    public async Task<CompanySettingsDto> GetAsync(CancellationToken cancellationToken = default)
        => Map(await GetOrCreateAsync(cancellationToken));

    public async Task<CompanySettingsDto> UpdateAsync(UpdateCompanySettingsDto input, CancellationToken cancellationToken = default)
    {
        var name = input.CompanyName?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 160) throw new ArgumentException("Informe o nome da empresa (até 160 caracteres).");
        if (input.DefaultValidityDays is < 1 or > 365) throw new ArgumentException("A validade padrão deve ficar entre 1 e 365 dias.");
        if (input.Email?.Length > 254 || input.Phone?.Length > 30 || input.Address?.Length > 400 || input.TradeName?.Length > 160 || input.Document?.Length > 18 || input.DefaultNotes?.Length > 2000)
            throw new ArgumentException("Um ou mais campos excedem o tamanho permitido.");
        var row = await GetOrCreateAsync(cancellationToken);
        row.CompanyName = name; row.TradeName = Clean(input.TradeName); row.Document = Clean(input.Document);
        row.Email = Clean(input.Email); row.Phone = Clean(input.Phone); row.Address = Clean(input.Address);
        row.DefaultValidityDays = input.DefaultValidityDays; row.DefaultNotes = Clean(input.DefaultNotes);
        await db.SaveChangesAsync(cancellationToken);
        return Map(row);
    }

    public async Task<CompanySettingsDto> SaveLogoAsync(byte[] content, CancellationToken cancellationToken = default)
    {
        if (content.Length is 0 or > MaxLogoBytes) throw new ArgumentException("A imagem deve ter até 2 MiB.");
        var png = content.Length >= 8 && content.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var jpeg = content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF;
        if (!png && !jpeg) throw new ArgumentException("Envie uma imagem PNG ou JPEG válida.");
        try
        {
            _ = Document.Create(document => document.Page(page => page.Content().Image(content).FitArea())).GeneratePdf();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            throw new ArgumentException("O arquivo tem uma assinatura PNG/JPEG, mas a imagem não pôde ser decodificada.", nameof(content), ex);
        }
        Directory.CreateDirectory(logoDirectory);
        var filename = $"{Guid.NewGuid():N}.{(png ? "png" : "jpg")}";
        var path = Path.Combine(logoDirectory, filename);
        await File.WriteAllBytesAsync(path, content, cancellationToken);
        try
        {
            var row = await GetOrCreateAsync(cancellationToken);
            var old = row.LogoPath;
            row.LogoPath = filename;
            await db.SaveChangesAsync(cancellationToken);
            DeleteLogo(old);
            return Map(row);
        }
        catch { File.Delete(path); throw; }
    }

    public async Task<CompanySettingsDto> RemoveLogoAsync(CancellationToken cancellationToken = default)
    {
        var row = await GetOrCreateAsync(cancellationToken); var old = row.LogoPath; row.LogoPath = null;
        await db.SaveChangesAsync(cancellationToken); DeleteLogo(old); return Map(row);
    }

    public async Task<CompanyLogoDto?> GetLogoAsync(CancellationToken cancellationToken = default)
    {
        var row = await GetOrCreateAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(row.LogoPath) || Path.GetFileName(row.LogoPath) != row.LogoPath) return null;
        var path = Path.Combine(logoDirectory, row.LogoPath);
        if (!File.Exists(path)) return null;
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        var type = row.LogoPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";
        return new(bytes, type);
    }

    private async Task<CompanySettings> GetOrCreateAsync(CancellationToken ct)
    {
        var row = await db.CompanySettings.FindAsync([1], ct);
        if (row is not null) return row;
        row = new CompanySettings { Id = 1, CompanyName = "Minha empresa", DefaultValidityDays = 7 };
        db.CompanySettings.Add(row); await db.SaveChangesAsync(ct); return row;
    }
    private void DeleteLogo(string? filename)
    {
        if (!string.IsNullOrWhiteSpace(filename) && Path.GetFileName(filename) == filename)
        { var path = Path.Combine(logoDirectory, filename); if (File.Exists(path)) File.Delete(path); }
    }
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static CompanySettingsDto Map(CompanySettings x) => new(x.Id, x.CompanyName, x.TradeName, x.Document, x.Email, x.Phone, x.Address, x.LogoPath, x.DefaultValidityDays, x.DefaultNotes);
}
