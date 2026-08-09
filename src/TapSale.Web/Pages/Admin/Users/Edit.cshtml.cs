using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TapSale.Web.Data;
using TapSale.Web.Models;
using TapSale.Web.Services;

namespace TapSale.Web.Pages.Admin.Users;
public sealed class EditModel(AppDbContext db,CurrentUser current,IPasswordHasher<AppUser> hasher):PageModel
{
 [BindProperty]public InputModel Input{get;set;}=new();[BindProperty]public List<long> SelectedLists{get;set;}=[];public List<SaleList> SaleLists{get;private set;}=[];public List<UserSession> Sessions{get;private set;}=[];public bool CanDelete{get;private set;}
 public async Task<IActionResult> OnGetAsync(long? id){SaleLists=await db.SaleList.OrderBy(x=>x.Name).ToListAsync();if(id is null)return Page();var u=await db.AppUser.Include(x=>x.SaleLists).SingleOrDefaultAsync(x=>x.Id==id);if(u is null)return NotFound();Input=new(){Id=u.Id,UserName=u.UserName,DisplayName=u.DisplayName,Role=u.Role,Language=u.Language,IsActive=u.IsActive};SelectedLists=u.SaleLists.Select(x=>x.SaleListId).ToList();Sessions=await db.UserSession.Where(x=>x.UserId==id&&x.RevokedDate==null).OrderByDescending(x=>x.LastSeenDate).ToListAsync();CanDelete=!await db.Sale.AnyAsync(x=>x.UserId==id);return Page();}
 public async Task<IActionResult> OnPostSaveAsync(){if(Input.Id==0&&string.IsNullOrWhiteSpace(Input.Password))ModelState.AddModelError("Input.Password","Password is required.");if(!ModelState.IsValid){await OnGetAsync(Input.Id==0?null:Input.Id);return Page();}var normalized=Input.UserName.Trim().ToLowerInvariant();if(await db.AppUser.AnyAsync(x=>x.UserName==normalized&&x.Id!=Input.Id)){ModelState.AddModelError("Input.UserName","Username already exists.");await OnGetAsync(Input.Id==0?null:Input.Id);return Page();}AppUser u;if(Input.Id==0){u=new AppUser{UserName=normalized,DisplayName=Input.DisplayName.Trim(),PasswordHash="pending",Role=Input.Role,Language=Input.Language=="de"?"de":"en",CreateUserId=current.Id,UpdateUserId=current.Id};u.PasswordHash=hasher.HashPassword(u,Input.Password!);db.AppUser.Add(u);await db.SaveChangesAsync();}else{u=await db.AppUser.Include(x=>x.SaleLists).SingleAsync(x=>x.Id==Input.Id);u.UserName=normalized;u.DisplayName=Input.DisplayName.Trim();u.Role=Input.Role;u.Language=Input.Language=="de"?"de":"en";u.IsActive=Input.IsActive;u.UpdateUserId=current.Id;if(!string.IsNullOrWhiteSpace(Input.Password))u.PasswordHash=hasher.HashPassword(u,Input.Password);db.UserSaleList.RemoveRange(u.SaleLists);}
 foreach(var id in SelectedLists.Distinct())db.UserSaleList.Add(new UserSaleList{UserId=u.Id,SaleListId=id,CreateUserId=current.Id,UpdateUserId=current.Id});await db.SaveChangesAsync();return RedirectToPage("Edit",new{id=u.Id});}
 public async Task<IActionResult> OnPostRevokeAsync(long sessionId){var s=await db.UserSession.SingleOrDefaultAsync(x=>x.Id==sessionId);if(s is null)return NotFound();s.RevokedDate=DateTimeOffset.UtcNow;s.UpdateUserId=current.Id;await db.SaveChangesAsync();return RedirectToPage("Edit",new{id=s.UserId});}
 public async Task<IActionResult> OnPostDeleteAsync(){var u=await db.AppUser.SingleOrDefaultAsync(x=>x.Id==Input.Id);if(u is null)return NotFound();if(await db.Sale.AnyAsync(x=>x.UserId==u.Id))return BadRequest("Used users can only be archived.");db.AppUser.Remove(u);await db.SaveChangesAsync();return RedirectToPage("Index");}
 public sealed class InputModel{public long Id{get;set;}[Required,MaxLength(80)]public string UserName{get;set;}="";[Required,MaxLength(120)]public string DisplayName{get;set;}="";public UserRole Role{get;set;}=UserRole.User;public string Language{get;set;}="en";public bool IsActive{get;set;}=true;[MinLength(10),DataType(DataType.Password)]public string? Password{get;set;}}
}
