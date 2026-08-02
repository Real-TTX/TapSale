using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Models;
using TabSale.Web.Services;

namespace TabSale.Web.Pages.Admin.Products;

public sealed class EditModel(AppDbContext db, CurrentUser current, ProductImageStorage images, AppText text) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public bool CanDelete { get; private set; }
    public string? CurrentImagePath { get; private set; }
    public List<ProductCategory> Categories { get; private set; } = [];
    public IReadOnlyList<ProductIconCatalog.Option> IconOptions => ProductIconCatalog.All;

    public async Task<IActionResult> OnGetAsync(long? id, long? saleListId)
    {
        if (id is null)
        {
            if (saleListId is null || !await CanEditList(saleListId.Value)) return Forbid();
            Input.SaleListId = saleListId.Value;
            await LoadOptionsAsync();
            return Page();
        }

        var product = await db.Product.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (product is null || !await CanEditList(product.SaleListId)) return NotFound();
        Input = new InputModel
        {
            Id = product.Id,
            SaleListId = product.SaleListId,
            Name = product.Name,
            Price = product.UnitPriceCents / 100m,
            Kind = product.Kind,
            ProductCategoryId = product.ProductCategoryId,
            Color = product.Color,
            Icon = product.Icon,
            SortOrder = product.SortOrder,
            IsActive = product.IsActive
        };
        CurrentImagePath = product.ImagePath;
        CanDelete = !await db.SaleLine.AnyAsync(x => x.ProductId == product.Id);
        await LoadOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!await CanEditList(Input.SaleListId)) return Forbid();
        var entity = Input.Id == 0
            ? null
            : await db.Product.SingleOrDefaultAsync(x => x.Id == Input.Id && x.SaleListId == Input.SaleListId);
        if (Input.Id != 0 && entity is null) return NotFound();

        CurrentImagePath = entity?.ImagePath;
        CanDelete = entity is not null && !await db.SaleLine.AnyAsync(x => x.ProductId == entity.Id);
        await LoadOptionsAsync();
        if (!ProductIconCatalog.Contains(Input.Icon)) ModelState.AddModelError("Input.Icon", text["InvalidIcon"]);
        if (Input.ProductCategoryId is not null && !Categories.Any(x => x.Id == Input.ProductCategoryId))
            ModelState.AddModelError("Input.ProductCategoryId", text["InvalidCategory"]);
        if (!ModelState.IsValid) return Page();

        string? uploadedImage = null;
        if (Input.Image is not null)
        {
            try { uploadedImage = await images.SaveAsync(Input.Image, HttpContext.RequestAborted); }
            catch (InvalidDataException) { ModelState.AddModelError("Input.Image", text["InvalidImage"]); return Page(); }
        }

        var cents = decimal.ToInt64(decimal.Round(Input.Price * 100, 0, MidpointRounding.AwayFromZero));
        var oldImage = entity?.ImagePath;
        if (entity is null)
        {
            entity = new Product
            {
                SaleListId = Input.SaleListId,
                Name = Input.Name.Trim(),
                UnitPriceCents = cents,
                Kind = Input.Kind,
                ProductCategoryId = Input.ProductCategoryId,
                Color = Input.Color,
                Icon = Input.Icon,
                ImagePath = uploadedImage,
                SortOrder = Input.SortOrder,
                CreateUserId = current.Id,
                UpdateUserId = current.Id
            };
            db.Product.Add(entity);
        }
        else
        {
            var changed = entity.Name != Input.Name.Trim() || entity.UnitPriceCents != cents || entity.Kind != Input.Kind;
            entity.Name = Input.Name.Trim();
            entity.UnitPriceCents = cents;
            entity.Kind = Input.Kind;
            entity.ProductCategoryId = Input.ProductCategoryId;
            entity.Color = Input.Color;
            entity.Icon = Input.Icon;
            entity.ImagePath = uploadedImage ?? (Input.RemoveImage ? null : entity.ImagePath);
            entity.SortOrder = Input.SortOrder;
            entity.IsActive = Input.IsActive;
            entity.UpdateUserId = current.Id;
            if (changed) entity.Version++;
        }

        await db.SaveChangesAsync();
        if (!await db.ProductPriceVersion.AnyAsync(x => x.ProductId == entity.Id && x.Version == entity.Version))
        {
            db.ProductPriceVersion.Add(new ProductPriceVersion
            {
                ProductId = entity.Id,
                Version = entity.Version,
                Name = entity.Name,
                UnitPriceCents = entity.UnitPriceCents,
                Kind = entity.Kind,
                CreateUserId = current.Id,
                UpdateUserId = current.Id
            });
            await db.SaveChangesAsync();
        }

        if (oldImage != entity.ImagePath) images.Delete(oldImage);
        return RedirectToPage("/Admin/SaleLists/Edit", new { id = entity.SaleListId });
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var product = await db.Product.SingleOrDefaultAsync(x => x.Id == Input.Id);
        if (product is null || !await CanEditList(product.SaleListId)) return NotFound();
        if (await db.SaleLine.AnyAsync(x => x.ProductId == product.Id)) return BadRequest("Used products can only be archived.");
        db.Product.Remove(product);
        await db.SaveChangesAsync();
        images.Delete(product.ImagePath);
        return RedirectToPage("/Admin/SaleLists/Edit", new { id = product.SaleListId });
    }

    private async Task LoadOptionsAsync() => Categories = await db.ProductCategory.AsNoTracking()
        .Where(x => x.SaleListId == Input.SaleListId)
        .OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync();

    private Task<bool> CanEditList(long id) => current.IsAdmin
        ? db.SaleList.AnyAsync(x => x.Id == id)
        : db.UserSaleList.AnyAsync(x => x.UserId == current.Id && x.SaleListId == id);

    public sealed class InputModel
    {
        public long Id { get; set; }
        public long SaleListId { get; set; }
        [Required, MaxLength(120)] public string Name { get; set; } = "";
        [Range(0, 100000)] public decimal Price { get; set; }
        public ProductKind Kind { get; set; }
        public long? ProductCategoryId { get; set; }
        [Required, RegularExpression("^#[0-9A-Fa-f]{6}$")] public string Color { get; set; } = "#167D6D";
        [Required, MaxLength(40)] public string Icon { get; set; } = "tag";
        public IFormFile? Image { get; set; }
        public bool RemoveImage { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
