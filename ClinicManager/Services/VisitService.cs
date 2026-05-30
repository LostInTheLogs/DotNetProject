using Microsoft.EntityFrameworkCore;
using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;

namespace ClinicManager.Services;

public class VisitService(ApplicationDbContext context, ClinicMapper mapper) : IVisitService
{
    public async Task<VisitResponseDto> CreateVisitAsync(CreateVisitRequestDto dto)
    {
        var slotStart = dto.ScheduledDate;
        var slotEnd = dto.ScheduledDate.AddMinutes(30);

        var hasConflict = await context.Visits
            .AnyAsync(v => v.DoctorId == dto.DoctorId &&
                           v.Status != VisitStatus.Cancelled &&
                           v.ScheduledDate < slotEnd &&
                           v.ScheduledDate.AddMinutes(30) > slotStart);

        if (hasConflict)
            throw new InvalidOperationException("This doctor is already booked for this specific time slot.");

        var visit = mapper.CreateDtoToVisit(dto);
        visit.Status = VisitStatus.Scheduled;
        visit.CreatedAt = DateTime.UtcNow;

        context.Visits.Add(visit);
        await context.SaveChangesAsync();

        await context.Entry(visit).Reference(v => v.Patient).LoadAsync();
        await context.Entry(visit).Reference(v => v.Doctor).LoadAsync();

        return mapper.VisitToResponseDto(visit);
    }

    public async Task<VisitResponseDto?> GetByIdAsync(int id)
    {
        return await context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .AsNoTracking()
            .Select(v => mapper.VisitToResponseDto(v))
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<VisitResponseDto>> GetByDoctorScheduleAsync(string doctorId, DateTime date)
    {
        var targetDate = date.Date;
        var visits = await context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .AsNoTracking()
            .Where(v => v.DoctorId == doctorId && v.ScheduledDate.Date == targetDate)
            .OrderBy(v => v.ScheduledDate)
            .ToListAsync();

        return visits.Select(mapper.VisitToResponseDto);
    }

    public async Task<IEnumerable<VisitResponseDto>> GetPatientVisitHistoryAsync(int patientId)
    {
        var visits = await context.Visits
            .Include(v => v.Doctor)
            .AsNoTracking()
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.ScheduledDate)
            .ToListAsync();

        return visits.Select(mapper.VisitToResponseDto);
    }

    public async Task<VisitResponseDto> UpdateStatusAsync(int visitId, VisitStatus newStatus)
    {
        var visit = await context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .FirstOrDefaultAsync(v => v.Id == visitId);

        if (visit == null)
            throw new KeyNotFoundException("Visit record not found.");

        if (visit.Status == VisitStatus.Completed || visit.Status == VisitStatus.Cancelled)
            throw new InvalidOperationException("Terminal visit states cannot be modified.");

        visit.Status = newStatus;
        await context.SaveChangesAsync();

        return mapper.VisitToResponseDto(visit);
    }
}
