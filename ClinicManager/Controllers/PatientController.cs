using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin,Doctor,Receptionist")]
public class PatientController(
    IPatientService patientService,
    IMedicalRecordService recordService,
    ApplicationDbContext context, 
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

        ViewBag.MedicalRecords = await recordService.GetByPatientIdAsync(id);
        return View(patient);
    }
    
    // PATCH/POST: /Patient/UpdateRecordDescription (AJAX Endpoint for Editing Inline)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRecordDescription(int id, string description)
    {
        var record = await context.MedicalRecords.FindAsync(id);
        if (record == null) return NotFound();

        record.Description = description ?? string.Empty;
        await context.SaveChangesAsync();

        return Json(new { success = true, message = "Description updated successfully." });
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
    // GET: /Patient/MedicalRecords/{patientId}
    [HttpGet]
    public async Task<IActionResult> MedicalRecords(int patientId)
    {
        ViewBag.PatientId = patientId;
        var records = await recordService.GetByPatientIdAsync(patientId);
        return View(records);
    }

    // POST: /Patient/UploadRecord
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadRecord(UploadMedicalRecordRequestDto dto, IFormFile file)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "The provided record details are invalid.";
            return RedirectToAction(nameof(MedicalRecords), new { patientId = dto.PatientId });
        }

        try
        {
            await recordService.UploadRecordAsync(dto, file);
            TempData["Success"] = "Medical document uploaded successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Upload failed: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = dto.PatientId });
    }

    // POST: /Patient/DeleteRecord
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRecord(int id, int patientId)
    {
        var success = await recordService.DeleteRecordAsync(id);
        if (success)
        {
            TempData["Success"] = "Document was removed from records.";
        }
        else
        {
            TempData["Error"] = "Failed to remove the requested file asset.";
        }

        return RedirectToAction(nameof(Details), new {id = patientId });
    }
}
