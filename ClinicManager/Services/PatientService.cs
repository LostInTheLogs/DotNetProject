using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Services;

public class PatientService(
    ApplicationDbContext context,
    ClinicMapper mapper,
    ILogger<PatientService> logger) : IPatientService
{
    public async Task<List<PatientResponseDto>> GetAllAsync()
    {
        var patients = await context.Patients
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return patients.Select(mapper.PatientToResponseDto).ToList();
    }

    public async Task<PatientResponseDto?> GetByIdAsync(int id)
    {
        var patient = await context.Patients
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
        {
            logger.LogWarning("Patient with Id {PatientId} not found.", id);
            return null;
        }

        return mapper.PatientToResponseDto(patient);
    }

    public async Task<List<PatientResponseDto>> SearchAsync(string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        var query = context.Patients.AsQueryable();

        if (searchTerm.Length == 11 && searchTerm.All(char.IsDigit))
        {
            query = query.Where(p => p.Pesel.Contains(searchTerm));
        }
        else
        {
            query = query.Where(p =>
                p.LastName.Contains(searchTerm) ||
                p.FirstName.Contains(searchTerm));
        }

        var patients = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return patients.Select(mapper.PatientToResponseDto).ToList();
    }

    public async Task<PatientResponseDto> CreateAsync(CreatePatientRequestDto dto)
    {
        var patient = mapper.CreateDtoToPatient(dto);

        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        logger.LogInformation("Created patient {FirstName} {LastName} (PESEL: {Pesel}).",
            patient.FirstName, patient.LastName, patient.Pesel);

        return mapper.PatientToResponseDto(patient);
    }

    public async Task<PatientResponseDto?> UpdateAsync(int id, UpdatePatientRequestDto dto)
    {
        var patient = await context.Patients
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
        {
            logger.LogWarning("Update failed: patient {PatientId} not found.", id);
            return null;
        }

        mapper.UpdatePatientFromDto(dto, patient);
        await context.SaveChangesAsync();

        logger.LogInformation("Updated patient {PatientId}.", id);
        return mapper.PatientToResponseDto(patient);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var patient = await context.Patients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
        {
            logger.LogWarning("Soft-delete failed: patient {PatientId} not found.", id);
            return false;
        }

        if (patient.IsDeleted)
        {
            logger.LogWarning("Patient {PatientId} is already deleted.", id);
            return false;
        }

        patient.IsDeleted = true;
        context.Patients.Update(patient);
        await context.SaveChangesAsync();

        logger.LogInformation("Soft-deleted patient {PatientId}.", id);
        return true;
    }
}
