using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Models;
using TabSale.Web.Services;

namespace TabSale.Web.Pages.Admin.SaleLists;
public sealed class EditModel(AppDbContext db, CurrentUser current) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public List<Product> Products { get; private set; } = [];
    public bool CanDelete { get; private set; }
    public async Task<IActionResult> OnGetAsync(long? id)
    {
        if (id is null) { if (!current.IsAdmin) return Forbid(); return Page(); }
        var entity = await Allowed().Include(x => x.Products).SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        Input = new() { Id = entity.Id, Name = entity.Name, IsActive = entity.IsActive }; Products = entity.Products.OrderBy(x => x.SortOrder).ToList();
        CanDelete = current.IsAdmin && !await db.Sale.AnyAsync(x => x.SaleListId == id) && entity.Products.Count == 0; return Page();
    }
    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid) return await OnGetAsync(Input.Id == 0 ? null : Input.Id);
        SaleList entity;
        if (Input.Id == 0) { if (!current.IsAdmin) return Forbid(); entity = new SaleList { Name = Input.Name.Trim(), CreateUserId = current.Id, UpdateUserId = current.Id }; db.SaleList.Add(entity); }
        else { entity = await Allowed().SingleOrDefaultAsync(x => x.Id == Input.Id) ?? throw new InvalidOperationException(); entity.Name = Input.Name.Trim(); entity.IsActive = Input.IsActive; entity.UpdateUserId = current.Id; }
        await db.SaveChangesAsync(); return RedirectToPage("Edit", new { id = entity.Id });
    }
    public async Task<IActionResult> OnPostDeleteAsync()
    {
        if (!current.IsAdmin) return Forbid();
        var entity = await db.SaleList.Include(x => x.Products).SingleOrDefaultAsync(x => x.Id == Input.Id); if (entity is null) return NotFound();
        if (entity.Products.Count > 0 || await db.Sale.AnyAsync(x => x.SaleListId == entity.Id)) return BadRequest("Used sale lists can only be archived.");
        db.SaleList.Remove(entity); await db.SaveChangesAsync(); return RedirectToPage("Index");
    }
    private IQueryable<SaleList> Allowed() => current.IsAdmin ? db.SaleList : db.SaleList.Where(x => x.Users.Any(a => a.UserId == current.Id));
    public sealed class InputModel { public long Id { get; set; } [Required, MaxLength(120)] public string Name { get; set; } = ""; public bool IsActive { get; set; } = true; }
}
