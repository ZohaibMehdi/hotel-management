using System.ComponentModel.DataAnnotations;

namespace TestApplication.Models;

public class Booking
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public int GuestId { get; set; }

    public Guest Guest { get; set; } = null!;

    public int RoomId { get; set; }

    public Room Room { get; set; } = null!;

    [Required]
    public DateOnly CheckInDate { get; set; }

    public DateOnly? CheckOutDate { get; set; }
}
