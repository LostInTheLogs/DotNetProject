using Microsoft.EntityFrameworkCore;
using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Models;

namespace ClinicManager.Services;

public class MedicalProcedureService(ApplicationDbContext context) : IMedicalProcedureService
{
    public async Task<IEnumerable<MedicalProcedure>> GetAllAsync()
    {
        var query = context.MedicalProcedures.AsNoTracking();

        return await query.OrderBy(m => m.Name).ToListAsync();
    }

    public async Task<MedicalProcedure?> GetByIdAsync(int id)
    {
        return await context.MedicalProcedures.FindAsync(id);
    }

    public async Task CreateAsync(MedicalProcedure dto)
    {
        var procedure = new MedicalProcedure
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            ServiceCost = dto.ServiceCost,
        };

        context.MedicalProcedures.Add(procedure);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MedicalProcedure dto)
    {
        var procedure = await context.MedicalProcedures.FindAsync(dto.Id);
        if (procedure == null) throw new KeyNotFoundException("Procedure item not found.");

        procedure.Name = dto.Name.Trim();
        procedure.Description = dto.Description.Trim();
        procedure.ServiceCost = dto.ServiceCost;

        context.MedicalProcedures.Update(procedure);
        await context.SaveChangesAsync();
    }

}
