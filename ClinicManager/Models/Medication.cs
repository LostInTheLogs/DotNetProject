using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManager.Models;

public class Medication
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Medication name is required.")]
    [MaxLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
    [Display(Name = "Medication Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description or usage guidelines are required.")]
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Unit price is required.")]
    [Range(0.01, 10000.00, ErrorMessage = "Price must be a positive value between 0.01 and 10,000.00.")]
    [Column(TypeName = "decimal(18, 2)")]
    [DataType(DataType.Currency)]
    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }

    [Display(Name = "Available for Prescribing")]
    public bool IsAvailable { get; set; } = true;
}
