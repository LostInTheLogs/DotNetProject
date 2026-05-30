using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin,Doctor,Receptionist")]
public class PatientController(
    IPatientService patientService,
    ILogger<PatientController> logger) : Controller
{
    // GET: /Patient
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var patients = await patientService.SearchAsync(searchTerm);
        ViewData["CurrentSearch"] = searchTerm ?? string.Empty;
        return View(patients);
    }

    // GET: /Patient/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var patient = await patientService.GetByIdAsync(id);
        if (patient == null)
            return NotFound();

        return View(patient);
    }

    // GET: /Patient/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Patient/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePatientRequestDto model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var patient = await patientService.CreateAsync(model);
            TempData["Success"] = $"Patient {patient.FirstName} {patient.LastName} has been created.";
            logger.LogInformation("Created patient via form: {FirstName} {LastName}.", patient.FirstName, patient.LastName);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create patient.");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the patient. Please try again.");
            return View(model);
        }
    }

    // GET: /Patient/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        var patient = await patientService.GetByIdAsync(id);
        if (patient == null)
            return NotFound();

        var model = new UpdatePatientRequestDto(
            patient.FirstName,
            patient.LastName,
            patient.Phone,
            patient.Email,
            patient.Address
        );

        return View(model);
    }

    // POST: /Patient/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdatePatientRequestDto model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var patient = await patientService.UpdateAsync(id, model);
        if (patient == null)
            return NotFound();

        TempData["Success"] = $"Patient {patient.FirstName} {patient.LastName} has been updated.";
        logger.LogInformation("Updated patient {PatientId} via form.", id);
        return RedirectToAction(nameof(Index));
    }

    // POST: /Patient/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await patientService.GetByIdAsync(id);
        if (patient == null)
            return NotFound();

        var fullName = $"{patient.FirstName} {patient.LastName}";
        var deleted = await patientService.SoftDeleteAsync(id);

        if (!deleted)
        {
            TempData["Error"] = $"Could not delete patient {fullName}.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = $"Patient {fullName} has been deleted.";
        logger.LogInformation("Soft-deleted patient {PatientId} via form.", id);
        return RedirectToAction(nameof(Index));
    }
}
