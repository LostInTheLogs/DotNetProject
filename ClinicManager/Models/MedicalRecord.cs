using System.ComponentModel.DataAnnotations;

namespace ClinicManager.Models;

public class MedicalRecord
{
    public int Id { get; set; }

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    [Required]
    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty; // e.g., "Skierowanie", "Badanie krwi"

    [Required]
    [MaxLength(500)]
    public string DocumentScanUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
