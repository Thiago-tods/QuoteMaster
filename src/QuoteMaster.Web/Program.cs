using QuoteMaster.Infrastructure;
using QuoteMaster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Application.Interfaces;
using QuoteMaster.Domain.Enums;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
var defaultUserData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuoteMaster");
if (!builder.Environment.IsDevelopment())
{
    var databaseDirectory = Path.Combine(defaultUserData, "database");
    Directory.CreateDirectory(databaseDirectory);
    if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")))
        builder.Configuration["ConnectionStrings:DefaultConnection"] = $"Data Source={Path.Combine(databaseDirectory, "quotemaster.db")}";
}
var configuredLogoPath = builder.Configuration["Storage:CompanyLogosDirectory"];
builder.Configuration["Storage:CompanyLogosDirectory"] = !string.IsNullOrWhiteSpace(configuredLogoPath)
    ? configuredLogoPath
    : Path.Combine(builder.Environment.IsDevelopment() ? builder.Environment.ContentRootPath : defaultUserData, "uploads", "logos");
var backupDirectory = builder.Configuration["Storage:BackupDirectory"];
builder.Configuration["Storage:BackupDirectory"] = !string.IsNullOrWhiteSpace(backupDirectory)
    ? backupDirectory
    : Path.Combine(builder.Environment.IsDevelopment() ? builder.Environment.ContentRootPath : defaultUserData, "backups");
QuestPDF.Settings.License = builder.Configuration["QuestPdf:LicenseType"]?.ToLowerInvariant() switch {
    "community" => QuestPDF.Infrastructure.LicenseType.Community,
    "professional" => QuestPDF.Infrastructure.LicenseType.Professional,
    "enterprise" => QuestPDF.Infrastructure.LicenseType.Enterprise,
    _ => QuestPDF.Infrastructure.LicenseType.Evaluation
};
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
var configuredConnection = new SqliteConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
var instanceKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(configuredConnection.DataSource.ToUpperInvariant())));
using var serverMutex = new Mutex(true, $"Local\\QuoteMaster.Web.{instanceKey}", out var ownsServerMutex);
if (!ownsServerMutex) throw new InvalidOperationException("Já existe uma instância usando este banco de dados.");
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (builder.Configuration.GetValue<bool>("DemoData:Enabled") && builder.Configuration.GetValue<bool>("DemoData:ResetOnStartup"))
    {
        var demoDatabase = configuredConnection.DataSource;
        if (demoDatabase != ":memory:")
        {
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                var path = demoDatabase + suffix;
                if (File.Exists(path)) File.Delete(path);
            }
        }
        var demoLogos = builder.Configuration["Storage:CompanyLogosDirectory"]!;
        if (Directory.Exists(demoLogos)) Directory.Delete(demoLogos, recursive: true);
    }
    var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        var sqlite = new SqliteConnectionStringBuilder(db.Database.GetDbConnection().ConnectionString);
        var existingDatabase = sqlite.DataSource == ":memory:" ? null : Path.GetFullPath(sqlite.DataSource);
        if (existingDatabase is not null && File.Exists(existingDatabase))
        {
            Directory.CreateDirectory(builder.Configuration["Storage:BackupDirectory"]!);
            var backupPath = Path.Combine(builder.Configuration["Storage:BackupDirectory"]!, $"pre-migration-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}.db");
            await using var source = new SqliteConnection(sqlite.ConnectionString);
            var backupBuilder = new SqliteConnectionStringBuilder { DataSource = backupPath };
            await using var destination = new SqliteConnection(backupBuilder.ConnectionString);
            await source.OpenAsync();
            await destination.OpenAsync();
            source.BackupDatabase(destination);
        }
    }
    await db.Database.MigrateAsync();
    if ((app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("DemoData:Enabled")) && builder.Configuration.GetValue<bool>("DemoData:SeedOnStartup"))
    {
        var hasData = await db.Clients.AnyAsync() || await db.Products.AnyAsync() || await db.Quotations.AnyAsync() || await db.CompanySettings.AnyAsync();
        if (!hasData)
        {
            var settings = scope.ServiceProvider.GetRequiredService<ICompanySettingsService>();
            await settings.UpdateAsync(new UpdateCompanySettingsDto("NovaTech Soluções — DEMONSTRAÇÃO", "NovaTech (Demo)", null, "contato@example.com", "(11) 5555-0100", "Rua de Exemplo, 100 — São Paulo/SP", 14, "Proposta demonstrativa. Valores e disponibilidade devem ser confirmados."));
            var clients = scope.ServiceProvider.GetRequiredService<IClientService>();
            var products = scope.ServiceProvider.GetRequiredService<IProductService>();
            var quotations = scope.ServiceProvider.GetRequiredService<IQuotationService>();
            var clientRows = new List<ClientDto>();
            var clientNames = new[] { "Mercado Central (fictício)", "Oficina Horizonte (fictícia)", "Clínica Vida (fictícia)", "Escritório Alfa (fictício)", "Residencial Jardim (fictício)" };
            for (var i = 0; i < clientNames.Length; i++) clientRows.Add(await clients.CreateAsync(new SaveClientDto(clientNames[i], null, $"contato{i + 1}@example.com", $"11555501{i:00}", $"Av. Exemplo, {(i + 1) * 10}")));
            var productRows = new List<ProductDto>();
            var catalog = new (string Name, decimal Price, ProductType Type)[]
            {
                ("Instalação de roteador", 150m, ProductType.Service), ("Cabo de rede Cat6 (metro)", 8m, ProductType.Product),
                ("Configuração de computador", 120m, ProductType.Service), ("Manutenção preventiva", 200m, ProductType.Service),
                ("SSD 480 GB", 280m, ProductType.Product), ("Formatação de computador", 100m, ProductType.Service),
                ("Configuração de rede Wi-Fi", 180m, ProductType.Service), ("Roteador dual-band", 350m, ProductType.Product),
                ("Instalação de câmera IP", 220m, ProductType.Service), ("Backup e migração de dados", 160m, ProductType.Service)
            };
            foreach (var item in catalog) productRows.Add(await products.CreateAsync(new SaveProductDto(item.Name, "Item fictício para demonstração local", item.Price, item.Type)));
            for (var i = 0; i < 10; i++)
            {
                var validUntil = DateTime.Today.AddDays(14);
                var quote = await quotations.CreateAsync(new CreateQuotationDto(clientRows[i % 5].Id, validUntil, i * 5m, "Dados demonstrativos fictícios; não representam proposta real.", [
                    new CreateQuotationItemDto(productRows[i].Id, null, i % 3 + 1, null, 0m),
                    new CreateQuotationItemDto(productRows[(i + 1) % productRows.Count].Id, null, 2m, null, 0m)
                ]));
                if (i == 9)
                {
                    // Seed-only setup: create through the business service first, then age this demo draft to exercise expiry.
                    var demoDraft = await db.Quotations.SingleAsync(x => x.Id == quote.Id);
                    demoDraft.ValidUntil = DateTime.Today.AddDays(-1);
                    demoDraft.MarkSent();
                    demoDraft.Expire(DateTime.Today);
                    await db.SaveChangesAsync();
                }
                else if (i is >= 2 and <= 4 or 8) quote = await quotations.SendAsync(quote.Id);
                else if (i is >= 5 and <= 7) { quote = await quotations.SendAsync(quote.Id); quote = await quotations.ApproveAsync(quote.Id); }
                if (i == 8) await quotations.RejectAsync(quote.Id);
            }
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!builder.Configuration.GetValue<bool>("LocalLauncher:HttpOnly")) app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Dashboard}/{action=Index}/{id?}");
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.Run();
