using System.ComponentModel.DataAnnotations;

namespace ClinicManager.Models;

public class Visit
{
    public int Id { get; set; }

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    [Required]
    public string DoctorId { get; set; } = string.Empty;
    public ApplicationUser? Doctor { get; set; } // Points to your extended Identity user model

    public DateTime ScheduledDate { get; set; }
    public VisitStatus Status { get; set; } = VisitStatus.Scheduled;

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public decimal TotalCost { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ProcedurePerformed> ProceduresPerformed { get; set; } = new List<ProcedurePerformed>();
    public ICollection<ClinicalNote> ClinicalNotes { get; set; } = new List<ClinicalNote>();
    public virtual ICollection<PrescribedMedication> Prescriptions { get; set; } = new List<PrescribedMedication>();
}
