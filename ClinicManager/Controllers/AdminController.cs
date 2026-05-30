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

    // GET: /Admin/CreateStaff
    [HttpGet]
    public async Task<IActionResult> CreateStaff()
    {
        var roles = await roleManager.Roles.Select(r => r.Name!).ToListAsync();
        var model = new CreateStaffDto { AvailableRoles = roles };
        return View(model);
    }

    // POST: /Admin/CreateStaff
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStaff(CreateStaffDto model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = await roleManager.Roles.Select(r => r.Name!).ToListAsync();
            return View(model);
        }

        var existingUser = await userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
            model.AvailableRoles = await roleManager.Roles.Select(r => r.Name!).ToListAsync();
            return View(model);
        }

        var password = GenerateRandomPassword();
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, model.SelectedRole);
            logger.LogInformation("Admin created staff account {Email} with role {Role}.", model.Email, model.SelectedRole);

            TempData["StaffCreated"] = "true";
            TempData["StaffFullName"] = $"{user.FirstName} {user.LastName}";
            TempData["StaffEmail"] = user.Email;
            TempData["StaffPassword"] = password;
            TempData["StaffRole"] = model.SelectedRole;

            return RedirectToAction(nameof(Users));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        model.AvailableRoles = await roleManager.Roles.Select(r => r.Name!).ToListAsync();
        return View(model);
    }

    private static string GenerateRandomPassword()
    {
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*()-_=+";
        const string all = upper + lower + digits + special;
        var random = new Random();
        var chars = new char[14];

        chars[0] = upper[random.Next(upper.Length)];
        chars[1] = lower[random.Next(lower.Length)];
        chars[2] = digits[random.Next(digits.Length)];
        chars[3] = special[random.Next(special.Length)];

        for (int i = 4; i < chars.Length; i++)
            chars[i] = all[random.Next(all.Length)];

        return new string(chars.OrderBy(_ => random.Next()).ToArray());
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

    // POST: /Admin/DeleteUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var userName = $"{user.FirstName} {user.LastName}";

        try
        {
            var result = await userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                logger.LogInformation("Admin deleted user {Email}.", user.Email);
                TempData["Success"] = $"User {userName} has been deleted.";
            }
            else
            {
                TempData["Success"] = $"Failed to delete user {userName}: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }
        }
        catch (DbUpdateException)
        {
            TempData["Success"] = $"Cannot delete {userName}. User has associated records (e.g., visits or clinical notes) in the system.";
        }

        return RedirectToAction(nameof(Users));
    }
}
