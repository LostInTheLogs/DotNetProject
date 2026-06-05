using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ClinicManager.DTOs;
using ClinicManager.Models;
using ClinicManager.Services;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin,Doctor")]
public class ReportsController(
    IServiceCostReportService reportService,
    IPatientService patientService,
    UserManager<ApplicationUser> userManager,
    IPdfService pdfService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(ServiceCostReportFilterDto? filter)
    {
        var patients = await patientService.GetAllAsync();
        ViewBag.PatientList = patients.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = $"{p.LastName}, {p.FirstName} ({p.Pesel})"
        }).ToList();

        var doctors = (await userManager.GetUsersInRoleAsync("Doctor"))
            .OrderBy(d => d.LastName)
            .ToList();
        ViewBag.DoctorList = doctors.Select(d => new SelectListItem
        {
            Value = d.Id,
            Text = $"{d.LastName}, {d.FirstName}"
        }).ToList();

        if (!HasActiveFilter(filter))
        {
            return View(new ServiceCostReportDto());
        }

        var report = await reportService.GetReportAsync(filter!);
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(ServiceCostReportFilterDto filter)
    {
        if (!HasActiveFilter(filter))
        {
            TempData["Error"] = "Apply at least one filter before exporting.";
            return RedirectToAction(nameof(Index));
        }

        var report = await reportService.GetReportAsync(filter);
        var pdfBytes = pdfService.GenerateServiceCostReportPdf(report);
        return File(pdfBytes, "application/pdf", "ServiceCostReport.pdf");
    }

    private static bool HasActiveFilter(ServiceCostReportFilterDto? filter)
    {
        return filter?.PatientId.HasValue == true
            || !string.IsNullOrEmpty(filter?.DoctorId)
            || filter?.Month.HasValue == true
            || filter?.Year.HasValue == true;
    }
}
