using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Models;
using TabSale.Web.Services;

namespace TabSale.Tests;

public sealed class HistoryQueryTests
{
    [Fact]
    public async Task HistoryPage_OrdersSalesUsingSortableUtcValue()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var user = new AppUser { UserName="admin", DisplayName="Admin", PasswordHash="test", Role=UserRole.Admin };
        var saleList = new SaleList { Name="Main" };
        db.AddRange(user, saleList);
        await db.SaveChangesAsync();
        var earlier = DateTimeOffset.UtcNow.AddMinutes(-5);
        var later = DateTimeOffset.UtcNow;
        db.Sale.AddRange(
            CreateSale(user.Id, saleList.Id, earlier),
            CreateSale(user.Id, saleList.Id, later));
        await db.SaveChangesAsync();

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, nameof(UserRole.Admin))
        ], "Test");
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
        var page = new TabSale.Web.Pages.History.IndexModel(db, new CurrentUser(accessor));

        await page.OnGetAsync(null, null, null, null);

        Assert.Equal(2, page.SaleCount);
        Assert.Equal(later.ToUnixTimeMilliseconds(), page.Items[0].SoldDateUnixMilliseconds);
        Assert.Equal(earlier.ToUnixTimeMilliseconds(), page.Items[1].SoldDateUnixMilliseconds);
    }

    private static Sale CreateSale(long userId, long saleListId, DateTimeOffset soldDate) => new()
    {
        Token=Guid.NewGuid(), DeviceToken=Guid.NewGuid(), UserId=userId, SaleListId=saleListId,
        SoldDate=soldDate, SoldDateUnixMilliseconds=soldDate.ToUnixTimeMilliseconds(), TotalCents=100
    };
}
