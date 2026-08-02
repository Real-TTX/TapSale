using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Models;
using TabSale.Web.Services;
namespace TabSale.Web.Pages.Sale;
public sealed class ShiftModel(AppDbContext db,CurrentUser current):PageModel
{
 [BindProperty]public long SaleListId{get;set;}[BindProperty,Range(0,1000000)]public decimal Amount{get;set;}public List<SaleList> Lists{get;private set;}=[];public CashShift? Active{get;private set;}public long ExpectedCents{get;private set;}
 public async Task OnGetAsync(long? saleListId){Lists=await AllowedLists().OrderBy(x=>x.Name).ToListAsync();SaleListId=Lists.Any(x=>x.Id==saleListId)?saleListId!.Value:Lists.FirstOrDefault()?.Id??0;Active=await db.CashShift.AsNoTracking().SingleOrDefaultAsync(x=>x.UserId==current.Id&&x.SaleListId==SaleListId&&x.ClosedDate==null);if(Active is not null)ExpectedCents=Active.OpeningCents+(await db.Sale.Where(x=>x.CashShiftId==Active.Id).SumAsync(x=>(long?)x.TotalCents)??0);}
 public async Task<IActionResult> OnPostOpenAsync(){if(!await AllowedLists().AnyAsync(x=>x.Id==SaleListId))return Forbid();if(await db.CashShift.AnyAsync(x=>x.UserId==current.Id&&x.SaleListId==SaleListId&&x.ClosedDate==null))return StatusCode(409);db.CashShift.Add(new CashShift{Token=Guid.NewGuid(),UserId=current.Id,SaleListId=SaleListId,OpeningCents=decimal.ToInt64(Amount*100),OpenedDate=DateTimeOffset.UtcNow,CreateUserId=current.Id,UpdateUserId=current.Id});await db.SaveChangesAsync();return RedirectToPage(new{saleListId=SaleListId});}
 public async Task<IActionResult> OnPostCloseAsync(long id){var shift=await db.CashShift.SingleOrDefaultAsync(x=>x.Id==id&&x.UserId==current.Id&&x.ClosedDate==null);if(shift is null)return NotFound();shift.CountedClosingCents=decimal.ToInt64(Amount*100);shift.ClosedDate=DateTimeOffset.UtcNow;shift.UpdateUserId=current.Id;await db.SaveChangesAsync();return RedirectToPage(new{saleListId=shift.SaleListId});}
 private IQueryable<SaleList> AllowedLists()=>current.IsAdmin?db.SaleList.Where(x=>x.IsActive):db.SaleList.Where(x=>x.IsActive&&x.Users.Any(a=>a.UserId==current.Id));
}
