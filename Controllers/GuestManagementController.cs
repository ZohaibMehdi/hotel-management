using System.Security.Claims;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestApplication.Data;
using TestApplication.Models;

namespace TestApplication.Controllers;

[Authorize(Roles = IdentityRoles.TenantAdmin)]
public class GuestManagementController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var tenant = await GetCurrentTenantAsync();
        if (tenant is null)
        {
            return Forbid();
        }

        var bookings = await dbContext.Bookings
            .AsNoTracking()
            .Where(booking => booking.TenantId == tenant.Id)
            .Include(booking => booking.Guest)
            .Include(booking => booking.Room)
            .OrderByDescending(booking => booking.CheckOutDate == null)
            .ThenByDescending(booking => booking.CheckInDate)
            .ToListAsync();

        return View(new GuestManagementViewModel
        {
            Guests = await dbContext.Guests
                .AsNoTracking()
                .Where(guest => guest.TenantId == tenant.Id)
                .OrderBy(guest => guest.Name)
                .ToListAsync(),
            Bookings = bookings
        });
    }

    [HttpGet]
    public IActionResult CreateGuest()
    {
        return View(new CreateGuestViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGuest(CreateGuestViewModel model)
    {
        var tenant = await GetCurrentTenantAsync();
        if (tenant is null)
        {
            return Forbid();
        }

        model.Name = model.Name?.Trim() ?? string.Empty;
        model.FathersName = TrimOrNull(model.FathersName);
        model.NicNumber = model.NicNumber?.Trim() ?? string.Empty;
        model.PhoneNumber = TrimOrNull(model.PhoneNumber);
        model.Address = TrimOrNull(model.Address);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        dbContext.Guests.Add(new Guest
        {
            TenantId = tenant.Id,
            Name = model.Name,
            FathersName = model.FathersName,
            NicNumber = model.NicNumber,
            PhoneNumber = model.PhoneNumber,
            Address = model.Address
        });

        await dbContext.SaveChangesAsync();
        TempData["StatusMessage"] = $"Guest '{model.Name}' was added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CreateBooking()
    {
        var tenant = await GetCurrentTenantAsync();
        if (tenant is null)
        {
            return Forbid();
        }

        var model = await PopulateBookingOptionsAsync(tenant.Id, new CreateBookingViewModel());
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBooking(CreateBookingViewModel model)
    {
        var tenant = await GetCurrentTenantAsync();
        if (tenant is null)
        {
            return Forbid();
        }

        var guestExists = await dbContext.Guests
            .AnyAsync(guest => guest.Id == model.GuestId && guest.TenantId == tenant.Id);
        var roomExists = await dbContext.Rooms
            .AnyAsync(room => room.Id == model.RoomId && room.TenantId == tenant.Id);

        if (!guestExists)
        {
            ModelState.AddModelError(nameof(model.GuestId), "Select a guest from your guest list.");
        }

        if (!roomExists)
        {
            ModelState.AddModelError(nameof(model.RoomId), "Select a room from your hotel's rooms.");
        }

        if (!ModelState.IsValid)
        {
            return View(await PopulateBookingOptionsAsync(tenant.Id, model));
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var roomIsOccupied = await dbContext.Bookings.AnyAsync(booking =>
            booking.TenantId == tenant.Id &&
            booking.RoomId == model.RoomId &&
            (booking.CheckOutDate == null || booking.CheckOutDate > model.CheckInDate));

        if (roomIsOccupied)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(nameof(model.RoomId),
                "This room is occupied by a stay that has not checked out by the selected check-in date.");
            return View(await PopulateBookingOptionsAsync(tenant.Id, model));
        }

        dbContext.Bookings.Add(new Booking
        {
            TenantId = tenant.Id,
            GuestId = model.GuestId,
            RoomId = model.RoomId,
            CheckInDate = model.CheckInDate
        });

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        TempData["StatusMessage"] = "The guest was assigned to the room.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CheckOut(int id)
    {
        var tenant = await GetCurrentTenantAsync();
        if (tenant is null)
        {
            return Forbid();
        }

        var booking = await GetBookingAsync(tenant.Id, id);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.CheckOutDate is not null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new CheckOutViewModel
        {
            BookingId = booking.Id,
            CheckOutDate = DateOnly.FromDateTime(DateTime.Today),
            GuestName = booking.Guest.Name,
            RoomNumber = booking.Room.RoomNumber,
            CheckInDate = booking.CheckInDate
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(CheckOutViewModel model)
    {
        var tenant = await GetCurrentTenantAsync();
        if (tenant is null)
        {
            return Forbid();
        }

        var booking = await GetBookingAsync(tenant.Id, model.BookingId);
        if (booking is null)
        {
            return NotFound();
        }

        if (booking.CheckOutDate is not null)
        {
            ModelState.AddModelError(string.Empty, "This stay has already been checked out.");
        }
        else if (model.CheckOutDate <= booking.CheckInDate)
        {
            ModelState.AddModelError(nameof(model.CheckOutDate), "Check-out date must be after check-in date.");
        }

        if (!ModelState.IsValid)
        {
            model.GuestName = booking.Guest.Name;
            model.RoomNumber = booking.Room.RoomNumber;
            model.CheckInDate = booking.CheckInDate;
            return View(model);
        }

        booking.CheckOutDate = model.CheckOutDate;
        await dbContext.SaveChangesAsync();
        TempData["StatusMessage"] = $"Stay for {booking.Guest.Name} was checked out.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Tenant?> GetCurrentTenantAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(applicationUser => applicationUser.Tenant)
            .SingleOrDefaultAsync(applicationUser => applicationUser.Id == userId);
        return user?.Tenant is { IsActive: true } tenant ? tenant : null;
    }

    private async Task<CreateBookingViewModel> PopulateBookingOptionsAsync(
        int tenantId,
        CreateBookingViewModel model)
    {
        model.Guests = await dbContext.Guests
            .AsNoTracking()
            .Where(guest => guest.TenantId == tenantId)
            .OrderBy(guest => guest.Name)
            .ToListAsync();
        model.Rooms = await dbContext.Rooms
            .AsNoTracking()
            .Where(room => room.TenantId == tenantId)
            .OrderBy(room => room.RoomNumber)
            .ToListAsync();
        return model;
    }

    private Task<Booking?> GetBookingAsync(int tenantId, int bookingId)
    {
        return dbContext.Bookings
            .Include(booking => booking.Guest)
            .Include(booking => booking.Room)
            .SingleOrDefaultAsync(booking => booking.Id == bookingId && booking.TenantId == tenantId);
    }

    private static string? TrimOrNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
