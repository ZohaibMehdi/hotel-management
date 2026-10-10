using System.ComponentModel.DataAnnotations;

namespace TestApplication.Models;

public class CreateGuestViewModel
{
    [Required, StringLength(150)]
    [Display(Name = "Guest name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Father's name")]
    public string? FathersName { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "NIC number")]
    public string NicNumber { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }
}
