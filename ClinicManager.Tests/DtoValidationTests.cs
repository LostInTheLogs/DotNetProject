using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ClinicManager.DTOs;
using ClinicManager.Models;

namespace ClinicManager.Tests;

public class DtoValidationTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();
        var type = model.GetType();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var attrs = prop.GetCustomAttributes<ValidationAttribute>(inherit: true).ToList();

            if (attrs.Count == 0)
            {
                // Records put attributes on constructor parameters, not properties.
                var ctorParam = type.GetConstructors()
                    .SelectMany(c => c.GetParameters())
                    .FirstOrDefault(p =>
                        string.Equals(p.Name, prop.Name, StringComparison.OrdinalIgnoreCase));
                if (ctorParam != null)
                    attrs = ctorParam.GetCustomAttributes<ValidationAttribute>(inherit: true).ToList();
            }

            if (attrs.Count == 0) continue;

            var context = new ValidationContext(model) { MemberName = prop.Name };
            var value = prop.GetValue(model);

            foreach (var attr in attrs)
            {
                var result = attr.GetValidationResult(value, context);
                if (result != ValidationResult.Success)
                    results.Add(result!);
            }
        }

        return results;
    }

    // ==========================================
    // LoginDto (class)
    // ==========================================

    [Fact]
    public void LoginDto_Valid_Passes()
    {
        var dto = new LoginDto { Email = "user@clinic.com", Password = "ValidPass1!" };
        var errors = ValidateModel(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void LoginDto_MissingEmail_Fails()
    {
        var dto = new LoginDto { Email = "", Password = "ValidPass1!" };
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("Email"));
    }

    [Fact]
    public void LoginDto_InvalidEmail_Fails()
    {
        var dto = new LoginDto { Email = "not-an-email", Password = "ValidPass1!" };
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("Email"));
    }

    [Fact]
    public void LoginDto_MissingPassword_Fails()
    {
        var dto = new LoginDto { Email = "user@clinic.com", Password = "" };
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("Password"));
    }

    // ==========================================
    // RegisterDto (class)
    // ==========================================

    [Fact]
    public void RegisterDto_Valid_Passes()
    {
        var dto = new RegisterDto
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@clinic.com",
            Password = "ValidPass1!",
            ConfirmPassword = "ValidPass1!"
        };
        var errors = ValidateModel(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void RegisterDto_MissingFirstName_Fails()
    {
        var dto = new RegisterDto
        {
            FirstName = "",
            LastName = "Kowalski",
            Email = "jan@clinic.com",
            Password = "ValidPass1!",
            ConfirmPassword = "ValidPass1!"
        };
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("FirstName"));
    }

    [Fact]
    public void RegisterDto_PasswordMismatch_Fails()
    {
        var dto = new RegisterDto
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@clinic.com",
            Password = "ValidPass1!",
            ConfirmPassword = "DifferentPass1!"
        };
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("ConfirmPassword"));
    }

    [Fact]
    public void RegisterDto_ShortPassword_Fails()
    {
        var dto = new RegisterDto
        {
            FirstName = "Jan",
            LastName = "Kowalski",
            Email = "jan@clinic.com",
            Password = "Ab1!",
            ConfirmPassword = "Ab1!"
        };
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("Password"));
    }

    [Fact]
    public void RegisterDto_FirstNameExceedsMaxLength_Fails()
    {
        var dto = new RegisterDto
        {
            FirstName = new string('A', 101),
            LastName = "Kowalski",
            Email = "jan@clinic.com",
            Password = "ValidPass1!",
            ConfirmPassword = "ValidPass1!"
        };
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("FirstName"));
    }

    // ==========================================
    // CreateStaffDto (class)
    // ==========================================

    [Fact]
    public void CreateStaffDto_Valid_Passes()
    {
        var dto = new CreateStaffDto
        {
            FirstName = "Anna",
            LastName = "Nowak",
            Email = "anna@clinic.com",
            SelectedRole = "Doctor"
        };
        var errors = ValidateModel(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void CreateStaffDto_MissingRole_Fails()
    {
        var dto = new CreateStaffDto
        {
            FirstName = "Anna",
            LastName = "Nowak",
            Email = "anna@clinic.com",
            SelectedRole = ""
        };
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("SelectedRole"));
    }

    // ==========================================
    // CreatePatientRequestDto (record)
    // ==========================================

    [Fact]
    public void CreatePatientRequestDto_Valid_Passes()
    {
        var dto = new CreatePatientRequestDto(
            "Jan", "Kowalski", "12345678901", "INS001",
            "+48123456789", "jan@example.com", "Ul. Testowa 1"
        );
        var errors = ValidateModel(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void CreatePatientRequestDto_InvalidPeselLength_Fails()
    {
        var dto = new CreatePatientRequestDto(
            "Jan", "Kowalski", "12345", "INS001",
            "+48123456789", "jan@example.com", "Ul. Testowa 1"
        );
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("Pesel"));
    }

    [Fact]
    public void CreatePatientRequestDto_InvalidPhone_Fails()
    {
        var dto = new CreatePatientRequestDto(
            "Jan", "Kowalski", "12345678901", "INS001",
            "not-a-phone", "jan@example.com", "Ul. Testowa 1"
        );
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("Phone"));
    }

    [Fact]
    public void CreatePatientRequestDto_InvalidEmail_Fails()
    {
        var dto = new CreatePatientRequestDto(
            "Jan", "Kowalski", "12345678901", "INS001",
            "+48123456789", "not-an-email", "Ul. Testowa 1"
        );
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("Email"));
    }

    [Fact]
    public void CreatePatientRequestDto_EmptyFirstName_Fails()
    {
        var dto = new CreatePatientRequestDto(
            "", "Kowalski", "12345678901", "INS001",
            "+48123456789", "jan@example.com", "Ul. Testowa 1"
        );
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("FirstName"));
    }

    // ==========================================
    // UpdatePatientRequestDto (record)
    // ==========================================

    [Fact]
    public void UpdatePatientRequestDto_Valid_Passes()
    {
        var dto = new UpdatePatientRequestDto(
            "Jan", "Kowalski", "+48123456789", "jan@example.com", "Ul. Testowa 1"
        );
        var errors = ValidateModel(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void UpdatePatientRequestDto_InvalidPhone_Fails()
    {
        var dto = new UpdatePatientRequestDto(
            "Jan", "Kowalski", "bad-phone", "jan@example.com", "Ul. Testowa 1"
        );
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("Phone"));
    }

    // ==========================================
    // CreateVisitRequestDto (record)
    // ==========================================

    [Fact]
    public void CreateVisitRequestDto_Valid_Passes()
    {
        var dto = new CreateVisitRequestDto(1, "doctor-id", new DateTime(2026, 6, 10, 10, 0, 0), "Routine checkup");
        var errors = ValidateModel(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void CreateVisitRequestDto_EmptyReason_Fails()
    {
        var dto = new CreateVisitRequestDto(1, "doctor-id", DateTime.UtcNow, "");
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("Reason"));
    }

    [Fact]
    public void CreateVisitRequestDto_ReasonExceedsMaxLength_Fails()
    {
        var dto = new CreateVisitRequestDto(1, "doctor-id", DateTime.UtcNow, new string('X', 501));
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("Reason"));
    }

    // ==========================================
    // LogProcedurePerformedDto (record)
    // ==========================================

    [Fact]
    public void LogProcedurePerformedDto_Valid_Passes()
    {
        var dto = new LogProcedurePerformedDto(1, "Some notes");
        var errors = ValidateModel(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void LogProcedurePerformedDto_NotesExceedsMaxLength_Fails()
    {
        var dto = new LogProcedurePerformedDto(1, new string('X', 501));
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("Notes"));
    }

    // ==========================================
    // AddPrescribedMedicationDto (record)
    // ==========================================

    [Fact]
    public void AddPrescribedMedicationDto_Valid_Passes()
    {
        var dto = new AddPrescribedMedicationDto(1, "1 tablet 3x daily", 30);
        var errors = ValidateModel(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void AddPrescribedMedicationDto_EmptyDosage_Fails()
    {
        var dto = new AddPrescribedMedicationDto(1, "", 30);
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("Dosage"));
    }

    [Fact]
    public void AddPrescribedMedicationDto_QuantityOutOfRange_Fails()
    {
        var dto = new AddPrescribedMedicationDto(1, "1 tablet 3x daily", 0);
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("Quantity"));
    }

    // ==========================================
    // CreateClinicalNoteDto (record)
    // ==========================================

    [Fact]
    public void CreateClinicalNoteDto_Valid_Passes()
    {
        var dto = new CreateClinicalNoteDto(1, "Diagnosis", "Patient shows improvement.");
        var errors = ValidateModel(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void CreateClinicalNoteDto_EmptyNoteType_Fails()
    {
        var dto = new CreateClinicalNoteDto(1, "", "Content");
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("NoteType"));
    }

    [Fact]
    public void CreateClinicalNoteDto_EmptyContent_Fails()
    {
        var dto = new CreateClinicalNoteDto(1, "Diagnosis", "");
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("Content"));
    }

    // ==========================================
    // UploadMedicalRecordRequestDto (record)
    // ==========================================

    [Fact]
    public void UploadMedicalRecordRequestDto_Valid_Passes()
    {
        var dto = new UploadMedicalRecordRequestDto(1, "X-Ray", "Chest X-Ray description");
        var errors = ValidateModel(dto);
        Assert.Empty(errors);
    }

    [Fact]
    public void UploadMedicalRecordRequestDto_EmptyDocumentType_Fails()
    {
        var dto = new UploadMedicalRecordRequestDto(1, "", "Description");
        var errors = ValidateModel(dto);
        Assert.Contains(errors, e => e.MemberNames.Contains("DocumentType"));
    }
}
