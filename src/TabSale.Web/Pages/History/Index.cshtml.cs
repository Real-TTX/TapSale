using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Models;
using TabSale.Web.Services;

namespace TabSale.Web.Pages.History;
public sealed class IndexModel(AppDbContext db,CurrentUser current):PageModel
{
 public List<TabSale.Web.Models.Sale> Items{get;private set;}=[];public List<SaleList> Lists{get;private set;}=[];public int PageNumber{get;private set;}=1;public int TotalPages{get;private set;}=1;public long RevenueCents{get;private set;}public int SaleCount{get;private set;}
 public async Task OnGetAsync(DateTime? from,DateTime? to,long? saleListId,string? search,int pageNumber=1){var query=Allowed().Include(x=>x.User).Include(x=>x.SaleList).AsNoTracking();query=Filter(query,from,to,saleListId,search);RevenueCents=await query.SumAsync(x=>(long?)x.TotalCents)??0;SaleCount=await query.CountAsync();PageNumber=Math.Max(1,pageNumber);TotalPages=Math.Max(1,(int)Math.Ceiling(SaleCount/25d));Items=await query.OrderByDescending(x=>x.SoldDate).Skip((PageNumber-1)*25).Take(25).ToListAsync();Lists=await AllowedLists().OrderBy(x=>x.Name).ToListAsync();}
 public async Task<IActionResult> OnGetExportAsync(DateTime? from,DateTime? to,long? saleListId,string? search){var items=await Filter(Allowed().Include(x=>x.User).Include(x=>x.SaleList).Include(x=>x.Lines).AsNoTracking(),from,to,saleListId,search).OrderBy(x=>x.SoldDate).ToListAsync();var de=System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName=="de";var header=de?"Zeitstempel;Beleg;Benutzer;Verkaufsliste;Typ;Produkt;Menge;EinzelpreisEUR;PositionssummeEUR\r\n":"Timestamp;Receipt;User;SaleList;Type;Product;Quantity;UnitPriceEUR;LineTotalEUR\r\n";var sb=new StringBuilder(header);foreach(var sale in items)foreach(var line in sale.Lines)sb.AppendJoin(';',Csv(sale.SoldDate.ToLocalTime().ToString("O")),sale.Token,Csv(sale.User.DisplayName),Csv(sale.SaleList.Name),sale.Kind,Csv(line.ProductName),line.Quantity,(line.UnitPriceCents/100m).ToString("0.00",System.Globalization.CultureInfo.InvariantCulture),(line.LineTotalCents/100m).ToString("0.00",System.Globalization.CultureInfo.InvariantCulture)).Append("\r\n");return File(new UTF8Encoding(true).GetBytes(sb.ToString()),"text/csv",$"tabsale-{DateTime.Now:yyyyMMdd-HHmm}.csv");}
 private IQueryable<TabSale.Web.Models.Sale> Allowed(){var q=db.Sale.AsQueryable();if(current.Role==UserRole.User)return q.Where(x=>x.UserId==current.Id);if(current.Role==UserRole.Manager)return q.Where(x=>x.SaleList.Users.Any(a=>a.UserId==current.Id));return q;}
 private IQueryable<SaleList> AllowedLists()=>current.IsAdmin?db.SaleList:db.SaleList.Where(x=>x.Users.Any(a=>a.UserId==current.Id));
 private static IQueryable<TabSale.Web.Models.Sale> Filter(IQueryable<TabSale.Web.Models.Sale> q,DateTime? from,DateTime? to,long? listId,string? search){if(from.HasValue)q=q.Where(x=>x.SoldDate>=new DateTimeOffset(from.Value.Date,TimeZoneInfo.Local.GetUtcOffset(from.Value.Date)));if(to.HasValue){var end=to.Value.Date.AddDays(1);q=q.Where(x=>x.SoldDate<new DateTimeOffset(end,TimeZoneInfo.Local.GetUtcOffset(end)));}if(listId.HasValue)q=q.Where(x=>x.SaleListId==listId);if(!string.IsNullOrWhiteSpace(search))q=q.Where(x=>x.User.DisplayName.Contains(search)||x.SaleList.Name.Contains(search)||x.Token.ToString().Contains(search));return q;}
 private static string Csv(object? value)=>$"\"{value?.ToString()?.Replace("\"","\"\"")}\"";
}
