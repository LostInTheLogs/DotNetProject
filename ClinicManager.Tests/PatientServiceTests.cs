using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;
using ClinicManager.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClinicManager.Tests;

public class PatientServiceTests
{
    private static readonly ClinicMapper Mapper = new();

    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static PatientService CreateService(ApplicationDbContext context)
    {
        var loggerMock = new Mock<ILogger<PatientService>>();
        return new PatientService(context, Mapper, loggerMock.Object);
    }

    // ==========================================
    // GetAllAsync
    // ==========================================

    [Fact]
    public async Task GetAllAsync_ReturnsAllPatientsOrderedByCreatedAtDesc()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        var older = new Patient { Id = 1, FirstName = "A", LastName = "B", Pesel = "11111111111", InsuranceNumber = "I1", Phone = "+481", Email = "a@b.com", Address = "Addr", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        var newer = new Patient { Id = 2, FirstName = "C", LastName = "D", Pesel = "22222222222", InsuranceNumber = "I2", Phone = "+482", Email = "c@d.com", Address = "Addr", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        context.Patients.AddRange(older, newer);
        await context.SaveChangesAsync();

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal(2, result[0].Id);
        Assert.Equal(1, result[1].Id);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        var result = await service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeletedPatients()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        context.Patients.AddRange(
            new Patient { Id = 1, FirstName = "Active", LastName = "User", Pesel = "11111111111", InsuranceNumber = "I1", Phone = "+481", Email = "a@b.com", Address = "Addr" },
            new Patient { Id = 2, FirstName = "Deleted", LastName = "User", Pesel = "22222222222", InsuranceNumber = "I2", Phone = "+482", Email = "c@d.com", Address = "Addr", IsDeleted = true }
        );
        await context.SaveChangesAsync();

        var result = await service.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Active", result[0].FirstName);
    }

    // ==========================================
    // GetByIdAsync
    // ==========================================

    [Fact]
    public async Task GetByIdAsync_ExistingPatient_ReturnsDto()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        context.Patients.Add(new Patient
        {
            Id = 99, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901",
            InsuranceNumber = "INS01", Phone = "+48123456789", Email = "jan@test.com",
            Address = "Ul. Testowa 1"
        });
        await context.SaveChangesAsync();

        var result = await service.GetByIdAsync(99);

        Assert.NotNull(result);
        Assert.Equal("Jan", result.FirstName);
        Assert.Equal("Kowalski", result.LastName);
        Assert.Equal("12345678901", result.Pesel);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingPatient_ReturnsNull()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    // ==========================================
    // SearchAsync
    // ==========================================

    [Fact]
    public async Task SearchAsync_NullOrEmpty_ReturnsAll()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        context.Patients.AddRange(
            new Patient { Id = 1, FirstName = "Jan", LastName = "A", Pesel = "11111111111", InsuranceNumber = "I1", Phone = "+481", Email = "a@b.com", Address = "Addr" },
            new Patient { Id = 2, FirstName = "Anna", LastName = "B", Pesel = "22222222222", InsuranceNumber = "I2", Phone = "+482", Email = "c@d.com", Address = "Addr" }
        );
        await context.SaveChangesAsync();

        Assert.Equal(2, (await service.SearchAsync(null)).Count);
        Assert.Equal(2, (await service.SearchAsync("")).Count);
        Assert.Equal(2, (await service.SearchAsync("   ")).Count);
    }

    [Fact]
    public async Task SearchAsync_ByPesel_ReturnsMatchingPatient()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        context.Patients.AddRange(
            new Patient { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901", InsuranceNumber = "I1", Phone = "+481", Email = "a@b.com", Address = "Addr" },
            new Patient { Id = 2, FirstName = "Anna", LastName = "Nowak", Pesel = "98765432109", InsuranceNumber = "I2", Phone = "+482", Email = "c@d.com", Address = "Addr" }
        );
        await context.SaveChangesAsync();

        var result = await service.SearchAsync("12345678901");

        Assert.Single(result);
        Assert.Equal("Jan", result[0].FirstName);
    }

