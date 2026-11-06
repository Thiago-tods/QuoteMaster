using QuoteMaster.Infrastructure.Services;

namespace QuoteMaster.IntegrationTests;

public sealed class QuotationNumberGeneratorTests
{
    [Fact]
    public async Task GeneratesYearlySequentialNumbersAndHandlesConcurrentCalls()
    {
        await using var database = new TestSqliteDatabase();
        await database.InitializeAsync();

        async Task<string> Generate(DateTime date)
        {
            await using var db = database.CreateContext();
            return await new QuotationNumberGenerator(db).GenerateAsync(date);
        }

        var first = await Generate(new DateTime(2026, 1, 1));
        var second = await Generate(new DateTime(2026, 12, 31));
        var nextYear = await Generate(new DateTime(2027, 1, 1));
        Assert.Equal("ORC-2026-000001", first);
        Assert.Equal("ORC-2026-000002", second);
        Assert.Equal("ORC-2027-000001", nextYear);

        var concurrent = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => Generate(new DateTime(2026, 6, 1))));
        Assert.Equal(12, concurrent.Distinct().Count());
        Assert.Contains("ORC-2026-000003", concurrent);
        Assert.Contains("ORC-2026-000014", concurrent);
    }
}
