using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManager.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_DoctorId",
                table: "Visits");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_DoctorId_ScheduledDate",
                table: "Visits",
                columns: new[] { "DoctorId", "ScheduledDate" })
                .Annotation("SqlServer:Include", new[] { "PatientId", "Status", "Reason" });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PESEL_Unique",
                table: "Patients",
                column: "Pesel",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_DoctorId_ScheduledDate",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Patients_PESEL_Unique",
                table: "Patients");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_DoctorId",
                table: "Visits",
                column: "DoctorId");
        }
    }
}
