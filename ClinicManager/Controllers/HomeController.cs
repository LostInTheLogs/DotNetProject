using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ClinicManager.Models;
using ClinicManager.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ClinicManager.Controllers;

[Authorize]
public class HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        var userId = userManager.GetUserId(User);

        ViewBag.TotalPatients = await context.Patients.CountAsync();
        ViewBag.TodayVisits = await context.Visits.CountAsync(v => v.ScheduledDate.Date == today);
        ViewBag.CompletedVisits = await context.Visits.CountAsync(v => v.Status == VisitStatus.Completed);
        ViewBag.ScheduledVisits = await context.Visits.CountAsync(v => v.Status == VisitStatus.Scheduled);

        if (User.IsInRole("Doctor"))
        {
            ViewBag.MyTodayVisits = await context.Visits
                .CountAsync(v => v.DoctorId == userId && v.ScheduledDate.Date == today);
        }

        if (User.IsInRole("Admin"))
        {
            ViewBag.TotalStaff = await userManager.Users.CountAsync();
        }

        var user = await userManager.GetUserAsync(User);
        ViewBag.UserFullName = user != null ? $"{user.FirstName} {user.LastName}" : User.Identity?.Name;
        ViewBag.Role = (await userManager.GetRolesAsync(user!)).FirstOrDefault() ?? "User";

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
