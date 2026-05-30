using ClinicManager.DTOs;

namespace ClinicManager.Services;

public interface IMedicalRecordService
{
    Task<MedicalRecordResponseDto> UploadRecordAsync(UploadMedicalRecordRequestDto dto, Microsoft.AspNetCore.Http.IFormFile file);
    Task<MedicalRecordResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<MedicalRecordResponseDto>> GetByPatientIdAsync(int patientId);
    Task<bool> DeleteRecordAsync(int id);
}
