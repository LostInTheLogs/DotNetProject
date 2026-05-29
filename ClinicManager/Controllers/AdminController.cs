using ClinicManager.DTOs;
using ClinicManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ILogger<AdminController> logger) : Controller
{
    // GET: /Admin/Users
    public async Task<IActionResult> Users()
    {
        var users = await userManager.Users.ToListAsync();
        var userRolesDict = new Dictionary<string, IList<string>>();

        foreach (var user in users)
            userRolesDict[user.Id] = await userManager.GetRolesAsync(user);

        ViewBag.UserRoles = userRolesDict;
        return View(users);
    }

    // GET: /Admin/ManageRoles/{userId}
    [HttpGet]
    public async Task<IActionResult> ManageRoles(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            logger.LogWarning("ManageRoles: user {UserId} not found.", userId);
            return NotFound();
        }

        var allRoles = await roleManager.Roles.Select(r => r.Name!).ToListAsync();
        var currentRoles = await userManager.GetRolesAsync(user);

        var model = new ManageRolesDto
        {
            UserId = user.Id,
            UserName = user.Email ?? string.Empty,
            FullName = $"{user.FirstName} {user.LastName}",
            AllRoles = allRoles,
            CurrentRoles = currentRoles,
            SelectedRoles = currentRoles.ToList()
        };

        return View(model);
    }

    // POST: /Admin/ManageRoles
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageRoles(ManageRolesDto model)
    {
        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();

        var currentRoles = await userManager.GetRolesAsync(user);
        var selectedRoles = model.SelectedRoles ?? new List<string>();

        var toAdd = selectedRoles.Except(currentRoles).ToList();
        var toRemove = currentRoles.Except(selectedRoles).ToList();

        if (toAdd.Any())
            await userManager.AddToRolesAsync(user, toAdd);

        if (toRemove.Any())
            await userManager.RemoveFromRolesAsync(user, toRemove);

        logger.LogInformation("Admin updated roles for user {Email}.", user.Email);
        TempData["Success"] = $"Roles for user {user.FirstName} {user.LastName} have been updated.";
        return RedirectToAction(nameof(Users));
    }
}
