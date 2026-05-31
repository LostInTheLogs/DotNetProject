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

    public async Task<VisitDetailsDto?> GetVisitDetailsAsync(int visitId)
    {
        var visit = await context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .Include(v => v.ProceduresPerformed)
                .ThenInclude(p => p.MedicalProcedure)
            .Include(v => v.Prescriptions)
                .ThenInclude(p => p.Medication)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == visitId);

        if (visit == null) return null;

        return new VisitDetailsDto
        {
            Id = visit.Id,
            PatientId = visit.PatientId,
            PatientFullName = $"{visit.Patient!.FirstName} {visit.Patient.LastName}",
            DoctorId = visit.DoctorId,
            DoctorFullName = $"{visit.Doctor!.FirstName} {visit.Doctor.LastName}",
            ScheduledDate = visit.ScheduledDate,
            Status = visit.Status,
            Reason = visit.Reason,
            TotalCost = visit.TotalCost,
            Procedures = visit.ProceduresPerformed.Select(mapper.ProcedureToResponseDto).ToList(),
            Prescriptions = visit.Prescriptions.Select(mapper.MedicationToResponseDto).ToList()
        };
    }

    public async Task<ProcedurePerformedResponseDto> AddProcedureAsync(int visitId, LogProcedurePerformedDto dto)
    {
        var visit = await context.Visits.FindAsync(visitId);
        if (visit == null)
            throw new KeyNotFoundException("Visit not found.");

        var procedure = await context.MedicalProcedures.FindAsync(dto.MedicalProcedureId);
        if (procedure == null)
            throw new KeyNotFoundException("Medical procedure not found.");

        var performed = mapper.LogDtoToProcedure(dto);
        performed.VisitId = visitId;
        performed.ActualCost = procedure.ServiceCost;

        context.ProceduresPerformed.Add(performed);
        await context.SaveChangesAsync();

        await RecalculateTotalCostAsync(visitId);

        await context.Entry(performed).Reference(p => p.MedicalProcedure).LoadAsync();
        return mapper.ProcedureToResponseDto(performed);
    }

    public async Task RemoveProcedureAsync(int procedureId)
    {
        var performed = await context.ProceduresPerformed
            .Include(p => p.PrescribedMedications)
            .FirstOrDefaultAsync(p => p.Id == procedureId);

        if (performed == null)
            throw new KeyNotFoundException("Procedure record not found.");

        var visitId = performed.VisitId;
        context.ProceduresPerformed.Remove(performed);
        await context.SaveChangesAsync();

        await RecalculateTotalCostAsync(visitId);
    }

    public async Task<PrescribedMedicationResponseDto> AddPrescriptionAsync(int visitId, AddPrescribedMedicationDto dto)
    {
        var visit = await context.Visits.FindAsync(visitId);
        if (visit == null)
            throw new KeyNotFoundException("Visit not found.");

        var medication = await context.Medications.FindAsync(dto.MedicationId);
        if (medication == null)
            throw new KeyNotFoundException("Medication not found.");

        var prescribed = mapper.AddDtoToMedication(dto);
        prescribed.VisitId = visitId;
        prescribed.TotalCost = medication.UnitPrice * dto.Quantity;

        context.PrescribedMedications.Add(prescribed);
        await context.SaveChangesAsync();

        await RecalculateTotalCostAsync(visitId);

        await context.Entry(prescribed).Reference(p => p.Medication).LoadAsync();
        return mapper.MedicationToResponseDto(prescribed);
    }

    public async Task RemovePrescriptionAsync(int prescriptionId)
    {
        var prescribed = await context.PrescribedMedications.FindAsync(prescriptionId);
        if (prescribed == null)
            throw new KeyNotFoundException("Prescription not found.");

        var visitId = prescribed.VisitId;
        context.PrescribedMedications.Remove(prescribed);
        await context.SaveChangesAsync();

        await RecalculateTotalCostAsync(visitId);
    }

    public async Task<IEnumerable<MedicalProcedureDto>> GetAllProceduresAsync()
    {
        return await context.MedicalProcedures
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => mapper.ProcedureToCatalogDto(p))
            .ToListAsync();
    }

    private async Task RecalculateTotalCostAsync(int visitId)
    {
        var visit = await context.Visits
            .Include(v => v.ProceduresPerformed)
            .Include(v => v.Prescriptions)
            .FirstOrDefaultAsync(v => v.Id == visitId);

        if (visit == null) return;

        visit.TotalCost = visit.ProceduresPerformed.Sum(p => p.ActualCost)
                          + visit.Prescriptions.Sum(p => p.TotalCost);

        await context.SaveChangesAsync();
    }
}
