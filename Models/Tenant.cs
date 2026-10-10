using System.ComponentModel.DataAnnotations;

namespace TestApplication.Models;

public class Tenant
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    public string ContactNumber { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public bool IsActive { get; set; } = true;
}
