using Microsoft.AspNetCore.Identity;

namespace KrossSounds.Data;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string DisplayName { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
}
