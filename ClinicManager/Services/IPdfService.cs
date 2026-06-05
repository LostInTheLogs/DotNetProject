using ClinicManager.DTOs;

namespace ClinicManager.Services;

public interface IPdfService
{
    byte[] GenerateVisitSummaryPdf(VisitDetailsDto visit, PatientResponseDto patient, IEnumerable<ClinicalNoteResponseDto> notes);
    byte[] GeneratePrescriptionPdf(VisitDetailsDto visit, PatientResponseDto patient);
    byte[] GenerateServiceCostReportPdf(ServiceCostReportDto report);
}
