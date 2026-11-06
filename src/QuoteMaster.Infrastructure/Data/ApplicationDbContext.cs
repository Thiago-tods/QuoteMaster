using Microsoft.EntityFrameworkCore;
using QuoteMaster.Domain.Entities;

namespace QuoteMaster.Infrastructure.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Document).HasMaxLength(18);
            entity.Property(x => x.Email).HasMaxLength(254);
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Address).HasMaxLength(400);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.DefaultPrice).HasPrecision(12, 2);
        });

        modelBuilder.Entity<Quotation>(entity =>
        {
            entity.Property(x => x.Number).HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.Number).IsUnique();
            entity.Property(x => x.Subtotal).HasPrecision(12, 2);
            entity.Property(x => x.Discount).HasPrecision(12, 2);
            entity.Property(x => x.Total).HasPrecision(12, 2);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.HasOne(x => x.Client).WithMany(x => x.Quotations).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QuotationItem>(entity =>
        {
            entity.Property(x => x.Description).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(12, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(12, 2);
            entity.Property(x => x.Discount).HasPrecision(12, 2);
            entity.Property(x => x.Total).HasPrecision(12, 2);
            entity.HasOne(x => x.Quotation).WithMany(x => x.Items).HasForeignKey(x => x.QuotationId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
