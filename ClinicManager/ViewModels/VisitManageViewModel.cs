using ClinicManager.DTOs;

namespace ClinicManager.ViewModels;

public class VisitManageViewModel
{
    public VisitDetailsDto Visit { get; set; } = null!;
    public List<MedicalProcedureDto> AvailableProcedures { get; set; } = new();
    public List<MedicationDto> AvailableMedications { get; set; } = new();
    public LogProcedurePerformedDto ProcedureForm { get; set; } = new(0, string.Empty);
    public AddPrescribedMedicationDto MedicationForm { get; set; } = new(0, string.Empty, 1);
}
