using System.ComponentModel.DataAnnotations;
using ClinicManager.Models;

namespace ClinicManager.DTOs;

public record VisitResponseDto(
    int Id,
    int PatientId,
    string PatientFullName,
    string DoctorId,
    string DoctorFullName,
    DateTime ScheduledDate,
    VisitStatus Status,
    string Reason,
    decimal TotalCost
);

public record CreateVisitRequestDto(
    [Required] int PatientId,
    [Required] string DoctorId,
    [Required] DateTime ScheduledDate,
    [Required][MaxLength(500)] string Reason
);

public record UpdateVisitStatusRequestDto(
    [Required] VisitStatus Status
);
