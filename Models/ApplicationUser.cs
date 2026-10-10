using Microsoft.AspNetCore.Identity;

namespace TestApplication.Models;

public class ApplicationUser : IdentityUser
{
    public int? TenantId { get; set; }

    public Tenant? Tenant { get; set; }
}
