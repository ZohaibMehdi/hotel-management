using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TestApplication.Models;

public class TenantPortalViewModel
{
    [ValidateNever]
    public Tenant Tenant { get; set; } = null!;

    [ValidateNever]
    public IReadOnlyList<Room> Rooms { get; set; } = [];

    [Range(1, int.MaxValue)]
    [Display(Name = "Room number")]
    public int RoomNumber { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Room type")]
    public string RoomType { get; set; } = string.Empty;
}
