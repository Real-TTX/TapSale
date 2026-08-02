using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Models;
using TabSale.Web.Services;
namespace TabSale.Web.Pages.History;
public sealed class DetailsModel(AppDbContext db,CurrentUser current):PageModel
{
 public TabSale.Web.Models.Sale Item{get;private set;}=null!;public bool CanCancel{get;private set;}
 public async Task<IActionResult> OnGetAsync(long id){var q=db.Sale.Include(x=>x.User).Include(x=>x.SaleList).Include(x=>x.Lines).AsQueryable();if(current.Role==UserRole.User)q=q.Where(x=>x.UserId==current.Id);else if(current.Role==UserRole.Manager)q=q.Where(x=>x.SaleList.Users.Any(a=>a.UserId==current.Id));var item=await q.SingleOrDefaultAsync(x=>x.Id==id);if(item is null)return NotFound();Item=item;CanCancel=current.IsManager&&item.Kind==SaleKind.Sale&&!await db.Sale.AnyAsync(x=>x.OriginalSaleId==id);return Page();}
}
