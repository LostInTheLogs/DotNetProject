using System.ComponentModel.DataAnnotations;

namespace ClinicManager.Models;

public class MedicalProcedure
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public decimal ServiceCost { get; set; }
}
