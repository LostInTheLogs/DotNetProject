using System.ComponentModel.DataAnnotations;

namespace ClinicManager.Models;

public class ClinicalNote
{
    public int Id { get; set; }

    public int VisitId { get; set; }
    public Visit? Visit { get; set; }

    [Required]
    public string AuthorId { get; set; } = string.Empty;
    public ApplicationUser? Author { get; set; }

    [Required]
    [MaxLength(100)]
    public string NoteType { get; set; } = string.Empty; // e.g., "Wywiad", "Rozpoznanie", "Zalecenia"

    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
