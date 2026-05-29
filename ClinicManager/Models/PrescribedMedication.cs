using System.ComponentModel.DataAnnotations;

namespace ClinicManager.Models;

public class PrescribedMedication
{
    public int Id { get; set; }

    public int ProcedurePerformedId { get; set; }
    public ProcedurePerformed? ProcedurePerformed { get; set; }

    public int MedicationId { get; set; }
    public Medication? Medication { get; set; }

    [Required]
    [MaxLength(200)]
    public string Dosage { get; set; } = string.Empty; // e.g., "1 tablet 3x a day"

    public int Quantity { get; set; }
    public decimal TotalCost { get; set; }
}
