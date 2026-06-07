using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;
using ClinicManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Tests;

public class VisitServiceTests
{
    private static readonly ClinicMapper Mapper = new();

    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static VisitService CreateService(ApplicationDbContext context)
        => new(context, Mapper);

    private static async Task SeedBaseDataAsync(ApplicationDbContext context)
    {
        context.Patients.AddRange(
            new Patient { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", InsuranceNumber = "I1", Phone = "+481", Email = "j@t.com", Address = "Addr" },
            new Patient { Id = 2, FirstName = "Anna", LastName = "Nowak", Pesel = "98765432109", InsuranceNumber = "I2", Phone = "+482", Email = "a@t.com", Address = "Addr" }
        );
        context.Users.AddRange(
            new ApplicationUser { Id = "doc-1", FirstName = "Adam", LastName = "Lekarski", UserName = "dr.lekarski@clinic.com" },
            new ApplicationUser { Id = "doc-2", FirstName = "Ewa", LastName = "Medyk", UserName = "dr.medyk@clinic.com" }
        );
        context.MedicalProcedures.AddRange(
            new MedicalProcedure { Id = 1, Name = "Konsultacja", Description = "Standard consultation", ServiceCost = 150.00m },
            new MedicalProcedure { Id = 2, Name = "USG", Description = "Ultrasound", ServiceCost = 220.00m }
        );
        context.Medications.AddRange(
            new Medication { Id = 1, Name = "Paracetamol", Description = "Painkiller", UnitPrice = 12.50m },
            new Medication { Id = 2, Name = "Amotaks", Description = "Antibiotic", UnitPrice = 34.20m }
        );
        await context.SaveChangesAsync();
    }

    // ==========================================
    // CreateVisitAsync
    // ==========================================

    [Fact]
    public async Task CreateVisitAsync_CreatesAndReturnsVisit()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        await SeedBaseDataAsync(context);
        var service = CreateService(context);

        var dto = new CreateVisitRequestDto(
            1, "doc-1", new DateTime(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc), "Routine checkup"
        );

        var result = await service.CreateVisitAsync(dto);

        Assert.Equal(1, result.PatientId);
        Assert.Equal("doc-1", result.DoctorId);
        Assert.Equal(VisitStatus.Scheduled, result.Status);
        Assert.Equal("Routine checkup", result.Reason);
        Assert.NotEqual(0, result.Id);
    }

    [Fact]
    public async Task CreateVisitAsync_TimeSlotConflict_Throws()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        await SeedBaseDataAsync(context);
        var service = CreateService(context);

