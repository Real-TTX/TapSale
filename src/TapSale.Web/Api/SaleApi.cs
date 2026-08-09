using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapSale.Web.Data;
using TapSale.Web.Models;
using TapSale.Web.Services;

namespace TapSale.Web.Api;

public static class SaleApi
{
    public static IEndpointRouteBuilder MapSaleApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization();
        group.MapPost("/sales/sync", SyncSales);
        group.MapPost("/sales/{id:long}/cancel", CancelSale).RequireAuthorization("Manager");
        group.MapPost("/shifts/open", OpenShift);
        group.MapPost("/shifts/{token:guid}/close", CloseShift);
        return app;
    }

    private static async Task<IResult> SyncSales(HttpContext http, SyncRequest request, AppDbContext db, CurrentUser current, IAntiforgery antiforgery)
    {
        await antiforgery.ValidateRequestAsync(http);
        if (request.Sales.Count > 100) return Results.BadRequest(new { error = "Too many sales." });
        var accepted = new List<Guid>();
        foreach (var input in request.Sales)
        {
            if (await db.Sale.AnyAsync(x => x.Token == input.Token)) { accepted.Add(input.Token); continue; }
            if (!await CanUseList(db, current, input.SaleListId)) return Results.Forbid();
            if (input.Lines.Count == 0 || input.Lines.Any(x => x.Quantity <= 0 || x.Quantity > 999)) return Results.BadRequest(new { error = "Invalid lines." });
            var keys = input.Lines.Select(x => new { x.ProductId, x.Version }).ToList();
            var productIds = keys.Select(x => x.ProductId).Distinct().ToList();
            var versions = await db.ProductPriceVersion.Where(x => productIds.Contains(x.ProductId)).ToListAsync();
            var soldDate = input.SoldDate.ToUniversalTime();
            var sale = new Models.Sale
            {
                Token = input.Token, DeviceToken = input.DeviceToken, UserId = current.Id, SaleListId = input.SaleListId,
                SoldDate = soldDate, SoldDateUnixMilliseconds = soldDate.ToUnixTimeMilliseconds(), Kind = SaleKind.Sale, TenderedCents = input.TenderedCents,
                CashShiftId = await db.CashShift.Where(x => x.UserId == current.Id && x.SaleListId == input.SaleListId && x.ClosedDate == null).Select(x => (long?)x.Id).FirstOrDefaultAsync(),
                CreateUserId = current.Id, UpdateUserId = current.Id
            };
            foreach (var line in input.Lines)
            {
                var version = versions.SingleOrDefault(x => x.ProductId == line.ProductId && x.Version == line.Version);
                if (version is null) return Results.BadRequest(new { error = "Unknown product version." });
                var total = SaleCalculator.LineTotal(version.Kind, version.UnitPriceCents, line.Quantity);
                sale.Lines.Add(new SaleLine { ProductId = line.ProductId, ProductName = version.Name, ProductKind = version.Kind, ProductVersion = version.Version, UnitPriceCents = version.UnitPriceCents, Quantity = line.Quantity, LineTotalCents = total, CreateUserId = current.Id, UpdateUserId = current.Id });
            }
            sale.TotalCents = sale.Lines.Sum(x => x.LineTotalCents);
            if (sale.TotalCents > 0)
            {
                if (sale.TenderedCents is null || sale.TenderedCents < sale.TotalCents) return Results.BadRequest(new { error = "Insufficient cash." });
                sale.ChangeCents = SaleCalculator.Change(sale.TotalCents, sale.TenderedCents.Value);
            }
            else { sale.TenderedCents = null; sale.ChangeCents = null; }
            db.Sale.Add(sale); await db.SaveChangesAsync(); accepted.Add(input.Token);
        }
        var session = await db.UserSession.SingleOrDefaultAsync(x => x.Token == current.SessionToken);
        if (session is not null) { session.LastSyncDate = DateTimeOffset.UtcNow; await db.SaveChangesAsync(); }
        return Results.Ok(new { accepted });
    }

    private static async Task<IResult> CancelSale(long id, HttpContext http, AppDbContext db, CurrentUser current, IAntiforgery antiforgery)
    {
        await antiforgery.ValidateRequestAsync(http);
        var original = await db.Sale.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id);
        if (original is null) return Results.NotFound();
        if (!current.IsAdmin && !await CanUseList(db, current, original.SaleListId)) return Results.Forbid();
        if (original.Kind == SaleKind.Cancellation || await db.Sale.AnyAsync(x => x.OriginalSaleId == id)) return Results.Conflict(new { error = "Already cancelled." });
        var cancellationDate = DateTimeOffset.UtcNow;
        var cancellation = new Models.Sale
        {
            Token = Guid.NewGuid(), DeviceToken = current.SessionToken, UserId = current.Id, SaleListId = original.SaleListId, CashShiftId = original.CashShiftId, Kind = SaleKind.Cancellation,
            OriginalSaleId = original.Id, SoldDate = cancellationDate, SoldDateUnixMilliseconds = cancellationDate.ToUnixTimeMilliseconds(), TotalCents = -original.TotalCents, CreateUserId = current.Id, UpdateUserId = current.Id,
            Lines = original.Lines.Select(x => new SaleLine { ProductId = x.ProductId, ProductName = x.ProductName, ProductKind = x.ProductKind, ProductVersion = x.ProductVersion, UnitPriceCents = x.UnitPriceCents, Quantity = x.Quantity, LineTotalCents = -x.LineTotalCents, CreateUserId = current.Id, UpdateUserId = current.Id }).ToList()
        };
        db.Sale.Add(cancellation); await db.SaveChangesAsync(); return Results.Ok(new { cancellation.Id });
    }

    private static async Task<IResult> OpenShift(HttpContext http, ShiftOpenRequest request, AppDbContext db, CurrentUser current, IAntiforgery antiforgery)
    {
        await antiforgery.ValidateRequestAsync(http);
        if (!await CanUseList(db, current, request.SaleListId)) return Results.Forbid();
        if (await db.CashShift.AnyAsync(x => x.UserId == current.Id && x.SaleListId == request.SaleListId && x.ClosedDate == null)) return Results.Conflict();
        var shift = new CashShift { Token = Guid.NewGuid(), UserId = current.Id, SaleListId = request.SaleListId, OpeningCents = request.OpeningCents, OpenedDate = DateTimeOffset.UtcNow, CreateUserId = current.Id, UpdateUserId = current.Id };
        db.CashShift.Add(shift); await db.SaveChangesAsync(); return Results.Ok(new { shift.Token });
    }

    private static async Task<IResult> CloseShift(Guid token, HttpContext http, ShiftCloseRequest request, AppDbContext db, CurrentUser current, IAntiforgery antiforgery)
    {
        await antiforgery.ValidateRequestAsync(http);
        var shift = await db.CashShift.SingleOrDefaultAsync(x => x.Token == token && x.UserId == current.Id && x.ClosedDate == null);
        if (shift is null) return Results.NotFound();
        shift.Name = NormalizeShiftName(request.Name);
        shift.CountedClosingCents = request.CountedCents; shift.ClosedDate = DateTimeOffset.UtcNow; shift.UpdateUserId = current.Id;
        await db.SaveChangesAsync(); return Results.Ok();
    }

    private static Task<bool> CanUseList(AppDbContext db, CurrentUser current, long listId) => current.IsAdmin
        ? db.SaleList.AnyAsync(x => x.Id == listId && x.IsActive)
        : db.UserSaleList.AnyAsync(x => x.UserId == current.Id && x.SaleListId == listId && x.SaleList.IsActive);

    public sealed record SyncRequest(List<SaleInput> Sales);
    public sealed record SaleInput(Guid Token, Guid DeviceToken, long SaleListId, DateTimeOffset SoldDate, long? TenderedCents, List<SaleLineInput> Lines);
    public sealed record SaleLineInput(long ProductId, int Version, int Quantity);
    public sealed record ShiftOpenRequest(long SaleListId, long OpeningCents);
    public sealed record ShiftCloseRequest(long CountedCents, string? Name);

    private static string? NormalizeShiftName(string? name)
    {
        var value = name?.Trim();
        return string.IsNullOrEmpty(value) ? null : value[..Math.Min(value.Length, 120)];
    }
}
