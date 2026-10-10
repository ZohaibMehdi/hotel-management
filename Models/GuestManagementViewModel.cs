namespace TestApplication.Models;

public class GuestManagementViewModel
{
    public IReadOnlyList<Guest> Guests { get; set; } = [];

    public IReadOnlyList<Booking> Bookings { get; set; } = [];
}
