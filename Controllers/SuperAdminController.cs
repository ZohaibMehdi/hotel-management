using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestApplication.Data;
using TestApplication.Models;

namespace TestApplication.Controllers;

[Authorize(Roles = IdentityRoles.SuperAdmin)]
public class SuperAdminController(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var tenants = await dbContext.Tenants
            .AsNoTracking()
            .OrderBy(tenant => tenant.Name)
            .ToListAsync();
        return View(tenants);
    }

    [HttpGet]
    public IActionResult CreateTenant()
    {
        return View(new CreateTenantViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTenant(CreateTenantViewModel model)
    {
        model.Email = model.Email.Trim();
        model.Name = model.Name.Trim();
        model.Address = model.Address.Trim();
        model.ContactNumber = model.ContactNumber.Trim();

        if (await dbContext.Tenants.AnyAsync(tenant => tenant.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "A tenant with this email already exists.");
        }

        if (await userManager.FindByEmailAsync(model.Email) is not null)
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var tenant = new Tenant
        {
            Name = model.Name,
            Address = model.Address,
            ContactNumber = model.ContactNumber,
            Email = model.Email,
            IsActive = model.IsActive
        };

        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();

        var user = new ApplicationUser
        {
            UserName = tenant.Email,
            Email = tenant.Email,
            TenantId = tenant.Id
        };

        var createUserResult = await userManager.CreateAsync(user, model.Password);
        if (!createUserResult.Succeeded)
        {
            await transaction.RollbackAsync();
            foreach (var error in createUserResult.Errors)
            {
                ModelState.AddModelError(nameof(model.Password), error.Description);
            }

            return View(model);
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, IdentityRoles.TenantAdmin);
        if (!addRoleResult.Succeeded)
        {
            await transaction.RollbackAsync();
            foreach (var error in addRoleResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await transaction.CommitAsync();
        TempData["StatusMessage"] = $"Tenant '{tenant.Name}' and its login were created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTenant(int id)
    {
        var tenant = await dbContext.Tenants.FindAsync(id);
        if (tenant is null)
        {
            return NotFound();
        }

        tenant.IsActive = !tenant.IsActive;
        await dbContext.SaveChangesAsync();
        TempData["StatusMessage"] = $"Tenant '{tenant.Name}' is now {(tenant.IsActive ? "active" : "inactive")}.";
        return RedirectToAction(nameof(Index));
    }
}
