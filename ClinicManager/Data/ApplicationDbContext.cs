using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ClinicManager.Models;

namespace ClinicManager.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<MedicalProcedure> MedicalProcedures => Set<MedicalProcedure>();
    public DbSet<ProcedurePerformed> ProceduresPerformed => Set<ProcedurePerformed>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<PrescribedMedication> PrescribedMedications => Set<PrescribedMedication>();
    public DbSet<ClinicalNote> ClinicalNotes => Set<ClinicalNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Set explicit SQL decimal precision to avoid warnings ---
        modelBuilder.Entity<Visit>().Property(v => v.TotalCost).HasPrecision(18, 2);
        modelBuilder.Entity<MedicalProcedure>().Property(m => m.ServiceCost).HasPrecision(18, 2);
        modelBuilder.Entity<ProcedurePerformed>().Property(p => p.ActualCost).HasPrecision(18, 2);
        modelBuilder.Entity<Medication>().Property(m => m.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<PrescribedMedication>().Property(p => p.TotalCost).HasPrecision(18, 2);

        // --- Database Optimization Non-Clustered Indexes ---
        modelBuilder.Entity<Patient>()
            .HasIndex(p => p.Pesel)
            .IsUnique()
            .HasDatabaseName("IX_Patients_PESEL_Unique");

        modelBuilder.Entity<Visit>()
            .HasIndex(v => new { v.DoctorId, v.ScheduledDate })
            .HasDatabaseName("IX_Visits_DoctorId_ScheduledDate")
            .IncludeProperties(v => new { v.PatientId, v.Status, v.Reason });

        // --- Relationships & Mappings ---
        modelBuilder.Entity<Visit>()
            .HasOne(v => v.Doctor)
            .WithMany(u => u.DoctorVisits)
            .HasForeignKey(v => v.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Cascaded Global Query Filters (RODO Safety Compliance) ---
        // Automatically filters out records linked to soft-deleted patients
        modelBuilder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<MedicalRecord>().HasQueryFilter(r => !r.Patient!.IsDeleted);
        modelBuilder.Entity<Visit>().HasQueryFilter(v => !v.Patient!.IsDeleted);
        modelBuilder.Entity<ClinicalNote>().HasQueryFilter(n => !n.Visit.Patient!.IsDeleted);
        modelBuilder.Entity<PrescribedMedication>().HasQueryFilter(n => !n.Visit.Patient!.IsDeleted);
        modelBuilder.Entity<ProcedurePerformed>().HasQueryFilter(n => !n.Visit.Patient!.IsDeleted);
    }
}
