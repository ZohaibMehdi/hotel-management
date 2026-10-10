using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestApplication.Data;
using TestApplication.Models;

namespace TestApplication.Controllers;

public class AccountController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user is not null && await userManager.IsInRoleAsync(user, IdentityRoles.SuperAdmin))
            {
                if (Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl!);
                }

                return RedirectToAction("Index", "SuperAdmin");
            }

            if (user is not null && user.TenantId.HasValue &&
                await userManager.IsInRoleAsync(user, IdentityRoles.TenantAdmin))
            {
                var tenantIsActive = await dbContext.Tenants
                    .AnyAsync(tenant => tenant.Id == user.TenantId && tenant.IsActive);
                if (tenantIsActive)
                {
                    return RedirectToAction("Index", "TenantPortal");
                }
            }

            await signInManager.SignOutAsync();
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password, or the account is inactive.");
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}
