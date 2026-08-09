using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TapSale.Web.Data;
using TapSale.Web.Models;
using TapSale.Web.Services;

namespace TapSale.Web.Pages.Account;
public sealed class SetupModel(AppDbContext db, IPasswordHasher<AppUser> hasher, AuthService auth) : PageModel
{
    [BindProperty] public SetupInput Input { get; set; } = new();
    public async Task<IActionResult> OnGetAsync() => await db.AppUser.AnyAsync() ? NotFound() : Page();
    public async Task<IActionResult> OnPostAsync()
    {
        if (await db.AppUser.AnyAsync()) return NotFound();
        if (!ModelState.IsValid) return Page();
        var user = new AppUser { UserName = Input.UserName.Trim().ToLowerInvariant(), DisplayName = Input.DisplayName.Trim(), PasswordHash = "pending", Role = UserRole.Admin, Language = Input.Language == "de" ? "de" : "en" };
        user.PasswordHash = hasher.HashPassword(user, Input.Password);
        db.AppUser.Add(user); await db.SaveChangesAsync();
        user.CreateUserId = user.Id; user.UpdateUserId = user.Id; await db.SaveChangesAsync();
        await auth.SignInAsync(HttpContext, user, Request.Headers.UserAgent.ToString());
        Response.Cookies.Append("TapSale.Language", user.Language, new CookieOptions { IsEssential = true, MaxAge = TimeSpan.FromDays(3650), SameSite = SameSiteMode.Lax });
        return RedirectToPage("/Sale/Index");
    }
    public sealed class SetupInput
    {
        [Required, MaxLength(120)] public string DisplayName { get; set; } = "";
        [Required, MinLength(3), MaxLength(80)] public string UserName { get; set; } = "";
        [Required, MinLength(10), DataType(DataType.Password)] public string Password { get; set; } = "";
        [Required] public string Language { get; set; } = "en";
    }
}
