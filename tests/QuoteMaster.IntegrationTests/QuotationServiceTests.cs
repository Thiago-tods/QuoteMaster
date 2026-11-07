using Microsoft.EntityFrameworkCore;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Domain.Enums;
using QuoteMaster.Domain.Exceptions;
using QuoteMaster.Infrastructure.Data;
using QuoteMaster.Infrastructure.Services;

namespace QuoteMaster.IntegrationTests;

public sealed class QuotationServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreatesSnapshotCalculatesPersistsAndTransitionsQuotation()
    {
        await using var database = new TestSqliteDatabase();
        await database.InitializeAsync();
        await using var db = database.CreateContext();
        var time = new FixedTimeProvider(Now);
        var clients = new ClientService(db);
        var products = new ProductService(db);
        var quotes = new QuotationService(db, new QuotationNumberGenerator(db), time);

        var client = await clients.CreateAsync(new SaveClientDto("Oficina Central", null, "contato@example.com", "11999999999", null));
        var service = await products.CreateAsync(new SaveProductDto("Instalação", null, 150m, ProductType.Service));
        var item = await products.CreateAsync(new SaveProductDto("Material", "Peça", 80m, ProductType.Product));

        var created = await quotes.CreateAsync(new CreateQuotationDto(client.Id, Now.UtcDateTime.AddDays(10), 30m, "Prazo comercial", [
            new CreateQuotationItemDto(service.Id, "Texto adulterado pelo cliente", 2m, 0m, 20m),
            new CreateQuotationItemDto(item.Id, null, 3m, null, 0m),
            new CreateQuotationItemDto(null, "Configuração", 1m, 120m, 10m)
        ]));

        Assert.Equal("ORC-2026-000001", created.Number);
        Assert.Equal(630m, created.Subtotal);
        Assert.Equal(30m, created.Discount);
        Assert.Equal(600m, created.Total);
        Assert.Equal("Instalação", created.Items.First().Description);
        Assert.Equal(150m, created.Items.First().UnitPrice);

        await products.UpdateAsync(service.Id, new SaveProductDto("Instalação", null, 200m, ProductType.Service));
        var reloaded = await quotes.GetAsync(created.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(150m, reloaded.Items.First().UnitPrice);
        Assert.Equal(600m, reloaded.Total);

        var sent = await quotes.SendAsync(created.Id);
        var approved = await quotes.ApproveAsync(sent.Id);
        Assert.Equal(QuotationStatus.Approved, approved.Status);

        await using var verify = database.CreateContext();
        var persisted = await verify.Quotations.Include(x => x.Items).SingleAsync(x => x.Id == created.Id);
        Assert.Equal(QuotationStatus.Approved, persisted.Status);
        Assert.Equal(3, persisted.Items.Count);
    }

    [Fact]
    public async Task RejectsInactiveClientsProductsAndInvalidQuotationAmounts()
    {
        await using var database = new TestSqliteDatabase();
        await database.InitializeAsync();
        await using var db = database.CreateContext();
        var clients = new ClientService(db);
        var products = new ProductService(db);
        var time = new FixedTimeProvider(Now);
        var quotes = new QuotationService(db, new QuotationNumberGenerator(db), time);

        var client = await clients.CreateAsync(new SaveClientDto("Cliente", null, null, null, null));
        var product = await products.CreateAsync(new SaveProductDto("Licença", null, 50m, ProductType.Product));
        await products.DeactivateAsync(product.Id);

        await Assert.ThrowsAsync<DomainException>(() => quotes.CreateAsync(new CreateQuotationDto(client.Id, Now.UtcDateTime.AddDays(1), 0m, null, [
            new CreateQuotationItemDto(product.Id, null, 1m, null, 0m)
        ])));
        await Assert.ThrowsAsync<DomainException>(() => quotes.CreateAsync(new CreateQuotationDto(client.Id, Now.UtcDateTime.AddDays(1), 0m, null, [
            new CreateQuotationItemDto(null, "Serviço", 0m, 10m, 0m)
        ])));

        await clients.DeactivateAsync(client.Id);
        await Assert.ThrowsAsync<DomainException>(() => quotes.CreateAsync(new CreateQuotationDto(client.Id, Now.UtcDateTime.AddDays(1), 0m, null, [
            new CreateQuotationItemDto(null, "Serviço", 1m, 10m, 0m)
        ])));
    }

    [Fact]
    public async Task SearchesAndSoftDeactivatesClientsAndProducts()
    {
        await using var database = new TestSqliteDatabase();
        await database.InitializeAsync();
        await using var db = database.CreateContext();
        var clients = new ClientService(db);
        var products = new ProductService(db);
        var client = await clients.CreateAsync(new SaveClientDto("Alpha Comércio", null, null, null, null));
        var product = await products.CreateAsync(new SaveProductDto("Serviço Alpha", null, 10m, ProductType.Service));

        Assert.Single(await clients.SearchAsync("Alpha", activeOnly: true));
        Assert.Single(await products.SearchAsync("Alpha", activeOnly: true));
        await clients.DeactivateAsync(client.Id);
        await products.DeactivateAsync(product.Id);
        Assert.Empty(await clients.SearchAsync("Alpha", activeOnly: true));
        Assert.Empty(await products.SearchAsync("Alpha", activeOnly: true));
        Assert.False((await clients.GetAsync(client.Id))!.IsActive);
        Assert.False((await products.GetAsync(product.Id))!.IsActive);
    }

    [Fact]
    public async Task UsesLocalBusinessDateForExpiryAndYearBoundaryNumbering()
    {
        await using var database = new TestSqliteDatabase();
        await database.InitializeAsync();
        var brazilTime = TimeZoneInfo.CreateCustomTimeZone("Brazil UTC-3", TimeSpan.FromHours(-3), "Brazil UTC-3", "Brazil UTC-3");
        var utcJustAfterNewYear = new DateTimeOffset(2027, 1, 1, 1, 30, 0, TimeSpan.Zero);
        int quoteId;
        await using (var db = database.CreateContext())
        {
            var clients = new ClientService(db);
            var products = new ProductService(db);
            var client = await clients.CreateAsync(new SaveClientDto("Cliente", null, null, null, null));
            var service = await products.CreateAsync(new SaveProductDto("Serviço", null, 100m, ProductType.Service));
            var quotes = new QuotationService(db, new QuotationNumberGenerator(db), new FixedTimeProvider(utcJustAfterNewYear, brazilTime));
            var quote = await quotes.CreateAsync(new CreateQuotationDto(client.Id, new DateTime(2026, 12, 31), 0m, null, [new CreateQuotationItemDto(service.Id, null, 1m, null, 0m)]));
            Assert.Equal("ORC-2026-000001", quote.Number);
            await quotes.SendAsync(quote.Id);
            quoteId = quote.Id;
            await Assert.ThrowsAsync<DomainException>(() => quotes.ExpireAsync(quoteId));
        }

        await using (var db = database.CreateContext())
        {
            var nextLocalDay = new FixedTimeProvider(new DateTimeOffset(2027, 1, 2, 1, 30, 0, TimeSpan.Zero), brazilTime);
            var quotes = new QuotationService(db, new QuotationNumberGenerator(db), nextLocalDay);
            var expired = await quotes.ExpireAsync(quoteId);
            Assert.Equal(QuotationStatus.Expired, expired.Status);
        }
    }

}
