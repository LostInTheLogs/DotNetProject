using System;
using System.Collections.Generic;
using ClinicManager.DTOs;
using ClinicManager.Models;
using ClinicManager.Services;
using QuestPDF.Infrastructure;
using Xunit;

namespace ClinicManager.Tests;

public class PdfServiceTests
{
    static PdfServiceTests()
    {
        // Register QuestPDF license for tests
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [Fact]
    public void GenerateVisitSummaryPdf_ReturnsNonEmptyBytes()
    {
        // Arrange
        var service = new PdfService();
        var visit = new VisitDetailsDto
        {
            Id = 123,
            PatientId = 1,
            PatientFullName = "John Doe",
            DoctorId = "doc-1",
            DoctorFullName = "Dr. House",
            ScheduledDate = DateTime.UtcNow,
            Status = VisitStatus.Completed,
            Reason = "Routine Checkup",
            TotalCost = 150.00m,
            Procedures = new List<ProcedurePerformedResponseDto>
            {
                new() { Id = 1, MedicalProcedureId = 10, ProcedureName = "Blood Test", ActualCost = 50.00m, Notes = "Routine lab work" }
            },
            Prescriptions = new List<PrescribedMedicationResponseDto>
            {
                new() { Id = 2, MedicationId = 5, MedicationName = "Aspirin", Dosage = "1 tablet daily", Quantity = 1, TotalCost = 10.00m }
            }
        };

        var patient = new PatientResponseDto(
            1,
            "John",
            "Doe",
            "12345678901",
            "INS-7788",
            "555-0199",
            "john.doe@example.com",
            "123 Maple St, Springfield",
            DateTime.UtcNow
        );

        var notes = new List<ClinicalNoteResponseDto>
        {
            new() { Id = 1, VisitId = 123, AuthorId = "doc-1", AuthorName = "Dr. House", NoteType = "Diagnosis", Content = "Healthy patient", CreatedAt = DateTime.UtcNow }
        };

        // Act
        var result = service.GenerateVisitSummaryPdf(visit, patient, notes);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public void GeneratePrescriptionPdf_ReturnsNonEmptyBytes()
    {
        // Arrange
        var service = new PdfService();
        var visit = new VisitDetailsDto
        {
            Id = 123,
            PatientId = 1,
            PatientFullName = "John Doe",
            DoctorId = "doc-1",
            DoctorFullName = "Dr. House",
            ScheduledDate = DateTime.UtcNow,
            Status = VisitStatus.Completed,
            Reason = "Routine Checkup",
            TotalCost = 150.00m,
            Prescriptions = new List<PrescribedMedicationResponseDto>
            {
                new() { Id = 2, MedicationId = 5, MedicationName = "Aspirin", Dosage = "1 tablet daily", Quantity = 1, TotalCost = 10.00m }
            }
        };

        var patient = new PatientResponseDto(
            1,
            "John",
            "Doe",
            "12345678901",
            "INS-7788",
            "555-0199",
            "john.doe@example.com",
            "123 Maple St, Springfield",
            DateTime.UtcNow
        );

        // Act
        var result = service.GeneratePrescriptionPdf(visit, patient);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }
}
