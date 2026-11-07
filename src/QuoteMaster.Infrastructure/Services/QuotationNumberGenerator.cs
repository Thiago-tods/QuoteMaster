using System.Data;
using Microsoft.EntityFrameworkCore;
using QuoteMaster.Application.Interfaces;
using QuoteMaster.Infrastructure.Data;

namespace QuoteMaster.Infrastructure.Services;

public sealed class QuotationNumberGenerator(ApplicationDbContext db) : IQuotationNumberGenerator
{
    public async Task<string> GenerateAsync(DateTime quotationLocalDate, CancellationToken cancellationToken = default)
    {
        var year = quotationLocalDate.Year;
        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO QuotationNumberSequences (Year, LastNumber)
                VALUES (@year, 1)
                ON CONFLICT(Year) DO UPDATE SET LastNumber = LastNumber + 1
                RETURNING LastNumber;
                """;
            var yearParameter = command.CreateParameter();
            yearParameter.ParameterName = "@year";
            yearParameter.DbType = DbType.Int32;
            yearParameter.Value = year;
            command.Parameters.Add(yearParameter);

            var sequence = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
            return $"ORC-{year}-{sequence:D6}";
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
