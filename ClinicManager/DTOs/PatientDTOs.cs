using System.ComponentModel.DataAnnotations;

namespace ClinicManager.DTOs;

public record PatientResponseDto(
    int Id,
    string FirstName,
    string LastName,
    string Pesel,
    string InsuranceNumber,
    string Phone,
    string Email,
    string Address,
    DateTime CreatedAt
);

public record CreatePatientRequestDto(
    [Required][MaxLength(50)] string FirstName,
    [Required][MaxLength(50)] string LastName,
    [Required][StringLength(11, MinimumLength = 11)] string Pesel,
    [Required][MaxLength(30)] string InsuranceNumber,
    [Required][Phone][MaxLength(20)] string Phone,
    [Required][EmailAddress][MaxLength(100)] string Email,
    [Required][MaxLength(200)] string Address
);

public record UpdatePatientRequestDto(
    [Required][MaxLength(50)] string FirstName,
    [Required][MaxLength(50)] string LastName,
    [Required][Phone][MaxLength(20)] string Phone,
    [Required][EmailAddress][MaxLength(100)] string Email,
    [Required][MaxLength(200)] string Address
);
