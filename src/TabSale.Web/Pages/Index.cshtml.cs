using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;

namespace TabSale.Web.Pages;
public sealed class IndexModel(AppDbContext db) : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        if (!await db.AppUser.AnyAsync()) return RedirectToPage("/Account/Setup");
        return User.Identity?.IsAuthenticated == true ? RedirectToPage("/Sale/Index") : RedirectToPage("/Account/Login");
    }
}
