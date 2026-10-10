using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestApplication.Data;
using TestApplication.Models;
using System.Security.Claims;

namespace TestApplication.Controllers;

[Authorize(Roles = IdentityRoles.TenantAdmin)]
public class TenantPortalController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var tenant = await GetCurrentTenantAsync();
        if (tenant is null)
        {
            return Forbid();
        }

        var model = new TenantPortalViewModel
        {
            Tenant = tenant,
            Rooms = await dbContext.Rooms
                .AsNoTracking()
                .Where(room => room.TenantId == tenant.Id)
                .OrderBy(room => room.RoomNumber)
                .ToListAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRoom(TenantPortalViewModel model)
    {
        var tenant = await GetCurrentTenantAsync();
        if (tenant is null)
        {
            return Forbid();
        }

        model.RoomType = model.RoomType?.Trim() ?? string.Empty;
        if (!ModelState.IsValid)
        {
            return await RenderPortalAsync(tenant, model);
        }

        if (await dbContext.Rooms.AnyAsync(room =>
                room.TenantId == tenant.Id && room.RoomNumber == model.RoomNumber))
        {
            ModelState.AddModelError(nameof(model.RoomNumber), "This room number already exists for your hotel.");
            return await RenderPortalAsync(tenant, model);
        }

        dbContext.Rooms.Add(new Room
        {
            TenantId = tenant.Id,
            RoomNumber = model.RoomNumber,
            RoomType = model.RoomType
        });

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsDuplicateRoomNumber(exception))
        {
            ModelState.AddModelError(nameof(model.RoomNumber), "This room number already exists for your hotel.");
            return await RenderPortalAsync(tenant, model);
        }

        TempData["StatusMessage"] = $"Room {model.RoomNumber} was added.";
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

    private async Task<IActionResult> RenderPortalAsync(Tenant tenant, TenantPortalViewModel model)
    {
        model.Tenant = tenant;
        model.Rooms = await dbContext.Rooms
            .AsNoTracking()
            .Where(room => room.TenantId == tenant.Id)
            .OrderBy(room => room.RoomNumber)
            .ToListAsync();
        return View(nameof(Index), model);
    }

    private static bool IsDuplicateRoomNumber(DbUpdateException exception)
    {
        return exception.InnerException is Microsoft.Data.SqlClient.SqlException
        {
            Number: 2601 or 2627
        };
    }
}
