using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TapSale.Web.Services;

namespace TapSale.Web.Pages.Admin.Appearance;

[Authorize(Policy = "Admin")]
public sealed class IndexModel(ThemeConfigStore store) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public bool Saved { get; private set; }

    public void OnGet() => Input = InputModel.From(store.Current);

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();
        store.Save(Input.ToConfig());
        Saved = true;
        Input = InputModel.From(store.Current);
        return Page();
    }

    public sealed class InputModel
    {
        [Required] public string Theme { get; set; } = "classic";
        [Required, RegularExpression("^#[0-9a-fA-F]{6}$")] public string Ink { get; set; } = "#102a2a";
        [Required, RegularExpression("^#[0-9a-fA-F]{6}$")] public string Brand { get; set; } = "#167d6d";
        [Required, RegularExpression("^#[0-9a-fA-F]{6}$")] public string Lime { get; set; } = "#e4f26b";
        [Required, RegularExpression("^#[0-9a-fA-F]{6}$")] public string Paper { get; set; } = "#f4f6f1";
        [Required, RegularExpression("^#[0-9a-fA-F]{6}$")] public string Danger { get; set; } = "#bc3c3c";
        public ThemeConfig ToConfig() => new() { Theme=Theme, Ink=Ink, Brand=Brand, Lime=Lime, Paper=Paper, Danger=Danger };
        public static InputModel From(ThemeConfig value) => new() { Theme=value.Theme, Ink=value.Ink, Brand=value.Brand, Lime=value.Lime, Paper=value.Paper, Danger=value.Danger };
    }
}
