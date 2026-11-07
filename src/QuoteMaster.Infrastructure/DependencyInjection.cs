using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuoteMaster.Infrastructure.Data;
using QuoteMaster.Infrastructure.Services;
using QuoteMaster.Application.Interfaces;

namespace QuoteMaster.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not configured.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IQuotationNumberGenerator, QuotationNumberGenerator>();
        services.AddScoped<IQuotationService, QuotationService>();
        services.AddScoped<ICompanySettingsService>(sp => new CompanySettingsService(
            sp.GetRequiredService<ApplicationDbContext>(),
            configuration["Storage:CompanyLogosDirectory"] ?? Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "CompanyLogos")));
        services.AddScoped<IQuotationPdfGenerator, QuotationPdfGenerator>();
        return services;
    }
}