        var slot = new DateTime(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        context.Visits.Add(new Visit
        {
            Id = 1,
            PatientId = 1,
            DoctorId = "doc-1",
            ScheduledDate = slot,
            Status = VisitStatus.Scheduled,
            Reason = "Existing"
        });
        await context.SaveChangesAsync();

        var dto = new CreateVisitRequestDto(2, "doc-1", slot, "Conflict");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateVisitAsync(dto));
        Assert.Contains("already booked", ex.Message);
    }

    [Fact]
    public async Task CreateVisitAsync_CancelledSlot_DoesNotConflict()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        await SeedBaseDataAsync(context);
        var service = CreateService(context);

        var slot = new DateTime(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        context.Visits.Add(new Visit
        {
            Id = 1,
            PatientId = 1,
            DoctorId = "doc-1",
            ScheduledDate = slot,
            Status = VisitStatus.Cancelled,
            Reason = "Cancelled"
        });
        await context.SaveChangesAsync();

        var dto = new CreateVisitRequestDto(2, "doc-1", slot, "New booking");
        var result = await service.CreateVisitAsync(dto);

        Assert.NotNull(result);
    }

    // ==========================================
    // GetByIdAsync
    // ==========================================

    [Fact]
    public async Task GetByIdAsync_ExistingVisit_ReturnsDto()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Patients.Add(new Patient { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", InsuranceNumber = "I1", Phone = "+481", Email = "j@t.com", Address = "Addr" });
        context.Users.Add(new ApplicationUser { Id = "doc-1", FirstName = "Adam", LastName = "Lekarski", UserName = "dr@clinic.com" });
        context.Visits.Add(new Visit { Id = 10, PatientId = 1, DoctorId = "doc-1", ScheduledDate = new DateTime(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc), Status = VisitStatus.Scheduled, Reason = "Checkup" });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetByIdAsync(10);

        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal(1, result.PatientId);
        Assert.Equal("doc-1", result.DoctorId);
        Assert.Equal(VisitStatus.Scheduled, result.Status);
        Assert.Equal("Checkup", result.Reason);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingVisit_ReturnsNull()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    // ==========================================
    // GetByDoctorScheduleAsync
    // ==========================================

    [Fact]
    public async Task GetByDoctorScheduleAsync_ReturnsFilteredVisits()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Patients.Add(new Patient { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", InsuranceNumber = "I1", Phone = "+481", Email = "j@t.com", Address = "Addr" });
        context.Users.Add(new ApplicationUser { Id = "doc-1", FirstName = "Adam", LastName = "Lekarski", UserName = "dr@clinic.com" });
        var day = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        context.Visits.AddRange(
            new Visit { Id = 1, PatientId = 1, DoctorId = "doc-1", ScheduledDate = day.AddHours(9), Status = VisitStatus.Scheduled, Reason = "R1" },
            new Visit { Id = 2, PatientId = 1, DoctorId = "doc-1", ScheduledDate = day.AddHours(10), Status = VisitStatus.Scheduled, Reason = "R2" },
            new Visit { Id = 3, PatientId = 1, DoctorId = "doc-1", ScheduledDate = day.AddDays(1).AddHours(9), Status = VisitStatus.Scheduled, Reason = "Next day" }
        );
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetByDoctorScheduleAsync("doc-1", day);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByDoctorScheduleAsync_NoVisits_ReturnsEmpty()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Users.Add(new ApplicationUser { Id = "doc-1", FirstName = "Adam", LastName = "Lekarski", UserName = "dr@clinic.com" });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetByDoctorScheduleAsync("doc-1", new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc));

        Assert.Empty(result);
    }

    // ==========================================
    // GetPatientVisitHistoryAsync
    // ==========================================

    [Fact]
    public async Task GetPatientVisitHistoryAsync_ReturnsVisitsOrderedByDateDesc()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Patients.Add(new Patient { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", InsuranceNumber = "I1", Phone = "+481", Email = "j@t.com", Address = "Addr" });
        context.Users.Add(new ApplicationUser { Id = "doc-1", FirstName = "Adam", LastName = "Lekarski", UserName = "dr@clinic.com" });
        context.Visits.AddRange(
            new Visit { Id = 1, PatientId = 1, DoctorId = "doc-1", ScheduledDate = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc), Status = VisitStatus.Completed, Reason = "Old" },
            new Visit { Id = 2, PatientId = 1, DoctorId = "doc-1", ScheduledDate = new DateTime(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc), Status = VisitStatus.Completed, Reason = "Recent" }
        );
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetPatientVisitHistoryAsync(1);

        Assert.Equal(2, result.Count());
        Assert.Equal("Recent", result.First().Reason);
        Assert.Equal("Old", result.Last().Reason);
    }

    [Fact]
    public async Task GetPatientVisitHistoryAsync_NoVisits_ReturnsEmpty()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Patients.Add(new Patient { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", InsuranceNumber = "I1", Phone = "+481", Email = "j@t.com", Address = "Addr" });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetPatientVisitHistoryAsync(1);

        Assert.Empty(result);
    }

    // ==========================================
    // UpdateStatusAsync
    // ==========================================

    [Fact]
    public async Task UpdateStatusAsync_ScheduledToInProgress_Updates()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Patients.Add(new Patient { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", InsuranceNumber = "I1", Phone = "+481", Email = "j@t.com", Address = "Addr" });
        context.Users.Add(new ApplicationUser { Id = "doc-1", FirstName = "Adam", LastName = "Lekarski", UserName = "dr@clinic.com" });
        context.Visits.Add(new Visit { Id = 1, PatientId = 1, DoctorId = "doc-1", ScheduledDate = DateTime.UtcNow, Status = VisitStatus.Scheduled, Reason = "Test" });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.UpdateStatusAsync(1, VisitStatus.InProgress);

        Assert.Equal(VisitStatus.InProgress, result.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_CompletedVisit_Throws()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Patients.Add(new Patient { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", InsuranceNumber = "I1", Phone = "+481", Email = "j@t.com", Address = "Addr" });
        context.Users.Add(new ApplicationUser { Id = "doc-1", FirstName = "Adam", LastName = "Lekarski", UserName = "dr@clinic.com" });
        context.Visits.Add(new Visit { Id = 1, PatientId = 1, DoctorId = "doc-1", ScheduledDate = DateTime.UtcNow, Status = VisitStatus.Completed, Reason = "Done" });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateStatusAsync(1, VisitStatus.InProgress));
    }

    [Fact]
    public async Task UpdateStatusAsync_NonExisting_Throws()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateStatusAsync(999, VisitStatus.Completed));
    }

    // ==========================================
    // GetVisitDetailsAsync
    // ==========================================

    [Fact]
    public async Task GetVisitDetailsAsync_ReturnsFullDetails()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Patients.Add(new Patient { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", InsuranceNumber = "I1", Phone = "+481", Email = "j@t.com", Address = "Addr" });
        context.Users.Add(new ApplicationUser { Id = "doc-1", FirstName = "Adam", LastName = "Lekarski", UserName = "dr@clinic.com" });
        context.MedicalProcedures.Add(new MedicalProcedure { Id = 10, Name = "Konsultacja", Description = "Consult", ServiceCost = 150.00m });
        context.Medications.Add(new Medication { Id = 20, Name = "Paracetamol", Description = "Painkiller", UnitPrice = 12.50m });

        context.Visits.Add(new Visit
        {
            Id = 5,
            PatientId = 1,
            DoctorId = "doc-1",
            ScheduledDate = new DateTime(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc),
            Status = VisitStatus.Completed,
            Reason = "Checkup",
            TotalCost = 150.00m + 37.50m,
            ProceduresPerformed = new List<ProcedurePerformed>
            {
                new() { Id = 1, MedicalProcedureId = 10, ActualCost = 150.00m, Notes = "Routine" }
            },
            Prescriptions = new List<PrescribedMedication>
            {
                new() { Id = 1, MedicationId = 20, Dosage = "1x3", Quantity = 30, TotalCost = 37.50m }
            }
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetVisitDetailsAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id);
        Assert.Single(result.Procedures);
        Assert.Single(result.Prescriptions);
        Assert.Equal(150.00m + 37.50m, result.TotalCost);
    }

    [Fact]
    public async Task GetVisitDetailsAsync_NonExisting_ReturnsNull()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        var result = await service.GetVisitDetailsAsync(999);

        Assert.Null(result);
    }

    // ==========================================
    // AddProcedureAsync
    // ==========================================

    [Fact]
    public async Task AddProcedureAsync_AddsAndRecalculatesCost()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.MedicalProcedures.Add(new MedicalProcedure { Id = 1, Name = "Konsultacja", Description = "Consult", ServiceCost = 150.00m });
        context.Patients.Add(new Patient { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", InsuranceNumber = "I1", Phone = "+481", Email = "j@t.com", Address = "Addr" });
        context.Users.Add(new ApplicationUser { Id = "doc-1", FirstName = "Adam", LastName = "Lekarski", UserName = "dr@clinic.com" });
        context.Visits.Add(new Visit { Id = 1, PatientId = 1, DoctorId = "doc-1", ScheduledDate = DateTime.UtcNow, Status = VisitStatus.Scheduled, Reason = "Test" });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var dto = new LogProcedurePerformedDto(1, "Clinical notes");
        await service.AddProcedureAsync(1, dto);

        var visit = await context.Visits.FindAsync(1);
        Assert.NotNull(visit);
        Assert.Equal(150.00m, visit.TotalCost);
    }

    [Fact]
    public async Task AddProcedureAsync_NonExistingVisit_Throws()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        var dto = new LogProcedurePerformedDto(1, "Notes");
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.AddProcedureAsync(999, dto));
    }

    // ==========================================
    // RemoveProcedureAsync
    // ==========================================

    [Fact]
    public async Task RemoveProcedureAsync_RemovesAndRecalculatesCost()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.MedicalProcedures.Add(new MedicalProcedure { Id = 1, Name = "Konsultacja", Description = "Consult", ServiceCost = 150.00m });
        context.Patients.Add(new Patient { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", InsuranceNumber = "I1", Phone = "+481", Email = "j@t.com", Address = "Addr" });
        context.Users.Add(new ApplicationUser { Id = "doc-1", FirstName = "Adam", LastName = "Lekarski", UserName = "dr@clinic.com" });
        context.Visits.Add(new Visit { Id = 1, PatientId = 1, DoctorId = "doc-1", ScheduledDate = DateTime.UtcNow, Status = VisitStatus.Scheduled, Reason = "Test", TotalCost = 150.00m });
        context.ProceduresPerformed.Add(new ProcedurePerformed { Id = 10, VisitId = 1, MedicalProcedureId = 1, ActualCost = 150.00m });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await service.RemoveProcedureAsync(10);

        var visit = await context.Visits.FindAsync(1);
        Assert.NotNull(visit);
        Assert.Equal(0, visit.TotalCost);
        Assert.Null(await context.ProceduresPerformed.FindAsync(10));
    }

    [Fact]
    public async Task RemoveProcedureAsync_NonExisting_Throws()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.RemoveProcedureAsync(999));
    }

    // ==========================================
    // AddPrescriptionAsync
    // ==========================================

    [Fact]
    public async Task AddPrescriptionAsync_AddsAndRecalculatesCost()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Medications.Add(new Medication { Id = 1, Name = "Paracetamol", Description = "Painkiller", UnitPrice = 12.50m });
        context.Patients.Add(new Patient { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", InsuranceNumber = "I1", Phone = "+481", Email = "j@t.com", Address = "Addr" });
        context.Users.Add(new ApplicationUser { Id = "doc-1", FirstName = "Adam", LastName = "Lekarski", UserName = "dr@clinic.com" });
        context.Visits.Add(new Visit { Id = 1, PatientId = 1, DoctorId = "doc-1", ScheduledDate = DateTime.UtcNow, Status = VisitStatus.Scheduled, Reason = "Test" });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var dto = new AddPrescribedMedicationDto(1, "1 tablet 3x daily", 30);
        await service.AddPrescriptionAsync(1, dto);

        var visit = await context.Visits.FindAsync(1);
        Assert.NotNull(visit);
        Assert.Equal(375.00m, visit.TotalCost);
    }

    [Fact]
    public async Task AddPrescriptionAsync_NonExistingVisit_Throws()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        var dto = new AddPrescribedMedicationDto(1, "1x3", 30);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.AddPrescriptionAsync(999, dto));
    }

    // ==========================================
    // RemovePrescriptionAsync
    // ==========================================

    [Fact]
    public async Task RemovePrescriptionAsync_RemovesAndRecalculates()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Medications.Add(new Medication { Id = 1, Name = "Paracetamol", Description = "Painkiller", UnitPrice = 12.50m });
        context.Patients.Add(new Patient { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", InsuranceNumber = "I1", Phone = "+481", Email = "j@t.com", Address = "Addr" });
        context.Users.Add(new ApplicationUser { Id = "doc-1", FirstName = "Adam", LastName = "Lekarski", UserName = "dr@clinic.com" });
        context.Visits.Add(new Visit { Id = 1, PatientId = 1, DoctorId = "doc-1", ScheduledDate = DateTime.UtcNow, Status = VisitStatus.Scheduled, Reason = "Test", TotalCost = 37.50m });
        context.PrescribedMedications.Add(new PrescribedMedication { Id = 10, VisitId = 1, MedicationId = 1, Dosage = "1x3", Quantity = 30, TotalCost = 37.50m });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        await service.RemovePrescriptionAsync(10);

        var visit = await context.Visits.FindAsync(1);
        Assert.NotNull(visit);
        Assert.Equal(0, visit.TotalCost);
    }

    [Fact]
    public async Task RemovePrescriptionAsync_NonExisting_Throws()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.RemovePrescriptionAsync(999));
    }

    // ==========================================
    // GetAllProceduresAsync
    // ==========================================

    [Fact]
    public async Task GetAllProceduresAsync_ReturnsOrderedProcedures()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.MedicalProcedures.AddRange(
            new MedicalProcedure { Id = 2, Name = "USG", Description = "Ultrasound", ServiceCost = 220.00m },
            new MedicalProcedure { Id = 1, Name = "Konsultacja", Description = "Consultation", ServiceCost = 150.00m }
        );
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetAllProceduresAsync();

        Assert.Equal(2, result.Count());
        Assert.Equal("Konsultacja", result.First().Name);
        Assert.Equal("USG", result.Last().Name);
    }
}
