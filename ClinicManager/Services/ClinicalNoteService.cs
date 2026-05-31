using Microsoft.EntityFrameworkCore;
using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;

namespace ClinicManager.Services;

public class ClinicalNoteService(ApplicationDbContext context, ClinicMapper mapper) : IClinicalNoteService
{
    public async Task<IEnumerable<ClinicalNoteResponseDto>> GetByVisitAsync(int visitId)
    {
        return await context.ClinicalNotes
            .Include(n => n.Author)
            .AsNoTracking()
            .Where(n => n.VisitId == visitId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => mapper.NoteToResponseDto(n))
            .ToListAsync();
    }

    public async Task<ClinicalNoteResponseDto> CreateAsync(CreateClinicalNoteDto dto, string authorId)
    {
        var note = new ClinicalNote
        {
            VisitId = dto.VisitId,
            AuthorId = authorId,
            NoteType = dto.NoteType,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        context.ClinicalNotes.Add(note);
        await context.SaveChangesAsync();

        await context.Entry(note).Reference(n => n.Author).LoadAsync();
        return mapper.NoteToResponseDto(note);
    }

    public async Task DeleteAsync(int noteId)
    {
        var note = await context.ClinicalNotes.FindAsync(noteId);
        if (note == null)
            throw new KeyNotFoundException("Clinical note not found.");

        context.ClinicalNotes.Remove(note);
        await context.SaveChangesAsync();
    }
}
