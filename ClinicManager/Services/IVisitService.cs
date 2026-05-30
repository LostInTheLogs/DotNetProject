using ClinicManager.DTOs;
using ClinicManager.Models;

namespace ClinicManager.Services;

public interface IVisitService
{
    Task<VisitResponseDto> CreateVisitAsync(CreateVisitRequestDto dto);
    Task<VisitResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<VisitResponseDto>> GetByDoctorScheduleAsync(string doctorId, DateTime date);
    Task<IEnumerable<VisitResponseDto>> GetPatientVisitHistoryAsync(int patientId);
    Task<VisitResponseDto> UpdateStatusAsync(int visitId, VisitStatus newStatus);
}
