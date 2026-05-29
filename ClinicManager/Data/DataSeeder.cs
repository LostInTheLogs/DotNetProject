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
        if (!await userManager.Users.AnyAsync())
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
                new MedicalProcedure { Name = "Standardowa konsultacja lekarska", Description = "Podstawowe badanie ogólne wraz z wywiadem chorobowym.", ServiceCost = 150.00m },
                new MedicalProcedure { Name = "USG Jamy Brzusznej", Description = "Badanie ultrasonograficzne narządów wewnętrznych.", ServiceCost = 220.00m },
                new MedicalProcedure { Name = "EKG spoczynkowe", Description = "Badanie elektrokardiograficzne z wydrukiem i opisem.", ServiceCost = 80.00m }
            );
        }

        // 4. Seed Lookup Medications Catalog
        if (!await context.Medications.AnyAsync())
        {
            context.Medications.AddRange(
                new Medication { Name = "Amotaks 500mg", Description = "Antybiotyk o szerokim spektrum (Amoxicillinum).", UnitPrice = 34.20m, IsAvailable = true },
                new Medication { Name = "Paracetamol Accord", Description = "Lek przeciwbólowy i przeciwgorączkowy.", UnitPrice = 12.50m, IsAvailable = true },
                new Medication { Name = "Xarelto 20mg", Description = "Doustny lek przeciwzakrzepowy nowej generacji.", UnitPrice = 139.90m, IsAvailable = true }
            );
        }

        await context.SaveChangesAsync();
    }
}
