using QuoteMaster.Application.DTOs;
using QuoteMaster.Infrastructure.Services;

namespace QuoteMaster.IntegrationTests;

public sealed class CompanySettingsTests
{
    [Fact]
    public async Task PersistsSettingsAndValidatesAndReplacesLogoSafely()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Evaluation;
        await using var database = new TestSqliteDatabase(); await database.InitializeAsync();
        var directory = Path.Combine(Path.GetTempPath(), "qm-logo-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            await using var db = database.CreateContext(); var service = new CompanySettingsService(db, directory);
            var defaults = await service.GetAsync(); Assert.Equal(7, defaults.DefaultValidityDays);
            var saved = await service.UpdateAsync(new UpdateCompanySettingsDto("Empresa Teste", null, null, "teste@example.com", null, null, 21, "Prazo padrão"));
            Assert.Equal("Empresa Teste", saved.CompanyName); Assert.Equal(21, saved.DefaultValidityDays);
            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateAsync(new UpdateCompanySettingsDto("", null, null, null, null, null, 7, null)));
            await Assert.ThrowsAsync<ArgumentException>(() => service.SaveLogoAsync(new byte[] { 1, 2, 3 }));
            await Assert.ThrowsAsync<ArgumentException>(() => service.SaveLogoAsync([137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 0]));
            await Assert.ThrowsAsync<ArgumentException>(() => service.SaveLogoAsync([255, 216, 255, 0, 0, 0]));
            await Assert.ThrowsAsync<ArgumentException>(() => service.SaveLogoAsync(new byte[2 * 1024 * 1024 + 1]));
            var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/pWQAAAAASUVORK5CYII=");
            var withLogo = await service.SaveLogoAsync(png);
            Assert.NotNull(withLogo.LogoPath);
            Assert.Equal("image/png", (await service.GetLogoAsync())!.ContentType);
            await service.RemoveLogoAsync(); Assert.Null((await service.GetLogoAsync()));
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }
}
