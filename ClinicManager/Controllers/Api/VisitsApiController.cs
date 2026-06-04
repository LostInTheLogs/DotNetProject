using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicManager.Data;
using ClinicManager.Models;

namespace ClinicManager.Controllers.Api;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class VisitsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public VisitsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Fetch all active clinical appointments
    /// </summary>
    /// <remarks>
    /// Scans the database for Scheduled or In-Progress visits. 
    /// </remarks>
    /// <param name="limit">Caps payload collection size to guard memory allocation limits (Max: 500).</param>
    [HttpGet("visits/active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveVisits([FromQuery] int limit = 100)
    {
        if (limit > 500) limit = 500;

        var activeVisits = await _context.Visits
            .Include(v => v.Patient)
            .Include(v => v.Doctor)
            .AsNoTracking() // Maximizes database throughput for high-concurrency performance scripts
            .Where(v => v.Status == VisitStatus.Scheduled || v.Status == VisitStatus.InProgress)
            .OrderBy(v => v.ScheduledDate)
            .Take(limit)
            .Select(v => new
            {
                VisitId = v.Id,
                ScheduledTime = v.ScheduledDate,
                Status = v.Status.ToString(),
                Reason = v.Reason,
                Patient = v.Patient != null ? new
                {
                    v.Patient.Id,
                    FullName = $"{v.Patient.LastName}, {v.Patient.FirstName}",
                    v.Patient.Pesel
                } : null,
                Doctor = v.Doctor != null ? new
                {
                    v.Doctor.Id,
                    FullName = $"Dr. {v.Doctor.FirstName} {v.Doctor.LastName}"
                } : null
            })
            .ToListAsync();

        return Ok(activeVisits);
    }

    /// <summary>
    /// Search charts across the patient registry
    /// </summary>
    /// <remarks>
    /// Queries by applying text string filters across First Name, Last Name, or PESEL numbers.
    /// </remarks>
    /// <param name="query">The alphanumeric keyword criteria string used for matching lookups.</param>
    [HttpGet("patients/search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchPatients([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { error = "Search query parameter cannot be empty." });
        }

        var lowerQuery = query.ToLower().Trim();

        var patients = await _context.Patients
            .AsNoTracking()
            .Where(p => p.FirstName.ToLower().Contains(lowerQuery) || 
                        p.LastName.ToLower().Contains(lowerQuery) || 
                        p.Pesel.Contains(lowerQuery))
            .Take(50)
            .Select(p => new
            {
                p.Id,
                p.FirstName,
                p.LastName,
                p.Pesel,
                p.Email
            })
            .ToListAsync();

        return Ok(patients);
    }
}
