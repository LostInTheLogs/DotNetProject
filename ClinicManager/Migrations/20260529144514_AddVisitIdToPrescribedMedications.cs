using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManager.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitIdToPrescribedMedications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrescribedMedications_ProceduresPerformed_ProcedurePerformedId",
                table: "PrescribedMedications");

            migrationBuilder.AlterColumn<int>(
                name: "ProcedurePerformedId",
                table: "PrescribedMedications",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "VisitId",
                table: "PrescribedMedications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PrescribedMedications_VisitId",
                table: "PrescribedMedications",
                column: "VisitId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrescribedMedications_ProceduresPerformed_ProcedurePerformedId",
                table: "PrescribedMedications",
                column: "ProcedurePerformedId",
                principalTable: "ProceduresPerformed",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PrescribedMedications_Visits_VisitId",
                table: "PrescribedMedications",
                column: "VisitId",
                principalTable: "Visits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrescribedMedications_ProceduresPerformed_ProcedurePerformedId",
                table: "PrescribedMedications");

            migrationBuilder.DropForeignKey(
                name: "FK_PrescribedMedications_Visits_VisitId",
                table: "PrescribedMedications");

            migrationBuilder.DropIndex(
                name: "IX_PrescribedMedications_VisitId",
                table: "PrescribedMedications");

            migrationBuilder.DropColumn(
                name: "VisitId",
                table: "PrescribedMedications");

            migrationBuilder.AlterColumn<int>(
                name: "ProcedurePerformedId",
                table: "PrescribedMedications",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PrescribedMedications_ProceduresPerformed_ProcedurePerformedId",
                table: "PrescribedMedications",
                column: "ProcedurePerformedId",
                principalTable: "ProceduresPerformed",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
