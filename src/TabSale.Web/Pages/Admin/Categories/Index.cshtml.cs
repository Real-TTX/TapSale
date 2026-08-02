using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Models;
using TabSale.Web.Services;

namespace TabSale.Web.Pages.Admin.Categories;

public sealed class IndexModel(AppDbContext db, CurrentUser current) : PageModel
{
    public SaleList SaleList { get; private set; } = null!;
    public List<ProductCategory> Items { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(long saleListId, string? search, string? status, string? sort)
    {
        SaleList = await AllowedLists().AsNoTracking().SingleOrDefaultAsync(x => x.Id == saleListId) ?? null!;
        if (SaleList is null) return NotFound();
        var query = db.ProductCategory.AsNoTracking().Include(x => x.Products).Where(x => x.SaleListId == saleListId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search));
        if (status == "active") query = query.Where(x => x.IsActive);
        else if (status == "archived") query = query.Where(x => !x.IsActive);
        Items = await (sort == "name" ? query.OrderBy(x => x.Name) : query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name)).ToListAsync();
        return Page();
    }

    private IQueryable<SaleList> AllowedLists() => current.IsAdmin
        ? db.SaleList
        : db.SaleList.Where(x => x.Users.Any(a => a.UserId == current.Id));
}
