using Microsoft.EntityFrameworkCore;
using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Models;

namespace ClinicManager.Services;

public class ServiceCostReportService(ApplicationDbContext context) : IServiceCostReportService
{
    public async Task<ServiceCostReportDto> GetReportAsync(ServiceCostReportFilterDto filter)
    {
        var query = context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .Include(v => v.ProceduresPerformed)
            .Include(v => v.Prescriptions)
            .Where(v => v.Status == VisitStatus.Completed)
            .AsNoTracking()
            .AsSplitQuery();

        if (filter.PatientId.HasValue)
            query = query.Where(v => v.PatientId == filter.PatientId.Value);

        if (!string.IsNullOrEmpty(filter.DoctorId))
            query = query.Where(v => v.DoctorId == filter.DoctorId);

        if (filter.Month.HasValue)
            query = query.Where(v => v.ScheduledDate.Month == filter.Month.Value);

        if (filter.Year.HasValue)
            query = query.Where(v => v.ScheduledDate.Year == filter.Year.Value);

        var visits = await query
            .OrderByDescending(v => v.ScheduledDate)
            .ToListAsync();

        var lines = visits.Select(v => new ServiceCostReportLineDto
        {
            VisitId = v.Id,
            ScheduledDate = v.ScheduledDate,
            PatientName = $"{v.Patient!.FirstName} {v.Patient.LastName}",
            DoctorName = $"{v.Doctor!.FirstName} {v.Doctor.LastName}",
            Reason = v.Reason,
            ProcedureCount = v.ProceduresPerformed.Count,
            MedicationCount = v.Prescriptions.Count,
            TotalCost = v.TotalCost
        }).ToList();

        return new ServiceCostReportDto
        {
            Lines = lines,
            GrandTotal = lines.Sum(l => l.TotalCost),
            TotalVisits = lines.Count,
            FilterDescription = BuildFilterDescription(filter)
        };
    }

    private static string BuildFilterDescription(ServiceCostReportFilterDto filter)
    {
        var parts = new List<string>();

        if (filter.PatientId.HasValue)
            parts.Add("Patient");

        if (!string.IsNullOrEmpty(filter.DoctorId))
            parts.Add("Doctor");

        if (filter.Month.HasValue && filter.Year.HasValue)
            parts.Add($"{new DateTime(filter.Year.Value, filter.Month.Value, 1):MMMM yyyy}");
        else if (filter.Month.HasValue)
            parts.Add($"Month {filter.Month}");
        else if (filter.Year.HasValue)
            parts.Add($"Year {filter.Year}");

        return parts.Count > 0
            ? $"Service cost report filtered by: {string.Join(", ", parts)}"
            : "Service cost report - all completed visits";
    }
}
