using Microsoft.EntityFrameworkCore;
using TabSale.Web.Models;

namespace TabSale.Web.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> AppUser => Set<AppUser>();
    public DbSet<UserSession> UserSession => Set<UserSession>();
    public DbSet<SaleList> SaleList => Set<SaleList>();
    public DbSet<UserSaleList> UserSaleList => Set<UserSaleList>();
    public DbSet<Product> Product => Set<Product>();
    public DbSet<ProductCategory> ProductCategory => Set<ProductCategory>();
    public DbSet<ProductPriceVersion> ProductPriceVersion => Set<ProductPriceVersion>();
    public DbSet<Sale> Sale => Set<Sale>();
    public DbSet<SaleLine> SaleLine => Set<SaleLine>();
    public DbSet<CashShift> CashShift => Set<CashShift>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var type in modelBuilder.Model.GetEntityTypes())
            type.SetTableName(type.ClrType.Name);

        modelBuilder.Entity<AppUser>().HasIndex(x => x.UserName).IsUnique();
        modelBuilder.Entity<UserSession>().HasIndex(x => x.Token).IsUnique();
        modelBuilder.Entity<Sale>().HasIndex(x => x.Token).IsUnique();
        modelBuilder.Entity<CashShift>().HasIndex(x => x.Token).IsUnique();
        modelBuilder.Entity<UserSaleList>().HasIndex(x => new { x.UserId, x.SaleListId }).IsUnique();
        modelBuilder.Entity<ProductPriceVersion>().HasIndex(x => new { x.ProductId, x.Version }).IsUnique();
        modelBuilder.Entity<ProductCategory>().HasIndex(x => new { x.SaleListId, x.Name }).IsUnique();
        modelBuilder.Entity<Product>().HasOne(x => x.ProductCategory).WithMany(x => x.Products).HasForeignKey(x => x.ProductCategoryId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Sale>().HasOne(x => x.OriginalSale).WithMany().HasForeignKey(x => x.OriginalSaleId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Sale>().HasOne(x => x.User).WithMany().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Sale>().HasOne(x => x.SaleList).WithMany().OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<SaleLine>().HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.SetNull);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreateDate = now;
                entry.Entity.UpdateDate = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdateDate = now;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
