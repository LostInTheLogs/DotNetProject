using ClinicManager.DTOs;

namespace ClinicManager.Services;

public interface IPatientService
{
    Task<List<PatientResponseDto>> GetAllAsync();
    Task<PatientResponseDto?> GetByIdAsync(int id);
    Task<List<PatientResponseDto>> SearchAsync(string? searchTerm);
    Task<PatientResponseDto> CreateAsync(CreatePatientRequestDto dto);
    Task<PatientResponseDto?> UpdateAsync(int id, UpdatePatientRequestDto dto);
    Task<bool> SoftDeleteAsync(int id);
}
