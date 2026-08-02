using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Models;
using TabSale.Web.Services;

namespace TabSale.Web.Pages.Admin.SaleLists;

public sealed class EditModel(AppDbContext db, CurrentUser current) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public List<Product> Products { get; private set; } = [];
    public List<ProductCategory> Categories { get; private set; } = [];
    public string ActiveTab { get; private set; } = "products";
    public int ProductCount { get; private set; }
    public int CategoryCount { get; private set; }
    public bool CanDelete { get; private set; }

    public async Task<IActionResult> OnGetAsync(long? id, string? tab = null, string? productSearch = null, string? productStatus = null, string? categorySearch = null, string? categoryStatus = null)
    {
        ActiveTab = tab == "categories" ? "categories" : "products";
        if (id is null)
        {
            if (!current.IsAdmin) return Forbid();
            return Page();
        }

        var entity = await Allowed().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        Input = new InputModel { Id=entity.Id, Name=entity.Name, IsActive=entity.IsActive };

        var productQuery = db.Product.AsNoTracking().Include(x => x.ProductCategory).Where(x => x.SaleListId == entity.Id);
        var categoryQuery = db.ProductCategory.AsNoTracking().Include(x => x.Products).Where(x => x.SaleListId == entity.Id);
        ProductCount = await productQuery.CountAsync();
        CategoryCount = await categoryQuery.CountAsync();
        if (!string.IsNullOrWhiteSpace(productSearch)) productQuery = productQuery.Where(x => x.Name.Contains(productSearch));
        if (productStatus == "active") productQuery = productQuery.Where(x => x.IsActive);
        else if (productStatus == "archived") productQuery = productQuery.Where(x => !x.IsActive);
        if (!string.IsNullOrWhiteSpace(categorySearch)) categoryQuery = categoryQuery.Where(x => x.Name.Contains(categorySearch));
        if (categoryStatus == "active") categoryQuery = categoryQuery.Where(x => x.IsActive);
        else if (categoryStatus == "archived") categoryQuery = categoryQuery.Where(x => !x.IsActive);
        Products = await productQuery.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync();
        Categories = await categoryQuery.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync();
        CanDelete = current.IsAdmin && ProductCount == 0 && !await db.Sale.AnyAsync(x => x.SaleListId == entity.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid) return await OnGetAsync(Input.Id == 0 ? null : Input.Id);
        SaleList entity;
        if (Input.Id == 0)
        {
            if (!current.IsAdmin) return Forbid();
            entity = new SaleList { Name=Input.Name.Trim(), CreateUserId=current.Id, UpdateUserId=current.Id };
            db.SaleList.Add(entity);
        }
        else
        {
            entity = await Allowed().SingleOrDefaultAsync(x => x.Id == Input.Id) ?? throw new InvalidOperationException();
            entity.Name=Input.Name.Trim(); entity.IsActive=Input.IsActive; entity.UpdateUserId=current.Id;
        }
        await db.SaveChangesAsync();
        return RedirectToPage("Edit", new { id=entity.Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        if (!current.IsAdmin) return Forbid();
        var entity = await db.SaleList.Include(x => x.Products).SingleOrDefaultAsync(x => x.Id == Input.Id);
        if (entity is null) return NotFound();
        if (entity.Products.Count > 0 || await db.Sale.AnyAsync(x => x.SaleListId == entity.Id)) return BadRequest("Used sale lists can only be archived.");
        db.SaleList.Remove(entity); await db.SaveChangesAsync();
        return RedirectToPage("Index");
    }

    private IQueryable<SaleList> Allowed() => current.IsAdmin
        ? db.SaleList
        : db.SaleList.Where(x => x.Users.Any(assignment => assignment.UserId == current.Id));

    public sealed class InputModel
    {
        public long Id { get; set; }
        [Required, MaxLength(120)] public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }
}
