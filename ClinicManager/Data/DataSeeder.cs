using Microsoft.AspNetCore.Identity;
using Bogus;
using ClinicManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManager.Data;

public static class DataSeeder
{
    public static async Task SeedDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // 1. Seed Roles
        string[] roles = { "Admin", "Doctor", "Receptionist" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Default Staff Accounts
        string adminEmail = "admin@clinic.com";

        if (!await userManager.Users.AnyAsync(u => u.Email == adminEmail))
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "System",
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(admin, "Admin@2026!");
            if (!createResult.Succeeded)
                throw new Exception($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (!await userManager.Users.AnyAsync(u => u.Email == "dr.kowalski@clinic.com"))
        {
            var doctor = new ApplicationUser
            {
                UserName = "dr.kowalski@clinic.com",
                Email = "dr.kowalski@clinic.com",
                FirstName = "Jan",
                LastName = "Kowalski",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(doctor, "ClinicSecure2026!");
            await userManager.AddToRoleAsync(doctor, "Doctor");
        }

        if (!await userManager.Users.AnyAsync(u => u.Email == "anna.nowak@clinic.com"))
        {
            var receptionist = new ApplicationUser
            {
                UserName = "anna.nowak@clinic.com",
                Email = "anna.nowak@clinic.com",
                FirstName = "Anna",
                LastName = "Nowak",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(receptionist, "ClinicSecure2026!");
            await userManager.AddToRoleAsync(receptionist, "Receptionist");
        }

        // 3. Seed Lookup Medical Procedures Catalog
        if (!await context.MedicalProcedures.AnyAsync())
        {
            context.MedicalProcedures.AddRange(
                new MedicalProcedure { Name = "Standard Medical Consultation", Description = "Basic general examination with medical history interview.", ServiceCost = 150.00m },
                new MedicalProcedure { Name = "Abdominal Ultrasound", Description = "Ultrasound examination of internal organs.", ServiceCost = 220.00m },
                new MedicalProcedure { Name = "Resting ECG", Description = "Electrocardiogram with printout and description.", ServiceCost = 80.00m }
            );
        }

        // 4. Seed Lookup Medications Catalog
        if (!await context.Medications.AnyAsync())
        {
            context.Medications.AddRange(
                new Medication { Name = "Amotaks 500mg", Description = "Broad-spectrum antibiotic (Amoxicillin).", UnitPrice = 34.20m, IsAvailable = true },
                new Medication { Name = "Paracetamol Accord", Description = "Pain reliever and antipyretic.", UnitPrice = 12.50m, IsAvailable = true },
                new Medication { Name = "Xarelto 20mg", Description = "Novel oral anticoagulant.", UnitPrice = 139.90m, IsAvailable = true }
            );
        }

        // Save catalog structures first to make sure everything below runs on valid baseline entities
        await context.SaveChangesAsync();

        // =========================================================================
        // 🚀 5. BOGUS DYNAMIC DATA SEEDING ENGINE (FOR PERFORMANCE LOAD TESTING)
        // =========================================================================
        
        // Only run if the database lacks patients and active load profiles
        if (!await context.Patients.AnyAsync())
        {
            // Deterministic random seed so data remains completely consistent on identical builds
            Randomizer.Seed = new Random(2026);

            // Fetch doctor IDs mapped through Identity Roles
            var doctorsInSystem = await userManager.GetUsersInRoleAsync("Doctor");
            var doctorIds = doctorsInSystem.Select(d => d.Id).ToList();

            if (!doctorIds.Any())
            {
                // Fallback catch boundary in case role resolution context is delayed at startup
                var defaultDoc = await userManager.FindByEmailAsync("dr.kowalski@clinic.com");
                if (defaultDoc != null) doctorIds.Add(defaultDoc.Id);
            }

            // A. Generate Mock Patients via Bogus Rules
            var patientFaker = new Faker<Patient>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName())
                .RuleFor(p => p.Email, f => f.Internet.Email())
                .RuleFor(p => p.Pesel, f => f.Random.ReplaceNumbers("###########"));

            var generatedPatients = patientFaker.Generate(100);
            context.Patients.AddRange(generatedPatients);
            
            // Persist patients so EF Core generates valid incremental primary Keys
            await context.SaveChangesAsync();

            // B. Generate Relational Mock Visits via Bogus Rules
            var clinicalReasons = new[] 
            { 
                "Routine chronic disease maintenance and follow-up consultation.",
                "Patient presenting with persistent cardiovascular palpitations.",
                "Acute abdominal pain review and diagnostic screening evaluation.",
                "Prescription extension and minor therapeutic symptoms overview."
            };

            var appointmentStatuses = new[] { VisitStatus.Scheduled, VisitStatus.InProgress };

            var visitFaker = new Faker<Visit>()
                .RuleFor(v => v.PatientId, f => f.PickRandom(generatedPatients).Id)
                .RuleFor(v => v.DoctorId, f => f.PickRandom(doctorIds))
                .RuleFor(v => v.ScheduledDate, f => f.Date.Between(DateTime.Now.AddDays(-2), DateTime.Now.AddDays(12)))
                .RuleFor(v => v.Status, f => f.PickRandom(appointmentStatuses))
                .RuleFor(v => v.Reason, f => f.PickRandom(clinicalReasons))
                .RuleFor(v => v.TotalCost, f => f.Finance.Amount(100, 350, 2))
                .RuleFor(v => v.CreatedAt, f => DateTime.UtcNow);

            var generatedVisits = visitFaker.Generate(300);
            context.Visits.AddRange(generatedVisits);
            
            await context.SaveChangesAsync();
        }
    }
}
