using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicManager.Models;

public class MedicalProcedure
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "The medical procedure name is required.")]
    [MaxLength(150, ErrorMessage = "The procedure name cannot exceed 150 characters.")]
    [Display(Name = "Procedure Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "A detailed clinical description is required.")]
    [MaxLength(1000, ErrorMessage = "The description cannot exceed 1000 characters.")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Procedure Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "The base service cost is required.")]
    [Range(0.00, 50000.00, ErrorMessage = "The service cost must be a positive value between 0.00 and 50,000.00.")]
    [Column(TypeName = "decimal(18, 2)")]
    [DataType(DataType.Currency)]
    [Display(Name = "Service Cost")]
    public decimal ServiceCost { get; set; }
}
