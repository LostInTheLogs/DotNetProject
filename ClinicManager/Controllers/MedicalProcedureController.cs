using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManager.Services;
using ClinicManager.Models;

namespace ClinicManager.Controllers;

[Authorize(Roles = "Admin,Receptionist")]
public class MedicalProcedureController(IMedicalProcedureService procedureService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var procedures = await procedureService.GetAllAsync();
        return View(procedures);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new MedicalProcedure());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MedicalProcedure dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await procedureService.CreateAsync(dto);
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
        var med = await procedureService.GetByIdAsync(id);
        if (med == null) return NotFound();

        var dto = new MedicalProcedure()
        {
            Id = med.Id,
            Name = med.Name,
            Description = med.Description,
            ServiceCost = med.ServiceCost,
        };

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MedicalProcedure dto)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await procedureService.UpdateAsync(dto);
            TempData["Success"] = "Procedure entry updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }
}
