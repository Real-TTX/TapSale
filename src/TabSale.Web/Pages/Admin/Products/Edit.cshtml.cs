using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TabSale.Web.Data;
using TabSale.Web.Models;
using TabSale.Web.Services;

namespace TabSale.Web.Pages.Admin.Products;
public sealed class EditModel(AppDbContext db, CurrentUser current) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new(); public bool CanDelete { get; private set; }
    public async Task<IActionResult> OnGetAsync(long? id, long? saleListId)
    {
        if (id is null) { if (saleListId is null || !await CanEditList(saleListId.Value)) return Forbid(); Input.SaleListId = saleListId.Value; return Page(); }
        var p = await db.Product.SingleOrDefaultAsync(x => x.Id == id); if (p is null || !await CanEditList(p.SaleListId)) return NotFound();
        Input = new() { Id=p.Id,SaleListId=p.SaleListId,Name=p.Name,Price=p.UnitPriceCents/100m,Kind=p.Kind,Color=p.Color,Icon=p.Icon,SortOrder=p.SortOrder,IsActive=p.IsActive };
        CanDelete = !await db.SaleLine.AnyAsync(x => x.ProductId == p.Id); return Page();
    }
    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid || !await CanEditList(Input.SaleListId)) return Page();
        Product entity;
        var cents = decimal.ToInt64(decimal.Round(Input.Price * 100, 0, MidpointRounding.AwayFromZero));
        if (Input.Id == 0) { entity = new Product { SaleListId=Input.SaleListId,Name=Input.Name.Trim(),UnitPriceCents=cents,Kind=Input.Kind,Color=Input.Color,Icon=Input.Icon,SortOrder=Input.SortOrder,CreateUserId=current.Id,UpdateUserId=current.Id }; db.Product.Add(entity); await db.SaveChangesAsync(); }
        else { entity = await db.Product.SingleAsync(x=>x.Id==Input.Id); var changed=entity.Name!=Input.Name.Trim()||entity.UnitPriceCents!=cents||entity.Kind!=Input.Kind; entity.Name=Input.Name.Trim();entity.UnitPriceCents=cents;entity.Kind=Input.Kind;entity.Color=Input.Color;entity.Icon=Input.Icon;entity.SortOrder=Input.SortOrder;entity.IsActive=Input.IsActive;entity.UpdateUserId=current.Id;if(changed)entity.Version++; await db.SaveChangesAsync(); }
        if (!await db.ProductPriceVersion.AnyAsync(x=>x.ProductId==entity.Id&&x.Version==entity.Version)) { db.ProductPriceVersion.Add(new ProductPriceVersion{ProductId=entity.Id,Version=entity.Version,Name=entity.Name,UnitPriceCents=entity.UnitPriceCents,Kind=entity.Kind,CreateUserId=current.Id,UpdateUserId=current.Id}); await db.SaveChangesAsync(); }
        return RedirectToPage("/Admin/SaleLists/Edit",new{id=entity.SaleListId});
    }
    public async Task<IActionResult> OnPostDeleteAsync(){var p=await db.Product.SingleOrDefaultAsync(x=>x.Id==Input.Id);if(p is null||!await CanEditList(p.SaleListId))return NotFound();if(await db.SaleLine.AnyAsync(x=>x.ProductId==p.Id))return BadRequest("Used products can only be archived.");db.Product.Remove(p);await db.SaveChangesAsync();return RedirectToPage("/Admin/SaleLists/Edit",new{id=p.SaleListId});}
    private Task<bool> CanEditList(long id)=>current.IsAdmin?db.SaleList.AnyAsync(x=>x.Id==id):db.UserSaleList.AnyAsync(x=>x.UserId==current.Id&&x.SaleListId==id);
    public sealed class InputModel{public long Id{get;set;}public long SaleListId{get;set;}[Required,MaxLength(120)]public string Name{get;set;}="";[Range(0,100000)]public decimal Price{get;set;}public ProductKind Kind{get;set;}public string Color{get;set;}="#167D6D";public string Icon{get;set;}="tag";public int SortOrder{get;set;}public bool IsActive{get;set;}=true;}
}
