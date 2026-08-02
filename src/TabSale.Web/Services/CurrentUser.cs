using System.Security.Claims;
using TabSale.Web.Models;

namespace TabSale.Web.Services;

public sealed class CurrentUser(IHttpContextAccessor accessor)
{
    public long Id => long.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    public string Name => accessor.HttpContext?.User.Identity?.Name ?? "";
    public UserRole Role => Enum.TryParse<UserRole>(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role), out var role) ? role : UserRole.User;
    public Guid SessionToken => Guid.TryParse(accessor.HttpContext?.User.FindFirstValue("session"), out var token) ? token : Guid.Empty;
    public bool IsAdmin => Role == UserRole.Admin;
    public bool IsManager => Role is UserRole.Admin or UserRole.Manager;
}
