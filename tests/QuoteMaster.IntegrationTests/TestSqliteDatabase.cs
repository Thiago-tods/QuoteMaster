using Microsoft.EntityFrameworkCore;
using QuoteMaster.Infrastructure.Data;

namespace QuoteMaster.IntegrationTests;

internal sealed class TestSqliteDatabase : IAsyncDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"quotemaster-test-{Guid.NewGuid():N}.db");
    public string ConnectionString => $"Data Source={_path};Default Timeout=30;Pooling=False";

    public ApplicationDbContext CreateContext() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(ConnectionString).Options);

    public async Task InitializeAsync()
    {
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public ValueTask DisposeAsync()
    {
        foreach (var suffix in new[] { "", "-shm", "-wal" })
        {
            var candidate = _path + suffix;
            if (File.Exists(candidate)) File.Delete(candidate);
        }
        return ValueTask.CompletedTask;
    }
}

internal sealed class FixedTimeProvider(DateTimeOffset now, TimeZoneInfo? localTimeZone = null) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
    public override TimeZoneInfo LocalTimeZone => localTimeZone ?? TimeZoneInfo.Local;
}
