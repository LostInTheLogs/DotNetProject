using Riok.Mapperly.Abstractions;
using ClinicManager.Models;
using ClinicManager.DTOs;

namespace ClinicManager.Mappers;

[Mapper(PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseInsensitive)]
public partial class ClinicMapper
{
    // ==========================================
    // 1. PATIENT MAPPINGS
    // ==========================================

    [MapperIgnoreSource(nameof(Patient.MedicalRecords))]
    [MapperIgnoreSource(nameof(Patient.Visits))]
    [MapperIgnoreSource(nameof(Patient.IsDeleted))]
    public partial PatientResponseDto PatientToResponseDto(Patient patient);

    [MapperIgnoreTarget(nameof(Patient.Id))]
    [MapperIgnoreTarget(nameof(Patient.IsDeleted))]
    [MapperIgnoreTarget(nameof(Patient.CreatedAt))]
    [MapperIgnoreTarget(nameof(Patient.MedicalRecords))]
    [MapperIgnoreTarget(nameof(Patient.Visits))]
    public partial Patient CreateDtoToPatient(CreatePatientRequestDto dto);

    [MapperIgnoreTarget(nameof(Patient.Id))]
    [MapperIgnoreTarget(nameof(Patient.Pesel))]
    [MapperIgnoreTarget(nameof(Patient.InsuranceNumber))]
    [MapperIgnoreTarget(nameof(Patient.IsDeleted))]
    [MapperIgnoreTarget(nameof(Patient.CreatedAt))]
    [MapperIgnoreTarget(nameof(Patient.MedicalRecords))]
    [MapperIgnoreTarget(nameof(Patient.Visits))]
    public partial void UpdatePatientFromDto(UpdatePatientRequestDto dto, Patient patient);

    public partial IQueryable<PatientResponseDto> ProjectPatientsToResponse(IQueryable<Patient> patients);

    // ==========================================
    // 2. VISIT MAPPINGS
    // ==========================================

    [MapProperty(nameof(Visit.Patient), nameof(VisitResponseDto.PatientFullName))]
    [MapProperty(nameof(Visit.Doctor), nameof(VisitResponseDto.DoctorFullName))]
    [MapperIgnoreSource(nameof(Visit.ProceduresPerformed))]
    [MapperIgnoreSource(nameof(Visit.Prescriptions))]
    [MapperIgnoreSource(nameof(Visit.ClinicalNotes))]
    [MapperIgnoreSource("CreatedAt")]
    public partial VisitResponseDto VisitToResponseDto(Visit visit);

    [MapperIgnoreTarget(nameof(Visit.Id))]
    [MapperIgnoreTarget(nameof(Visit.Status))]
    [MapperIgnoreTarget(nameof(Visit.TotalCost))]
    [MapperIgnoreTarget(nameof(Visit.Patient))]
    [MapperIgnoreTarget(nameof(Visit.Doctor))]
    [MapperIgnoreTarget(nameof(Visit.ProceduresPerformed))]
    [MapperIgnoreTarget(nameof(Visit.Prescriptions))]
    [MapperIgnoreTarget(nameof(Visit.ClinicalNotes))]
    [MapperIgnoreTarget("CreatedAt")]
    public partial Visit CreateDtoToVisit(CreateVisitRequestDto dto);

    // ==========================================
    // 3. VISIT DETAILS & TRANSACTION MAPPINGS
    // ==========================================

    [MapProperty("MedicalProcedure.Name", nameof(ProcedurePerformedResponseDto.ProcedureName))]
    [MapperIgnoreSource(nameof(ProcedurePerformed.Visit))]
    [MapperIgnoreSource(nameof(ProcedurePerformed.VisitId))]
    [MapperIgnoreSource("PrescribedMedications")]
    public partial ProcedurePerformedResponseDto ProcedureToResponseDto(ProcedurePerformed proc);

    [MapperIgnoreTarget(nameof(ProcedurePerformed.Id))]
    [MapperIgnoreTarget(nameof(ProcedurePerformed.VisitId))]
    [MapperIgnoreTarget(nameof(ProcedurePerformed.Visit))]
    [MapperIgnoreTarget(nameof(ProcedurePerformed.MedicalProcedure))]
    [MapperIgnoreTarget("ActualCost")] // Set downstream by service logic, not raw input DTO
    [MapperIgnoreTarget("PrescribedMedications")]
    public partial ProcedurePerformed LogDtoToProcedure(LogProcedurePerformedDto dto);

    [MapProperty("Medication.Name", nameof(PrescribedMedicationResponseDto.MedicationName))]
    [MapperIgnoreSource(nameof(PrescribedMedication.Visit))]
    [MapperIgnoreSource(nameof(PrescribedMedication.VisitId))]
    [MapperIgnoreSource(nameof(PrescribedMedication.ProcedurePerformedId))]
    [MapperIgnoreSource(nameof(PrescribedMedication.ProcedurePerformed))]
    public partial PrescribedMedicationResponseDto MedicationToResponseDto(PrescribedMedication med);

    [MapperIgnoreTarget(nameof(PrescribedMedication.Id))]
    [MapperIgnoreTarget(nameof(PrescribedMedication.VisitId))]
    [MapperIgnoreTarget(nameof(PrescribedMedication.Visit))]
    [MapperIgnoreTarget(nameof(PrescribedMedication.ProcedurePerformedId))]
    [MapperIgnoreTarget(nameof(PrescribedMedication.ProcedurePerformed))]
    [MapperIgnoreTarget(nameof(PrescribedMedication.Medication))]
    [MapperIgnoreTarget(nameof(PrescribedMedication.TotalCost))]
    public partial PrescribedMedication AddDtoToMedication(AddPrescribedMedicationDto dto);

    // ==========================================
    // 4. CATALOG LOOKUPS
    // ==========================================

    [MapperIgnoreSource("Description")]
    [MapperIgnoreSource("IsAvailable")]
    public partial MedicationDto MedicationToCatalogDto(Medication medication);

    [MapperIgnoreSource("Description")]
    public partial MedicalProcedureDto ProcedureToCatalogDto(MedicalProcedure procedure);

    // ==========================================
    // 5. MEDICAL RECORDS & CLINICAL NOTES
    // ==========================================

    [MapperIgnoreSource(nameof(MedicalRecord.Patient))]
    public partial MedicalRecordResponseDto RecordToResponseDto(MedicalRecord record);

    [MapProperty(nameof(ClinicalNote.Author), nameof(ClinicalNoteResponseDto.AuthorName))]
    [MapperIgnoreSource(nameof(ClinicalNote.Visit))]
    [MapperIgnoreSource(nameof(ClinicalNote.AuthorId))]
    public partial ClinicalNoteResponseDto NoteToResponseDto(ClinicalNote note);

    // ==========================================
    // PROPERTY RESOLVERS
    // ==========================================

    private string MapPatientToPatientFullName(Patient patient) =>
        $"{patient.FirstName} {patient.LastName}";

    private string MapApplicationUserToDoctorFullName(ApplicationUser doctor) =>
        $"{doctor.FirstName} {doctor.LastName}";

    private string MapApplicationUserToAuthorName(ApplicationUser author) =>
        $"{author.FirstName} {author.LastName}";
}
