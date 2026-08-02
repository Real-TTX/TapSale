using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Models;

namespace TabSale.Web.Services;

public sealed class AuthService(AppDbContext db, IPasswordHasher<AppUser> hasher)
{
    public async Task<AppUser?> ValidateAsync(string userName, string password)
    {
        var normalized = userName.Trim().ToLowerInvariant();
        var user = await db.AppUser.SingleOrDefaultAsync(x => x.UserName == normalized && x.IsActive);
        if (user is null) return null;
        return hasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed ? null : user;
    }

    public async Task SignInAsync(HttpContext context, AppUser user, string deviceName)
    {
        var session = new UserSession
        {
            UserId = user.Id, Token = Guid.NewGuid(), DeviceName = deviceName.Trim()[..Math.Min(deviceName.Trim().Length, 120)],
            LastSeenDate = DateTimeOffset.UtcNow, CreateUserId = user.Id, UpdateUserId = user.Id
        };
        db.UserSession.Add(session);
        await db.SaveChangesAsync();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role.ToString()), new Claim("session", session.Token.ToString())
        };
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = true, AllowRefresh = true });
    }

    public async Task SignOutAsync(HttpContext context, Guid token)
    {
        var session = await db.UserSession.SingleOrDefaultAsync(x => x.Token == token);
        if (session is not null) session.RevokedDate = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        await context.SignOutAsync();
    }
}
