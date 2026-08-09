using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TapSale.Web.Data;
using TapSale.Web.Models;

namespace TapSale.Tests;

public sealed class CategoryDisplayModeTests
{
    [Theory]
    [InlineData(CategoryDisplayMode.Filter)]
    [InlineData(CategoryDisplayMode.Sections)]
    [InlineData(CategoryDisplayMode.Drilldown)]
    public async Task SaleList_PersistsCategoryDisplayMode(CategoryDisplayMode mode)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var saleList = new SaleList { Name="Main", CategoryDisplayMode=mode };
        db.SaleList.Add(saleList);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var stored = await db.SaleList.SingleAsync();
        Assert.Equal(mode, stored.CategoryDisplayMode);
    }
}
