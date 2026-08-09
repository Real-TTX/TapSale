using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TapSale.Web.Data;
using TapSale.Web.Models;
using TapSale.Web.Services;

namespace TapSale.Web.Pages.History;

public sealed class IndexModel(AppDbContext db, CurrentUser current) : PageModel
{
    public List<TapSale.Web.Models.Sale> Items { get; private set; } = [];
    public List<CashShift> Shifts { get; private set; } = [];
    public List<SaleList> Lists { get; private set; } = [];
    public int PageNumber { get; private set; } = 1;
    public int TotalPages { get; private set; } = 1;
    public long RevenueCents { get; private set; }
    public int SaleCount { get; private set; }

    public async Task OnGetAsync(DateTime? from, DateTime? to, long? saleListId, string? search, int pageNumber = 1)
    {
        var query = Filter(Allowed().Include(x => x.User).Include(x => x.SaleList).Include(x => x.CashShift).Include(x => x.Lines).AsNoTracking(), from, to, saleListId, search);
        RevenueCents = await query.SumAsync(x => (long?)x.TotalCents) ?? 0;
        SaleCount = await query.CountAsync();
        PageNumber = Math.Max(1, pageNumber);
        TotalPages = Math.Max(1, (int)Math.Ceiling(SaleCount / 25d));
        Items = await query.OrderByDescending(x => x.SoldDateUnixMilliseconds).ThenByDescending(x => x.Id)
            .Skip((PageNumber - 1) * 25).Take(25).ToListAsync();
        Lists = await AllowedLists().AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        Shifts = await LoadShifts(from, to, saleListId, search);
    }

    public async Task<IActionResult> OnGetExportAsync(DateTime? from, DateTime? to, long? saleListId, string? search)
    {
        var items = await Filter(Allowed().Include(x => x.User).Include(x => x.SaleList).Include(x => x.CashShift).Include(x => x.Lines).AsNoTracking(), from, to, saleListId, search)
            .OrderBy(x => x.SoldDateUnixMilliseconds).ThenBy(x => x.Id).ToListAsync();
        var german = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de";
        var header = german
            ? "Zeitstempel;Beleg;Benutzer;Verkaufsliste;Schicht;Typ;Produkt;Menge;EinzelpreisEUR;PositionssummeEUR\r\n"
            : "Timestamp;Receipt;User;SaleList;Shift;Type;Product;Quantity;UnitPriceEUR;LineTotalEUR\r\n";
        var csv = new StringBuilder(header);
        foreach (var sale in items)
        foreach (var line in sale.Lines)
            csv.AppendJoin(';', Csv(sale.SoldDate.ToLocalTime().ToString("O")), sale.Token, Csv(sale.User.DisplayName), Csv(sale.SaleList.Name), Csv(sale.CashShift?.Name), sale.Kind,
                Csv(line.ProductName), line.Quantity, (line.UnitPriceCents / 100m).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                (line.LineTotalCents / 100m).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).Append("\r\n");
        return File(new UTF8Encoding(true).GetBytes(csv.ToString()), "text/csv", $"tapsale-{DateTime.Now:yyyyMMdd-HHmm}.csv");
    }

    private IQueryable<TapSale.Web.Models.Sale> Allowed()
    {
        var query = db.Sale.AsQueryable();
        if (current.Role == UserRole.User) return query.Where(x => x.UserId == current.Id);
        if (current.Role == UserRole.Manager) return query.Where(x => x.SaleList.Users.Any(assignment => assignment.UserId == current.Id));
        return query;
    }

    private IQueryable<SaleList> AllowedLists() => current.IsAdmin
        ? db.SaleList
        : db.SaleList.Where(x => x.Users.Any(assignment => assignment.UserId == current.Id));

    private IQueryable<CashShift> AllowedShifts()
    {
        var query = db.CashShift.AsQueryable();
        if (current.Role == UserRole.User) return query.Where(x => x.UserId == current.Id);
        if (current.Role == UserRole.Manager) return query.Where(x => x.SaleList.Users.Any(assignment => assignment.UserId == current.Id));
        return query;
    }

    private async Task<List<CashShift>> LoadShifts(DateTime? from, DateTime? to, long? saleListId, string? search)
    {
        var query = AllowedShifts().Include(x => x.User).Include(x => x.SaleList).Include(x => x.Sales).AsNoTracking();
        if (saleListId.HasValue) query = query.Where(x => x.SaleListId == saleListId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => (x.Name != null && x.Name.Contains(term)) || x.User.DisplayName.Contains(term) || x.SaleList.Name.Contains(term));
        }

        var shifts = await query.OrderByDescending(x => x.Id).ToListAsync();
        if (from.HasValue) shifts = shifts.Where(x => x.OpenedDate.ToLocalTime().Date >= from.Value.Date).ToList();
        if (to.HasValue) shifts = shifts.Where(x => x.OpenedDate.ToLocalTime().Date <= to.Value.Date).ToList();
        return shifts.Take(50).ToList();
    }

    private static IQueryable<TapSale.Web.Models.Sale> Filter(IQueryable<TapSale.Web.Models.Sale> query, DateTime? from, DateTime? to, long? listId, string? search)
    {
        if (from.HasValue) query = query.Where(x => x.SoldDateUnixMilliseconds >= LocalDateBoundary(from.Value.Date));
        if (to.HasValue) query = query.Where(x => x.SoldDateUnixMilliseconds < LocalDateBoundary(to.Value.Date.AddDays(1)));
        if (listId.HasValue) query = query.Where(x => x.SaleListId == listId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            if (Guid.TryParse(term, out var token))
                query = query.Where(x => x.User.DisplayName.Contains(term) || x.SaleList.Name.Contains(term) || x.Token == token || x.Lines.Any(line => line.ProductName.Contains(term)) || (x.CashShift != null && x.CashShift.Name != null && x.CashShift.Name.Contains(term)));
            else
                query = query.Where(x => x.User.DisplayName.Contains(term) || x.SaleList.Name.Contains(term) || x.Lines.Any(line => line.ProductName.Contains(term)) || (x.CashShift != null && x.CashShift.Name != null && x.CashShift.Name.Contains(term)));
        }
        return query;
    }

    private static long LocalDateBoundary(DateTime date)
    {
        var localDate = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
        return new DateTimeOffset(localDate, TimeZoneInfo.Local.GetUtcOffset(localDate)).ToUnixTimeMilliseconds();
    }

    private static string Csv(object? value) => $"\"{value?.ToString()?.Replace("\"", "\"\"")}\"";
}
