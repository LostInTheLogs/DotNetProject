using ClinicManager.DTOs;
using ClinicManager.Models;

namespace ClinicManager.Services;

public interface IMedicalProcedureService
{
    Task<IEnumerable<MedicalProcedure>> GetAllAsync();
    Task<MedicalProcedure?> GetByIdAsync(int id);
    Task CreateAsync(MedicalProcedure dto);
    Task UpdateAsync(MedicalProcedure dto);
}
