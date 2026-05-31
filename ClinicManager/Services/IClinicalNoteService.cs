using ClinicManager.DTOs;

namespace ClinicManager.Services;

public interface IClinicalNoteService
{
    Task<IEnumerable<ClinicalNoteResponseDto>> GetByVisitAsync(int visitId);
    Task<ClinicalNoteResponseDto> CreateAsync(CreateClinicalNoteDto dto, string authorId);
    Task DeleteAsync(int noteId);
}
