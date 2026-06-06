using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;

namespace ClinicManager.Tests;

public class ClinicMapperTests
{
    private static readonly ClinicMapper Mapper = new();

    // ==========================================
    // Patient mappings
    // ==========================================

    [Fact]
    public void PatientToResponseDto_MapsAllProperties()
    {
        var patient = new Patient
        {
            Id = 42,
            FirstName = "Jan",
            LastName = "Kowalski",
            Pesel = "12345678901",
            InsuranceNumber = "INS001",
            Phone = "+48123456789",
            Email = "jan@example.com",
            Address = "Ul. Testowa 1",
            CreatedAt = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc)
        };

        var dto = Mapper.PatientToResponseDto(patient);

        Assert.Equal(patient.Id, dto.Id);
        Assert.Equal(patient.FirstName, dto.FirstName);
        Assert.Equal(patient.LastName, dto.LastName);
        Assert.Equal(patient.Pesel, dto.Pesel);
        Assert.Equal(patient.InsuranceNumber, dto.InsuranceNumber);
        Assert.Equal(patient.Phone, dto.Phone);
        Assert.Equal(patient.Email, dto.Email);
        Assert.Equal(patient.Address, dto.Address);
        Assert.Equal(patient.CreatedAt, dto.CreatedAt);
    }

    [Fact]
    public void CreateDtoToPatient_MapsAllProperties()
    {
        var dto = new CreatePatientRequestDto(
            "Anna", "Nowak", "98765432109", "INS002",
            "+48987654321", "anna@example.com", "Ul. Nowa 2"
        );

        var patient = Mapper.CreateDtoToPatient(dto);

        Assert.Equal(dto.FirstName, patient.FirstName);
        Assert.Equal(dto.LastName, patient.LastName);
        Assert.Equal(dto.Pesel, patient.Pesel);
        Assert.Equal(dto.InsuranceNumber, patient.InsuranceNumber);
        Assert.Equal(dto.Phone, patient.Phone);
        Assert.Equal(dto.Email, patient.Email);
        Assert.Equal(dto.Address, patient.Address);
        Assert.False(patient.IsDeleted);
    }

    [Fact]
    public void UpdatePatientFromDto_UpdatesOnlySpecifiedProperties()
    {
        var patient = new Patient
        {
            Id = 1,
            FirstName = "OldName",
            LastName = "OldSurname",
            Pesel = "12345678901",
            InsuranceNumber = "INS001",
            Phone = "+48111111111",
            Email = "old@example.com",
            Address = "Old Address",
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var dto = new UpdatePatientRequestDto(
            "NewName", "NewSurname", "+48222222222", "new@example.com", "New Address"
        );

        Mapper.UpdatePatientFromDto(dto, patient);

        Assert.Equal("NewName", patient.FirstName);
        Assert.Equal("NewSurname", patient.LastName);
        Assert.Equal("New Address", patient.Address);
        Assert.Equal("+48222222222", patient.Phone);
        Assert.Equal("new@example.com", patient.Email);
        Assert.Equal("12345678901", patient.Pesel);
        Assert.Equal("INS001", patient.InsuranceNumber);
        Assert.Equal(1, patient.Id);
    }

    // ==========================================
    // Visit mappings
    // ==========================================

    [Fact]
    public void VisitToResponseDto_MapsWithFullNameResolvers()
    {
        var patient = new Patient
        {
            Id = 10,
            FirstName = "Jan",
            LastName = "Kowalski"
        };

        var doctor = new ApplicationUser
        {
            Id = "doc-1",
            FirstName = "Adam",
            LastName = "Lekarski"
        };

        var visit = new Visit
        {
            Id = 100,
            PatientId = 10,
            Patient = patient,
            DoctorId = "doc-1",
            Doctor = doctor,
            ScheduledDate = new DateTime(2026, 6, 15, 9, 0, 0, DateTimeKind.Utc),
            Status = VisitStatus.Scheduled,
            Reason = "Annual checkup",
            TotalCost = 150.00m
        };

        var dto = Mapper.VisitToResponseDto(visit);

        Assert.Equal(visit.Id, dto.Id);
        Assert.Equal(visit.PatientId, dto.PatientId);
        Assert.Equal("Jan Kowalski", dto.PatientFullName);
        Assert.Equal(visit.DoctorId, dto.DoctorId);
        Assert.Equal("Adam Lekarski", dto.DoctorFullName);
        Assert.Equal(visit.ScheduledDate, dto.ScheduledDate);
        Assert.Equal(visit.Status, dto.Status);
        Assert.Equal(visit.Reason, dto.Reason);
        Assert.Equal(visit.TotalCost, dto.TotalCost);
    }

    [Fact]
    public void CreateDtoToVisit_MapsAllProperties()
    {
        var dto = new CreateVisitRequestDto(
            5, "doc-2", new DateTime(2026, 7, 1, 14, 30, 0, DateTimeKind.Utc), "Follow-up"
        );

        var visit = Mapper.CreateDtoToVisit(dto);

        Assert.Equal(dto.PatientId, visit.PatientId);
        Assert.Equal(dto.DoctorId, visit.DoctorId);
        Assert.Equal(dto.ScheduledDate, visit.ScheduledDate);
        Assert.Equal(dto.Reason, visit.Reason);
    }

    // ==========================================
    // Procedure mappings
    // ==========================================

    [Fact]
    public void ProcedureToResponseDto_MapsWithProcedureName()
    {
        var procedure = new MedicalProcedure
        {
            Id = 7,
            Name = "Konsultacja",
            ServiceCost = 200.00m
        };

        var performed = new ProcedurePerformed
        {
            Id = 1,
            MedicalProcedureId = 7,
            MedicalProcedure = procedure,
            ActualCost = 200.00m,
            Notes = "Routine consultation"
        };

        var dto = Mapper.ProcedureToResponseDto(performed);

        Assert.Equal(performed.Id, dto.Id);
        Assert.Equal(performed.MedicalProcedureId, dto.MedicalProcedureId);
        Assert.Equal("Konsultacja", dto.ProcedureName);
        Assert.Equal(performed.ActualCost, dto.ActualCost);
        Assert.Equal(performed.Notes, dto.Notes);
    }

    [Fact]
    public void LogDtoToProcedure_MapsAllProperties()
    {
        var dto = new LogProcedurePerformedDto(3, "Some clinical notes");

        var performed = Mapper.LogDtoToProcedure(dto);

        Assert.Equal(dto.MedicalProcedureId, performed.MedicalProcedureId);
        Assert.Equal(dto.Notes, performed.Notes);
    }

    // ==========================================
    // PrescribedMedication mappings
    // ==========================================

    [Fact]
    public void MedicationToResponseDto_MapsWithMedicationName()
    {
        var medication = new Medication
        {
            Id = 5,
            Name = "Paracetamol",
            UnitPrice = 12.50m
        };

        var prescribed = new PrescribedMedication
        {
            Id = 10,
            MedicationId = 5,
            Medication = medication,
            Dosage = "1 tablet 3x daily",
            Quantity = 30,
            TotalCost = 37.50m
        };

        var dto = Mapper.MedicationToResponseDto(prescribed);

        Assert.Equal(prescribed.Id, dto.Id);
        Assert.Equal(prescribed.MedicationId, dto.MedicationId);
        Assert.Equal("Paracetamol", dto.MedicationName);
        Assert.Equal(prescribed.Dosage, dto.Dosage);
        Assert.Equal(prescribed.Quantity, dto.Quantity);
        Assert.Equal(prescribed.TotalCost, dto.TotalCost);
    }

    [Fact]
    public void AddDtoToMedication_MapsAllProperties()
    {
        var dto = new AddPrescribedMedicationDto(5, "2 tablets daily", 60);

        var prescribed = Mapper.AddDtoToMedication(dto);

        Assert.Equal(dto.MedicationId, prescribed.MedicationId);
        Assert.Equal(dto.Dosage, prescribed.Dosage);
        Assert.Equal(dto.Quantity, prescribed.Quantity);
    }

    // ==========================================
    // Catalog mappings
    // ==========================================

    [Fact]
    public void MedicationToCatalogDto_MapsAllProperties()
    {
        var medication = new Medication
        {
            Id = 3,
            Name = "Amotaks",
            UnitPrice = 34.20m,
            Description = "Antibiotic",
            IsAvailable = true
        };

        var dto = Mapper.MedicationToCatalogDto(medication);

        Assert.Equal(medication.Id, dto.Id);
        Assert.Equal(medication.Name, dto.Name);
        Assert.Equal(medication.UnitPrice, dto.UnitPrice);
    }

    [Fact]
    public void ProcedureToCatalogDto_MapsAllProperties()
    {
        var procedure = new MedicalProcedure
        {
            Id = 2,
            Name = "Ultrasound",
            ServiceCost = 220.00m
        };

        var dto = Mapper.ProcedureToCatalogDto(procedure);

        Assert.Equal(procedure.Id, dto.Id);
        Assert.Equal(procedure.Name, dto.Name);
        Assert.Equal(procedure.ServiceCost, dto.ServiceCost);
    }

    // ==========================================
    // MedicalRecord mapping
    // ==========================================

    [Fact]
    public void RecordToResponseDto_MapsAllProperties()
    {
        var record = new MedicalRecord
        {
            Id = 15,
            PatientId = 3,
            DocumentType = "X-Ray",
            DocumentScanUrl = "/uploads/xray.pdf",
            Description = "Chest X-Ray",
            UploadedAt = new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc)
        };

        var dto = Mapper.RecordToResponseDto(record);

        Assert.Equal(record.Id, dto.Id);
        Assert.Equal(record.PatientId, dto.PatientId);
        Assert.Equal(record.DocumentType, dto.DocumentType);
        Assert.Equal(record.DocumentScanUrl, dto.DocumentScanUrl);
        Assert.Equal(record.Description, dto.Description);
        Assert.Equal(record.UploadedAt, dto.UploadedAt);
    }

    // ==========================================
    // ClinicalNote mapping
    // ==========================================

    [Fact]
    public void NoteToResponseDto_MapsWithAuthorName()
    {
        var author = new ApplicationUser
        {
            Id = "author-1",
            FirstName = "Piotr",
            LastName = "Autor"
        };

        var note = new ClinicalNote
        {
            Id = 7,
            VisitId = 100,
            AuthorId = "author-1",
            Author = author,
            NoteType = "Diagnosis",
            Content = "Patient is recovering well.",
            CreatedAt = new DateTime(2026, 4, 20, 15, 30, 0, DateTimeKind.Utc)
        };

        var dto = Mapper.NoteToResponseDto(note);

        Assert.Equal(note.Id, dto.Id);
        Assert.Equal(note.VisitId, dto.VisitId);
        Assert.Equal("Piotr Autor", dto.AuthorName);
        Assert.Equal(note.NoteType, dto.NoteType);
        Assert.Equal(note.Content, dto.Content);
        Assert.Equal(note.CreatedAt, dto.CreatedAt);
    }
}
