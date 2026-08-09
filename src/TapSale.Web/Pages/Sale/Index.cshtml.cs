using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TapSale.Web.Data;
using TapSale.Web.Models;
using TapSale.Web.Services;

namespace TapSale.Web.Pages.Sale;
public sealed class IndexModel(AppDbContext db, CurrentUser current) : PageModel
{
    public List<SaleListOption> Lists { get; private set; } = [];
    public long? ActiveListId { get; private set; }
    public string CatalogJson { get; private set; } = "[]";
    public long UserId => current.Id;

    public async Task OnGetAsync(long? listId)
    {
        var query = db.SaleList.AsNoTracking().Where(x => x.IsActive);
        if (!current.IsAdmin) query = query.Where(x => x.Users.Any(a => a.UserId == current.Id));
        Lists = await query.OrderBy(x => x.Name).Select(x => new SaleListOption(x.Id, x.Name)).ToListAsync();
        ActiveListId = Lists.Any(x => x.Id == listId) ? listId : Lists.FirstOrDefault()?.Id;
        var catalog = await query.Select(x => new
        {
            id = x.Id, name = x.Name, categoryDisplayMode = x.CategoryDisplayMode.ToString(),
            products = x.Products.Where(p => p.IsActive)
                .OrderBy(p => p.ProductCategory == null ? int.MaxValue : p.ProductCategory.SortOrder).ThenBy(p => p.SortOrder).ThenBy(p => p.Name)
                .Select(p => new
                {
                    id = p.Id, p.Name, priceCents = p.UnitPriceCents, kind = p.Kind.ToString(), p.Color, p.Icon,
                    imageUrl = p.ImagePath == null ? null : "/uploads/" + p.ImagePath,
                    categoryId = p.ProductCategory != null && p.ProductCategory.IsActive ? p.ProductCategoryId : null,
                    categoryName = p.ProductCategory != null && p.ProductCategory.IsActive ? p.ProductCategory.Name : null,
                    categoryIcon = p.ProductCategory != null && p.ProductCategory.IsActive ? p.ProductCategory.Icon : null,
                    categoryColor = p.ProductCategory != null && p.ProductCategory.IsActive ? p.ProductCategory.Color : null,
                    categorySortOrder = p.ProductCategory != null && p.ProductCategory.IsActive ? p.ProductCategory.SortOrder : int.MaxValue,
                    p.Version
                })
        }).ToListAsync();
        CatalogJson = JsonSerializer.Serialize(catalog, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
    public sealed record SaleListOption(long Id, string Name);
}
