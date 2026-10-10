using System.ComponentModel.DataAnnotations;

namespace TestApplication.Models;

public class CreateTenantViewModel
{
    [Required, StringLength(150)]
    [Display(Name = "Tenant name")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(300)]
    [Display(Name = "Tenant address")]
    public string Address { get; set; } = string.Empty;

    [Required, Phone, StringLength(30)]
    [Display(Name = "Contact number")]
    public string ContactNumber { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    [Display(Name = "Tenant email / login")]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 12)]
    [DataType(DataType.Password)]
    [Display(Name = "Initial password (at least 12 characters)")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}
