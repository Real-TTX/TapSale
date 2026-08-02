using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Models;
namespace TabSale.Web.Pages.Admin.Users;
public sealed class IndexModel(AppDbContext db):PageModel
{
 public List<AppUser> Items{get;private set;}=[];
 public async Task OnGetAsync(string? search,string? role,string? status){var q=db.AppUser.AsNoTracking().AsQueryable();if(!string.IsNullOrWhiteSpace(search))q=q.Where(x=>x.UserName.Contains(search)||x.DisplayName.Contains(search));if(Enum.TryParse<UserRole>(role,out var parsed))q=q.Where(x=>x.Role==parsed);if(status=="active")q=q.Where(x=>x.IsActive);else if(status=="archived")q=q.Where(x=>!x.IsActive);Items=await q.OrderBy(x=>x.DisplayName).ToListAsync();}
}
