using Microsoft.AspNetCore.Identity;
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

        await context.SaveChangesAsync();
    }
}
