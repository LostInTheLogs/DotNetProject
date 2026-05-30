using ClinicManager.DTOs;
using ClinicManager.Models;

namespace ClinicManager.ViewModels;

public class DoctorScheduleViewModel
{
    public string SelectedDoctorId { get; set; } = string.Empty;
    public DateTime SelectedDate { get; set; }
    public IEnumerable<ApplicationUser> Doctors { get; set; } = new List<ApplicationUser>();
    public IEnumerable<PatientResponseDto> Patients { get; set; } = new List<PatientResponseDto>();
    public IEnumerable<VisitResponseDto> BookedVisits { get; set; } = new List<VisitResponseDto>();
    public IEnumerable<DateTime> AvailableSlots { get; set; } = new List<DateTime>();
}
