using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ClinicManager.Data;
using ClinicManager.DTOs;
using ClinicManager.Mappers;
using ClinicManager.Models;

namespace ClinicManager.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly ApplicationDbContext _context;
    private readonly ClinicMapper _mapper;
    private readonly IWebHostEnvironment _environment;

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };

    public MedicalRecordService(
        ApplicationDbContext context,
        ClinicMapper mapper,
        IWebHostEnvironment environment)
    {
        _context = context;
        _mapper = mapper;
        _environment = environment;
    }

    public async Task<MedicalRecordResponseDto> UploadRecordAsync(UploadMedicalRecordRequestDto dto, IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file selected.");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("File too big (5MB).");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Invalid format, only images and documents allowed (JPG, PNG, PDF).");

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        var medicalRecord = new MedicalRecord
        {
            PatientId = dto.PatientId,
            DocumentType = dto.DocumentType,
            Description = dto.Description ?? string.Empty,
            DocumentScanUrl = $"/uploads/{uniqueFileName}",
            UploadedAt = DateTime.UtcNow
        };

        _context.MedicalRecords.Add(medicalRecord);
        await _context.SaveChangesAsync();

        return _mapper.RecordToResponseDto(medicalRecord);
    }

    public async Task<MedicalRecordResponseDto?> GetByIdAsync(int id)
    {
        var record = await _context.MedicalRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        return record == null ? null : _mapper.RecordToResponseDto(record);
    }

    public async Task<IEnumerable<MedicalRecordResponseDto>> GetByPatientIdAsync(int patientId)
    {
        var records = await _context.MedicalRecords
            .AsNoTracking()
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.UploadedAt)
            .ToListAsync();

        return records.Select(_mapper.RecordToResponseDto);
    }

    public async Task<bool> DeleteRecordAsync(int id)
    {
        var record = await _context.MedicalRecords.FindAsync(id);
        if (record == null) return false;

        var relativePath = record.DocumentScanUrl.TrimStart('/');
        var physicalPath = Path.Combine(_environment.WebRootPath, relativePath);

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        _context.MedicalRecords.Remove(record);
        await _context.SaveChangesAsync();
        return true;
    }
}
