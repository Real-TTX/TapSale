using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Services;

namespace TabSale.Web.Pages.Account;
public sealed class LoginModel(AppDbContext db, AuthService auth) : PageModel
{
    [BindProperty] public LoginInput Input { get; set; } = new();
    public async Task<IActionResult> OnGetAsync()
    {
        if (!await db.AppUser.AnyAsync()) return RedirectToPage("/Account/Setup");
        if (User.Identity?.IsAuthenticated == true) return RedirectToPage("/Sale/Index");
        return Page();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var user = await auth.ValidateAsync(Input.UserName, Input.Password);
        if (user is null) { ModelState.AddModelError("", "Invalid username or password."); return Page(); }
        await auth.SignInAsync(HttpContext, user, Request.Headers.UserAgent.ToString());
        Response.Cookies.Append("TabSale.Language", user.Language, new CookieOptions { IsEssential = true, MaxAge = TimeSpan.FromDays(3650), SameSite = SameSiteMode.Lax });
        return RedirectToPage("/Sale/Index");
    }
    public sealed class LoginInput { [Required] public string UserName { get; set; } = ""; [Required, DataType(DataType.Password)] public string Password { get; set; } = ""; }
}
