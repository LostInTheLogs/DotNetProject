using Microsoft.AspNetCore.Mvc;
using ClinicManager.Services;
using ClinicManager.Models;
using ClinicManager.DTOs;
using ClinicManager.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace ClinicManager.Controllers;

public class VisitController(IVisitService visitService, UserManager<ApplicationUser> userManager, IPatientService patientService) : Controller
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
