using System.ComponentModel.DataAnnotations;

namespace ClinicManager.Models;

public class Patient
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11)]
    public string Pesel { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string InsuranceNumber { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false; // Soft-delete for RODO / data retention regulations
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
