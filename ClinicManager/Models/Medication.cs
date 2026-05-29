using System.ComponentModel.DataAnnotations;

namespace ClinicManager.Models;

public class Medication
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }
    public bool IsAvailable { get; set; } = true;
}
