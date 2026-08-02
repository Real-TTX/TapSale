using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Models;
using TabSale.Web.Services;
using TabSale.Web.Api;

var builder = WebApplication.CreateBuilder(args);
var dataPath = builder.Configuration["DataPath"] ?? Path.Combine(builder.Environment.ContentRootPath, "data");
Directory.CreateDirectory(dataPath);
var keysPath = Path.Combine(dataPath, "keys");
Directory.CreateDirectory(keysPath);
builder.Services.AddDataProtection().SetApplicationName("TabSale").PersistKeysToFileSystem(new DirectoryInfo(keysPath));
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={Path.Combine(dataPath, "tabsale.db")}"));
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Sale");
    options.Conventions.AuthorizeFolder("/History");
    options.Conventions.AuthorizeFolder("/Admin", "Manager");
    options.Conventions.AuthorizeFolder("/Admin/Users", "Admin");
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "TabSale.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromDays(3650);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.Events.OnValidatePrincipal = async context =>
    {
        var tokenValue = context.Principal?.FindFirstValue("session");
        if (!Guid.TryParse(tokenValue, out var token)) { context.RejectPrincipal(); return; }
        var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var session = await db.UserSession.Include(x => x.User).SingleOrDefaultAsync(x => x.Token == token);
        if (session is null || session.RevokedDate is not null || !session.User.IsActive)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
            return;
        }
        session.LastSeenDate = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
    };
});
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Manager", policy => policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Manager)))
    .AddPolicy("Admin", policy => policy.RequireRole(nameof(UserRole.Admin)));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddSingleton<AppText>();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

var app = builder.Build();

app.Use(async (context, next) =>
{
    var requested = context.Request.Cookies["TabSale.Language"];
    if (requested is not ("de" or "en"))
        requested = context.Request.GetTypedHeaders().AcceptLanguage?.Any(x => x.Value.Value?.StartsWith("de", StringComparison.OrdinalIgnoreCase) == true) == true ? "de" : "en";
    var culture = CultureInfo.GetCultureInfo(requested);
    CultureInfo.CurrentCulture = culture;
    CultureInfo.CurrentUICulture = culture;
    await next();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapSaleApi();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

public partial class Program;
