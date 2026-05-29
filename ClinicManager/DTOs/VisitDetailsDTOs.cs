using System.ComponentModel.DataAnnotations;

namespace ClinicManager.DTOs;

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
    [Required] [MaxLength(500)] string Notes // Matches model property destination
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
    [Required] [MaxLength(200)] string Dosage,
    [Required] [Range(1, 100)] int Quantity
);