    [Fact]
    public async Task SearchAsync_ByName_ReturnsMatchingPatients()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        context.Patients.AddRange(
            new Patient { Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "11111111111", InsuranceNumber = "I1", Phone = "+481", Email = "a@b.com", Address = "Addr" },
            new Patient { Id = 2, FirstName = "Anna", LastName = "Nowak", Pesel = "22222222222", InsuranceNumber = "I2", Phone = "+482", Email = "c@d.com", Address = "Addr" },
            new Patient { Id = 3, FirstName = "Piotr", LastName = "Kowalski", Pesel = "33333333333", InsuranceNumber = "I3", Phone = "+483", Email = "e@f.com", Address = "Addr" }
        );
        await context.SaveChangesAsync();

        var result = await service.SearchAsync("Kowalski");

        Assert.Equal(2, result.Count);
    }

    // ==========================================
    // CreateAsync
    // ==========================================

    [Fact]
    public async Task CreateAsync_PersistsAndReturnsDto()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        var dto = new CreatePatientRequestDto(
            "Jan", "Kowalski", "12345678901", "INS01",
            "+48123456789", "jan@test.com", "Ul. Testowa 1"
        );

        var result = await service.CreateAsync(dto);

        Assert.Equal("Jan", result.FirstName);
        Assert.Equal("Kowalski", result.LastName);
        Assert.Equal("12345678901", result.Pesel);
        Assert.NotEqual(0, result.Id);

        var saved = await context.Patients.FindAsync(result.Id);
        Assert.NotNull(saved);
    }

    // ==========================================
    // UpdateAsync
    // ==========================================

    [Fact]
    public async Task UpdateAsync_ExistingPatient_UpdatesAndReturnsDto()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        context.Patients.Add(new Patient
        {
            Id = 1, FirstName = "Old", LastName = "Name", Pesel = "12345678901",
            InsuranceNumber = "I1", Phone = "+481", Email = "old@test.com", Address = "Old"
        });
        await context.SaveChangesAsync();

        var dto = new UpdatePatientRequestDto("New", "Name", "+482", "new@test.com", "New Address");
        var result = await service.UpdateAsync(1, dto);

        Assert.NotNull(result);
        Assert.Equal("New", result.FirstName);
        Assert.Equal("new@test.com", result.Email);
        Assert.Equal("+482", result.Phone);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingPatient_ReturnsNull()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        var dto = new UpdatePatientRequestDto("New", "Name", "+482", "new@test.com", "New Address");
        var result = await service.UpdateAsync(999, dto);

        Assert.Null(result);
    }

    // ==========================================
    // SoftDeleteAsync
    // ==========================================

    [Fact]
    public async Task SoftDeleteAsync_ExistingPatient_MarksAsDeleted()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        context.Patients.Add(new Patient
        {
            Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901",
            InsuranceNumber = "I1", Phone = "+481", Email = "j@t.com", Address = "Addr"
        });
        await context.SaveChangesAsync();

        var result = await service.SoftDeleteAsync(1);

        Assert.True(result);

        var deleted = await context.Patients.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == 1);
        Assert.NotNull(deleted);
        Assert.True(deleted.IsDeleted);
    }

    [Fact]
    public async Task SoftDeleteAsync_NonExistingPatient_ReturnsFalse()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        var result = await service.SoftDeleteAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task SoftDeleteAsync_AlreadyDeleted_ReturnsFalse()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        var service = CreateService(context);

        context.Patients.Add(new Patient
        {
            Id = 1, FirstName = "Jan", LastName = "Kowalski", Pesel = "12345678901",
            InsuranceNumber = "I1", Phone = "+481", Email = "j@t.com", Address = "Addr",
            IsDeleted = true
        });
        await context.SaveChangesAsync();

        var result = await service.SoftDeleteAsync(1);

        Assert.False(result);
    }


}
