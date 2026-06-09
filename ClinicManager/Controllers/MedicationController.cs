using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManager.Services;
using ClinicManager.Models;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin,Receptionist")]
public class MedicationController(IMedicationService medicationService, ILogger<MedicationController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var medications = await medicationService.GetAllAsync();
        return View(medications);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Medication());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Medication dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await medicationService.CreateAsync(dto);
            logger.LogInformation("Medication '{Name}' created.", dto.Name);
            TempData["Success"] = $"{dto.Name} successfully added to inventory catalog.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create medication '{Name}'.", dto.Name);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var med = await medicationService.GetByIdAsync(id);
        if (med == null) return NotFound();

        var dto = new Medication()
        {
            Id = med.Id,
            Name = med.Name,
            Description = med.Description,
            UnitPrice = med.UnitPrice,
            IsAvailable = med.IsAvailable
        };

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Medication dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await medicationService.UpdateAsync(dto);
            logger.LogInformation("Medication {Id} ('{Name}') updated.", dto.Id, dto.Name);
            TempData["Success"] = "Medication entry updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to update medication {Id}.", dto.Id);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        try
        {
            await medicationService.ToggleAvailabilityAsync(id);
            logger.LogInformation("Medication {Id} availability toggled.", id);
            TempData["Success"] = "Medication availability status changed.";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to toggle availability for medication {Id}.", id);
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
