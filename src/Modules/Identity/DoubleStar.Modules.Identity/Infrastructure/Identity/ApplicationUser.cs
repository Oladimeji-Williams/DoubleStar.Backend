// Infrastructure/Identity/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace DoubleStar.Modules.Identity.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}