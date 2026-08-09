using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TapSale.Web.Services;
namespace TapSale.Web.Pages.Account;
public sealed class LogoutModel(AuthService auth, CurrentUser current) : PageModel
{
    public async Task<IActionResult> OnPostAsync() { await auth.SignOutAsync(HttpContext, current.SessionToken); return RedirectToPage("/Account/Login"); }
    public IActionResult OnGet() => RedirectToPage("/Sale/Index");
}
