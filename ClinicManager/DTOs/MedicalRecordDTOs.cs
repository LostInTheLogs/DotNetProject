using System.ComponentModel.DataAnnotations;

namespace ClinicManager.DTOs;

public record MedicalRecordResponseDto
{
    public int Id { get; init; }
    public int PatientId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string DocumentScanUrl { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
}

public record UploadMedicalRecordRequestDto(
    [Required] int PatientId,
    [Required] [MaxLength(100)] string DocumentType,
    [MaxLength(500)] string Description
);
