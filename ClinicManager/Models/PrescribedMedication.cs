using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManager.Models;

public class PrescribedMedication
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int VisitId { get; set; }

    [ForeignKey(nameof(VisitId))]
    public virtual Visit Visit { get; set; } = null!;

    public int? ProcedurePerformedId { get; set; }

    [ForeignKey(nameof(ProcedurePerformedId))]
    public virtual ProcedurePerformed? ProcedurePerformed { get; set; }

    [Required]
    public int MedicationId { get; set; }

    [ForeignKey(nameof(MedicationId))]
    public virtual Medication Medication { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Dosage { get; set; } = string.Empty; // e.g., "1 tablet 3x a day"

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }
}
