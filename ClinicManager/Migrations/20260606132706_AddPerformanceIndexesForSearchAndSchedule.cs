using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManager.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexesForSearchAndSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_Doctor_Schedule",
                table: "Visits");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_DoctorId_ScheduledDate",
                table: "Visits",
                columns: new[] { "DoctorId", "ScheduledDate" })
                .Annotation("SqlServer:Include", new[] { "PatientId", "Status", "Reason" });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_LastName_FirstName_CreatedAt",
                table: "Patients",
                columns: new[] { "LastName", "FirstName", "CreatedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Pesel_CreatedAt",
                table: "Patients",
                columns: new[] { "Pesel", "CreatedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_DoctorId_ScheduledDate",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Patients_LastName_FirstName_CreatedAt",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_Pesel_CreatedAt",
                table: "Patients");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_Doctor_Schedule",
                table: "Visits",
                columns: new[] { "DoctorId", "ScheduledDate" });
        }
    }
}
