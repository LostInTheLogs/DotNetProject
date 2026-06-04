using System.ComponentModel.DataAnnotations;

namespace ClinicManager.Models;

public class ProcedurePerformed
{
    public int Id { get; set; }

    public int VisitId { get; set; }
    public Visit Visit { get; set; }

    public int MedicalProcedureId { get; set; }
    public MedicalProcedure? MedicalProcedure { get; set; }

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    public decimal ActualCost { get; set; }

    public ICollection<PrescribedMedication> PrescribedMedications { get; set; } = new List<PrescribedMedication>();
}
