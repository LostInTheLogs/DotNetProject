using Microsoft.AspNetCore.Mvc;
using ClinicManager.Services;
using ClinicManager.Models;
using ClinicManager.DTOs;
using ClinicManager.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin,Doctor,Receptionist")]
public class VisitController(IVisitService visitService, UserManager<ApplicationUser> userManager, IPatientService patientService, IMedicationService medicationService, IClinicalNoteService clinicalNoteService) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, VisitStatus newStatus, string returnUrl)
    {
        try
        {
            await visitService.UpdateStatusAsync(id, newStatus);
            TempData["Success"] = $"Visit changed to {newStatus}.";
        }
        catch (Exception ex)
        {
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

    // POST: /Visit/Book
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
            TempData["Success"] = "Procedure added to visit.";
        }
        catch (Exception ex)
        {
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
            TempData["Success"] = "Procedure removed from visit.";
        }
        catch (Exception ex)
        {
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
            TempData["Success"] = "Prescription added to visit.";
        }
        catch (Exception ex)
        {
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
            TempData["Success"] = "Prescription removed from visit.";
        }
        catch (Exception ex)
        {
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
            TempData["Success"] = "Clinical note added.";
        }
        catch (Exception ex)
        {
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
            TempData["Success"] = "Clinical note deleted.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Manage), new { id = visitId });
    }

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
            TempData["Success"] = "Appointment booked successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Calendar), new { doctorId = doctorId, date = scheduledDate.ToString("yyyy-MM-dd") });
    }
}
