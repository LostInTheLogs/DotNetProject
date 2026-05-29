namespace ClinicManager.DTOs;

public record MedicationDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; } // Matches Model
}

public record MedicalProcedureDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal ServiceCost { get; init; } // Matches Model
}
