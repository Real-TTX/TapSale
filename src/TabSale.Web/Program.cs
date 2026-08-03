using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TabSale.Web.Data;
using TabSale.Web.Models;
using TabSale.Web.Services;
using TabSale.Web.Api;

var builder = WebApplication.CreateBuilder(args);
var dataPath = builder.Configuration["DataPath"] ?? Path.Combine(builder.Environment.ContentRootPath, "data");
Directory.CreateDirectory(dataPath);
var keysPath = Path.Combine(dataPath, "keys");
Directory.CreateDirectory(keysPath);
var uploadsPath = Path.Combine(dataPath, "uploads");
Directory.CreateDirectory(uploadsPath);
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
builder.Services.AddSingleton(new ThemeConfigStore(Path.Combine(dataPath, "theme.json")));
builder.Services.AddSingleton(new ProductImageStorage(uploadsPath));
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

app.Use(async (context, next) =>
{
    var requestPath = context.Request.Path.Value ?? "";
    var extension = Path.GetExtension(requestPath);
    var fileName = Path.GetFileNameWithoutExtension(requestPath);
    var fingerprintSeparator = fileName.LastIndexOf('.');
    var fingerprint = fingerprintSeparator >= 0 ? fileName[(fingerprintSeparator + 1)..] : "";
    var hasFingerprint = fingerprint.Length >= 8 && fingerprint.All(char.IsLetterOrDigit);
    var isCacheableAsset = extension is ".css" or ".js" or ".svg" or ".png" or ".ico" or ".webmanifest";
    var isVersionedAsset = isCacheableAsset && (hasFingerprint || context.Request.Query.ContainsKey("v"));
    if (isVersionedAsset)
    {
        context.Response.OnStarting(() =>
        {
            if (context.Response.StatusCode == StatusCodes.Status200OK)
                context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
            return Task.CompletedTask;
        });
    }

    await next();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
        context.Context.Response.Headers.XContentTypeOptions = "nosniff";
    }
});
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
