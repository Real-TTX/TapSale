using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Models;
using TabSale.Web.Services;

namespace TabSale.Web.Pages.Admin.SaleLists;
public sealed class IndexModel(AppDbContext db, CurrentUser current) : PageModel
{
    public List<SaleList> Items { get; private set; } = [];
    public async Task OnGetAsync(string? search, string? status)
    {
        var query = db.SaleList.AsNoTracking().Include(x => x.Products).AsQueryable();
        if (!current.IsAdmin) query = query.Where(x => x.Users.Any(a => a.UserId == current.Id));
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search));
        if (status == "active") query = query.Where(x => x.IsActive); else if (status == "archived") query = query.Where(x => !x.IsActive);
        Items = await query.OrderBy(x => x.Name).ToListAsync();
    }
}
