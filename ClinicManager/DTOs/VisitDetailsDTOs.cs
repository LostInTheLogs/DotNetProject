using System.ComponentModel.DataAnnotations;
using ClinicManager.Models;

namespace ClinicManager.DTOs;

public record VisitDetailsDto
{
    public int Id { get; init; }
    public int PatientId { get; init; }
    public string PatientFullName { get; init; } = string.Empty;
    public string DoctorId { get; init; } = string.Empty;
    public string DoctorFullName { get; init; } = string.Empty;
    public DateTime ScheduledDate { get; init; }
    public VisitStatus Status { get; init; }
    public string Reason { get; init; } = string.Empty;
    public decimal TotalCost { get; init; }
    public List<ProcedurePerformedResponseDto> Procedures { get; init; } = new();
    public List<PrescribedMedicationResponseDto> Prescriptions { get; init; } = new();
}

public record ProcedurePerformedResponseDto
{
    public int Id { get; init; }
    public int MedicalProcedureId { get; init; }
    public string ProcedureName { get; init; } = string.Empty;
    public decimal ActualCost { get; init; }
    public string Notes { get; init; } = string.Empty;
}

public record LogProcedurePerformedDto(
    [Required] int MedicalProcedureId,
    [MaxLength(500)] string Notes = "" // Matches model property destination
);

public record PrescribedMedicationResponseDto
{
    public int Id { get; init; }
    public int MedicationId { get; init; }
    public string MedicationName { get; init; } = string.Empty;
    public string Dosage { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal TotalCost { get; init; }
}

public record AddPrescribedMedicationDto(
    [Required] int MedicationId,
    [Required][MaxLength(200)] string Dosage,
    [Required][Range(1, 100)] int Quantity
);

public record ClinicalNoteResponseDto
{
    public int Id { get; init; }
    public int VisitId { get; init; }
    public string AuthorId { get; init; } = string.Empty;
    public string AuthorName { get; init; } = string.Empty;
    public string NoteType { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record CreateClinicalNoteDto(
    [Required] int VisitId,
    [Required][MaxLength(100)] string NoteType,
    [Required][MaxLength(4000)] string Content
);
