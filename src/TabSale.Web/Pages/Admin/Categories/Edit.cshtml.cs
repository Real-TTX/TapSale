using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Models;
using TabSale.Web.Services;

namespace TabSale.Web.Pages.Admin.Categories;

public sealed class EditModel(AppDbContext db, CurrentUser current, AppText text) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public bool CanDelete { get; private set; }
    public IReadOnlyList<ProductIconCatalog.Option> IconOptions => ProductIconCatalog.All;

    public async Task<IActionResult> OnGetAsync(long? id, long? saleListId)
    {
        if (id is null)
        {
            if (saleListId is null || !await CanEditList(saleListId.Value)) return Forbid();
            Input.SaleListId = saleListId.Value;
            return Page();
        }
        var category = await db.ProductCategory.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (category is null || !await CanEditList(category.SaleListId)) return NotFound();
        Input = new InputModel { Id=category.Id, SaleListId=category.SaleListId, Name=category.Name, Color=category.Color, Icon=category.Icon, SortOrder=category.SortOrder, IsActive=category.IsActive };
        CanDelete = !await db.Product.AnyAsync(x => x.ProductCategoryId == category.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!await CanEditList(Input.SaleListId)) return Forbid();
        if (!ProductIconCatalog.Contains(Input.Icon)) ModelState.AddModelError("Input.Icon", text["InvalidIcon"]);
        var duplicate = await db.ProductCategory.AnyAsync(x => x.SaleListId == Input.SaleListId && x.Name == Input.Name.Trim() && x.Id != Input.Id);
        if (duplicate) ModelState.AddModelError("Input.Name", text["CategoryExists"]);
        if (!ModelState.IsValid) return Page();

        ProductCategory entity;
        if (Input.Id == 0)
        {
            entity = new ProductCategory { SaleListId=Input.SaleListId, Name=Input.Name.Trim(), Color=Input.Color, Icon=Input.Icon, SortOrder=Input.SortOrder, CreateUserId=current.Id, UpdateUserId=current.Id };
            db.ProductCategory.Add(entity);
        }
        else
        {
            entity = await db.ProductCategory.SingleOrDefaultAsync(x => x.Id == Input.Id && x.SaleListId == Input.SaleListId) ?? throw new InvalidOperationException();
            entity.Name=Input.Name.Trim(); entity.Color=Input.Color; entity.Icon=Input.Icon; entity.SortOrder=Input.SortOrder; entity.IsActive=Input.IsActive; entity.UpdateUserId=current.Id;
        }
        await db.SaveChangesAsync();
        return RedirectToPage("/Admin/SaleLists/Edit", new { id=entity.SaleListId, tab="categories" });
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var category = await db.ProductCategory.SingleOrDefaultAsync(x => x.Id == Input.Id);
        if (category is null || !await CanEditList(category.SaleListId)) return NotFound();
        if (await db.Product.AnyAsync(x => x.ProductCategoryId == category.Id)) return BadRequest("Used categories can only be archived.");
        db.ProductCategory.Remove(category); await db.SaveChangesAsync();
        return RedirectToPage("/Admin/SaleLists/Edit", new { id=category.SaleListId, tab="categories" });
    }

    private Task<bool> CanEditList(long id) => current.IsAdmin
        ? db.SaleList.AnyAsync(x => x.Id == id)
        : db.UserSaleList.AnyAsync(x => x.UserId == current.Id && x.SaleListId == id);

    public sealed class InputModel
    {
        public long Id { get; set; }
        public long SaleListId { get; set; }
        [Required, MaxLength(80)] public string Name { get; set; } = "";
        [Required, RegularExpression("^#[0-9A-Fa-f]{6}$")] public string Color { get; set; } = "#167D6D";
        [Required, MaxLength(40)] public string Icon { get; set; } = "tag";
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
