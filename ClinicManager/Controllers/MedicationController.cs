using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManager.Services;
using ClinicManager.DTOs;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin,Receptionist")]
public class MedicationController(IMedicationService medicationService) : Controller
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
        return View(new MedicationFormDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MedicationFormDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await medicationService.CreateAsync(dto);
            TempData["Success"] = $"{dto.Name} successfully added to inventory catalog.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var med = await medicationService.GetByIdAsync(id);
        if (med == null) return NotFound();

        var dto = new MedicationFormDto
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
    public async Task<IActionResult> Edit(MedicationFormDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await medicationService.UpdateAsync(dto);
            TempData["Success"] = "Medication entry updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
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
            TempData["Success"] = "Medication availability status changed.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
