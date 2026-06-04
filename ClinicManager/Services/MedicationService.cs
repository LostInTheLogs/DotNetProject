using Microsoft.EntityFrameworkCore;
using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Models;

namespace ClinicManager.Services;

public class MedicationService(ApplicationDbContext context) : IMedicationService
{
    public async Task<IEnumerable<Medication>> GetAllAsync(bool includeUnavailable = true)
    {
        var query = context.Medications.AsNoTracking();

        if (!includeUnavailable)
            query = query.Where(m => m.IsAvailable);

        return await query.OrderBy(m => m.Name).ToListAsync();
    }

    public async Task<Medication?> GetByIdAsync(int id)
    {
        return await context.Medications.FindAsync(id);
    }

    public async Task CreateAsync(Medication dto)
    {
        var medication = new Medication
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            UnitPrice = dto.UnitPrice,
            IsAvailable = dto.IsAvailable
        };

        context.Medications.Add(medication);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Medication dto)
    {
        var medication = await context.Medications.FindAsync(dto.Id);
        if (medication == null) throw new KeyNotFoundException("Medication item not found.");

        medication.Name = dto.Name.Trim();
        medication.Description = dto.Description.Trim();
        medication.UnitPrice = dto.UnitPrice;
        medication.IsAvailable = dto.IsAvailable;

        context.Medications.Update(medication);
        await context.SaveChangesAsync();
    }

    public async Task ToggleAvailabilityAsync(int id)
    {
        var medication = await context.Medications.FindAsync(id);
        if (medication == null) throw new KeyNotFoundException("Medication item not found.");

        medication.IsAvailable = !medication.IsAvailable;
        await context.SaveChangesAsync();
    }
}
