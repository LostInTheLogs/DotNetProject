using Microsoft.AspNetCore.Mvc;
using ClinicManager.Services;
using ClinicManager.Models;
using ClinicManager.DTOs;
using ClinicManager.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin,Doctor,Receptionist")]
public class VisitController(IVisitService visitService, UserManager<ApplicationUser> userManager, IPatientService patientService, IMedicationService medicationService, IClinicalNoteService clinicalNoteService, IPdfService pdfService, ILogger<VisitController> logger) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, VisitStatus newStatus, string returnUrl)
    {
        try
        {
            await visitService.UpdateStatusAsync(id, newStatus);
            logger.LogInformation("Visit {VisitId} status changed to {Status}.", id, newStatus);
            TempData["Success"] = $"Visit changed to {newStatus}.";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to update status for visit {VisitId}.", id);
            TempData["Error"] = ex.Message;
        }

        if (!string.IsNullOrEmpty(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Calendar");
    }

    // GET: /Visit/Calendar
    [HttpGet]
    public async Task<IActionResult> Calendar(string? doctorId, DateTime? date)
    {
        var selectedDate = date ?? DateTime.Today;

        var activeDoctors = await userManager.GetUsersInRoleAsync("Doctor");
        var patients = await patientService.GetAllAsync();

        var selectedDoctorId = doctorId ?? activeDoctors.FirstOrDefault()?.Id ?? string.Empty;

        var bookedVisits = string.IsNullOrEmpty(selectedDoctorId)
            ? new List<VisitResponseDto>()
            : await visitService.GetByDoctorScheduleAsync(selectedDoctorId, selectedDate);

        var availableSlots = new List<DateTime>();
        var baseTime = selectedDate.Date.AddHours(8);
        for (int i = 0; i < 16; i++)
        {
            var currentSlot = baseTime.AddMinutes(i * 30);
            if (!bookedVisits.Any(v => v.ScheduledDate == currentSlot && v.Status != VisitStatus.Cancelled))
            {
                availableSlots.Add(currentSlot);
            }
        }

        // Update your ViewModel to expect ApplicationUser collections
        var viewModel = new DoctorScheduleViewModel
        {
            SelectedDoctorId = selectedDoctorId,
            SelectedDate = selectedDate,
            Doctors = activeDoctors.OrderBy(d => d.LastName),
            Patients = patients,
            BookedVisits = bookedVisits,
            AvailableSlots = availableSlots
        };

        return View(viewModel);
    }

    // GET: /Visit/Manage/{id}
    [HttpGet]
    public async Task<IActionResult> Manage(int id)
    {
        var visit = await visitService.GetVisitDetailsAsync(id);
        if (visit == null) return NotFound();

        var procedures = await visitService.GetAllProceduresAsync();
        var medications = await medicationService.GetAllAsync();
        var notes = await clinicalNoteService.GetByVisitAsync(id);

        var viewModel = new VisitManageViewModel
        {
            Visit = visit,
            AvailableProcedures = procedures.ToList(),
            AvailableMedications = medications.Select(m => new MedicationDto
            {
                Id = m.Id,
                Name = m.Name,
                UnitPrice = m.UnitPrice
            }).ToList(),
            ClinicalNotes = notes.ToList()
        };

        return View(viewModel);
    }

    // POST: /Visit/AddProcedure
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProcedure(int visitId, LogProcedurePerformedDto dto)
    {
        try
        {
            await visitService.AddProcedureAsync(visitId, dto);
            logger.LogInformation("Procedure {ProcedureId} added to visit {VisitId}.", dto.MedicalProcedureId, visitId);
            TempData["Success"] = "Procedure added to visit.";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to add procedure to visit {VisitId}.", visitId);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Manage), new { id = visitId });
    }

    // POST: /Visit/RemoveProcedure
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveProcedure(int procedureId, int visitId)
    {
        try
        {
            await visitService.RemoveProcedureAsync(procedureId);
            logger.LogInformation("Procedure {ProcedureId} removed from visit {VisitId}.", procedureId, visitId);
            TempData["Success"] = "Procedure removed from visit.";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to remove procedure {ProcedureId} from visit {VisitId}.", procedureId, visitId);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Manage), new { id = visitId });
    }

    // POST: /Visit/AddPrescription
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPrescription(int visitId, AddPrescribedMedicationDto dto)
    {
        try
        {
            await visitService.AddPrescriptionAsync(visitId, dto);
            logger.LogInformation("Prescription (medication {MedicationId}) added to visit {VisitId}.", dto.MedicationId, visitId);
            TempData["Success"] = "Prescription added to visit.";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to add prescription to visit {VisitId}.", visitId);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Manage), new { id = visitId });
    }

    // POST: /Visit/RemovePrescription
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePrescription(int prescriptionId, int visitId)
    {
        try
        {
            await visitService.RemovePrescriptionAsync(prescriptionId);
            logger.LogInformation("Prescription {PrescriptionId} removed from visit {VisitId}.", prescriptionId, visitId);
            TempData["Success"] = "Prescription removed from visit.";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to remove prescription {PrescriptionId} from visit {VisitId}.", prescriptionId, visitId);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Manage), new { id = visitId });
    }

    // POST: /Visit/AddNote
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNote(CreateClinicalNoteDto dto)
    {
        try
        {
            var userId = userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            await clinicalNoteService.CreateAsync(dto, userId);
            logger.LogInformation("Clinical note ({NoteType}) added to visit {VisitId}.", dto.NoteType, dto.VisitId);
            TempData["Success"] = "Clinical note added.";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to add clinical note to visit {VisitId}.", dto.VisitId);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Manage), new { id = dto.VisitId });
    }

    // POST: /Visit/DeleteNote
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNote(int noteId, int visitId)
    {
        try
        {
            await clinicalNoteService.DeleteAsync(noteId);
            logger.LogInformation("Clinical note {NoteId} deleted from visit {VisitId}.", noteId, visitId);
            TempData["Success"] = "Clinical note deleted.";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete clinical note {NoteId} from visit {VisitId}.", noteId, visitId);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Manage), new { id = visitId });
    }

    // POST: /Visit/Book
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int patientId, string doctorId, DateTime scheduledDate, string reason)
    {
        try
        {
            var dto = new CreateVisitRequestDto(
                 patientId,
                 doctorId,
                 scheduledDate,
                 reason
            );

            await visitService.CreateVisitAsync(dto);
            logger.LogInformation("Visit booked for patient {PatientId} with doctor {DoctorId} on {Date}.", patientId, doctorId, scheduledDate);
            TempData["Success"] = "Appointment booked successfully.";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to book visit for patient {PatientId}.", patientId);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Calendar), new { doctorId = doctorId, date = scheduledDate.ToString("yyyy-MM-dd") });
    }

    // GET: /Visit/DownloadSummary/{id}
    [HttpGet]
    public async Task<IActionResult> DownloadSummary(int id)
    {
        var visit = await visitService.GetVisitDetailsAsync(id);
        if (visit == null) return NotFound();

        if (visit.Status != VisitStatus.Completed)
        {
            logger.LogWarning("Attempted to download summary for non-completed visit {VisitId}.", id);
            TempData["Error"] = "Visit summary PDF can only be generated for completed visits.";
            return RedirectToAction(nameof(Manage), new { id = id });
        }

        var patient = await patientService.GetByIdAsync(visit.PatientId);
        if (patient == null) return NotFound("Patient data not found.");

        var notes = await clinicalNoteService.GetByVisitAsync(id);

        var pdfBytes = pdfService.GenerateVisitSummaryPdf(visit, patient, notes);
        var fileName = $"VisitSummary_{visit.Id}_{patient.LastName}.pdf";

        logger.LogInformation("Visit summary PDF downloaded for visit {VisitId}.", id);
        return File(pdfBytes, "application/pdf", fileName);
    }

    // GET: /Visit/DownloadPrescription/{id}
    [HttpGet]
    public async Task<IActionResult> DownloadPrescription(int id)
    {
        var visit = await visitService.GetVisitDetailsAsync(id);
        if (visit == null) return NotFound();

        if (visit.Status != VisitStatus.Completed)
        {
            logger.LogWarning("Attempted to download prescription for non-completed visit {VisitId}.", id);
            TempData["Error"] = "Prescriptions can only be printed for completed visits.";
            return RedirectToAction(nameof(Manage), new { id = id });
        }

        if (!visit.Prescriptions.Any())
        {
            logger.LogWarning("Attempted to download prescription for visit {VisitId} with no prescriptions.", id);
            TempData["Error"] = "No prescriptions were recorded for this visit.";
            return RedirectToAction(nameof(Manage), new { id = id });
        }

        var patient = await patientService.GetByIdAsync(visit.PatientId);
        if (patient == null) return NotFound("Patient data not found.");

        var pdfBytes = pdfService.GeneratePrescriptionPdf(visit, patient);
        var fileName = $"Prescription_{visit.Id}_{patient.LastName}.pdf";

        logger.LogInformation("Prescription PDF downloaded for visit {VisitId}.", id);
        return File(pdfBytes, "application/pdf", fileName);
    }
}
