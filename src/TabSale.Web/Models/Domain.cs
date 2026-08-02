using System.ComponentModel.DataAnnotations;

namespace TabSale.Web.Models;

public abstract class AuditEntity
{
    public long Id { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public long? CreateUserId { get; set; }
    public DateTimeOffset UpdateDate { get; set; }
    public long? UpdateUserId { get; set; }
}

public enum UserRole { Admin, Manager, User }
public enum ProductKind { Product, DepositCharge, DepositReturn }
public enum SaleKind { Sale, Cancellation }

public sealed class AppUser : AuditEntity
{
    [MaxLength(80)] public required string UserName { get; set; }
    [MaxLength(120)] public required string DisplayName { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    [MaxLength(2)] public string Language { get; set; } = "en";
    public List<UserSaleList> SaleLists { get; set; } = [];
}

public sealed class UserSession : AuditEntity
{
    public long UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public Guid Token { get; set; }
    [MaxLength(120)] public string DeviceName { get; set; } = "Browser";
    public DateTimeOffset LastSeenDate { get; set; }
    public DateTimeOffset? LastSyncDate { get; set; }
    public DateTimeOffset? RevokedDate { get; set; }
}

public sealed class SaleList : AuditEntity
{
    [MaxLength(120)] public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Product> Products { get; set; } = [];
    public List<ProductCategory> Categories { get; set; } = [];
    public List<UserSaleList> Users { get; set; } = [];
}

public sealed class UserSaleList : AuditEntity
{
    public long UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public long SaleListId { get; set; }
    public SaleList SaleList { get; set; } = null!;
}

public sealed class Product : AuditEntity
{
    public long SaleListId { get; set; }
    public SaleList SaleList { get; set; } = null!;
    [MaxLength(120)] public required string Name { get; set; }
    public long UnitPriceCents { get; set; }
    public ProductKind Kind { get; set; }
    public long? ProductCategoryId { get; set; }
    public ProductCategory? ProductCategory { get; set; }
    [MaxLength(16)] public string Color { get; set; } = "#167D6D";
    [MaxLength(40)] public string Icon { get; set; } = "tag";
    [MaxLength(200)] public string? ImagePath { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
    public List<ProductPriceVersion> Versions { get; set; } = [];
}

public sealed class ProductCategory : AuditEntity
{
    public long SaleListId { get; set; }
    public SaleList SaleList { get; set; } = null!;
    [MaxLength(80)] public required string Name { get; set; }
    [MaxLength(16)] public string Color { get; set; } = "#167D6D";
    [MaxLength(40)] public string Icon { get; set; } = "tag";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Product> Products { get; set; } = [];
}

public sealed class ProductPriceVersion : AuditEntity
{
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Version { get; set; }
    [MaxLength(120)] public required string Name { get; set; }
    public long UnitPriceCents { get; set; }
    public ProductKind Kind { get; set; }
}

public sealed class CashShift : AuditEntity
{
    public Guid Token { get; set; }
    public long UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public long SaleListId { get; set; }
    public SaleList SaleList { get; set; } = null!;
    public long OpeningCents { get; set; }
    public long? CountedClosingCents { get; set; }
    public DateTimeOffset OpenedDate { get; set; }
    public DateTimeOffset? ClosedDate { get; set; }
}

public sealed class Sale : AuditEntity
{
    public Guid Token { get; set; }
    public Guid DeviceToken { get; set; }
    public long UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public long SaleListId { get; set; }
    public SaleList SaleList { get; set; } = null!;
    public long? CashShiftId { get; set; }
    public CashShift? CashShift { get; set; }
    public SaleKind Kind { get; set; }
    public long? OriginalSaleId { get; set; }
    public Sale? OriginalSale { get; set; }
    public DateTimeOffset SoldDate { get; set; }
    public long TotalCents { get; set; }
    public long? TenderedCents { get; set; }
    public long? ChangeCents { get; set; }
    public List<SaleLine> Lines { get; set; } = [];
}

public sealed class SaleLine : AuditEntity
{
    public long SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public long? ProductId { get; set; }
    [MaxLength(120)] public required string ProductName { get; set; }
    public ProductKind ProductKind { get; set; }
    public int ProductVersion { get; set; }
    public long UnitPriceCents { get; set; }
    public int Quantity { get; set; }
    public long LineTotalCents { get; set; }
}
