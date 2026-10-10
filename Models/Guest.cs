using System.ComponentModel.DataAnnotations;

namespace TestApplication.Models;

public class Guest
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    public string? FathersName { get; set; }

    [Required, StringLength(50)]
    public string NicNumber { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }
}
