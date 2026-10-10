using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TestApplication.Models;

public class CreateBookingViewModel : IValidatableObject
{
    [Range(1, int.MaxValue)]
    [Display(Name = "Guest")]
    public int GuestId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Room number")]
    public int RoomId { get; set; }

    [Required]
    [Display(Name = "Check-in date")]
    public DateOnly CheckInDate { get; set; }

    [ValidateNever]
    public IReadOnlyList<Guest> Guests { get; set; } = [];

    [ValidateNever]
    public IReadOnlyList<Room> Rooms { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckInDate == default)
        {
            yield return new ValidationResult("A check-in date is required.", [nameof(CheckInDate)]);
        }
    }
}
