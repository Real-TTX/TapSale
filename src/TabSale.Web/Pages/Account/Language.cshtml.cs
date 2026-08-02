using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Services;
namespace TabSale.Web.Pages.Account;
public sealed class LanguageModel(AppDbContext db, CurrentUser current) : PageModel
{
    public async Task<IActionResult> OnPostAsync(string language, string? returnUrl)
    {
        var value = language == "de" ? "de" : "en";
        Response.Cookies.Append("TabSale.Language", value, new CookieOptions { IsEssential = true, MaxAge = TimeSpan.FromDays(3650), SameSite = SameSiteMode.Lax });
        if (current.Id != 0)
        {
            var user = await db.AppUser.SingleOrDefaultAsync(x => x.Id == current.Id);
            if (user is not null) { user.Language = value; user.UpdateUserId = current.Id; await db.SaveChangesAsync(); }
        }
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
    }
}
