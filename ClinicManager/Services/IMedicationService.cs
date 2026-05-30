using ClinicManager.DTOs;
using ClinicManager.Models;

namespace ClinicManager.Services;

public interface IMedicationService
{
    Task<IEnumerable<Medication>> GetAllAsync(bool includeUnavailable = true);
    Task<Medication?> GetByIdAsync(int id);
    Task CreateAsync(MedicationFormDto dto);
    Task UpdateAsync(MedicationFormDto dto);
    Task ToggleAvailabilityAsync(int id);
}
