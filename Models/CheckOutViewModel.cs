using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TestApplication.Models;

public class CheckOutViewModel
{
    [Range(1, int.MaxValue)]
    public int BookingId { get; set; }

    [Required]
    [Display(Name = "Actual check-out date")]
    public DateOnly CheckOutDate { get; set; }

    [ValidateNever]
    public string GuestName { get; set; } = string.Empty;

    [ValidateNever]
    public int RoomNumber { get; set; }

    [ValidateNever]
    public DateOnly CheckInDate { get; set; }
}
