using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Clinic.Repo.Migrations
{
    /// <inheritdoc />
    public partial class AddingNewTableAndRemovePatientTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Patients",
                table: "Patients");

            migrationBuilder.RenameTable(
                name: "Patients",
                newName: "Patient");

            migrationBuilder.AddColumn<int>(
                name: "AppointmentArchiveId",
                table: "Notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Patient",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Patient",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Patient",
                table: "Patient",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AppointmentArchives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QueueNumber = table.Column<int>(type: "integer", nullable: false),
                    EstimatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppointmentType = table.Column<int>(type: "integer", nullable: false),
                    PatientName = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentArchives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentArchives_Patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AppointmentArchiveId",
                table: "Notifications",
                column: "AppointmentArchiveId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentArchives_PatientId",
                table: "AppointmentArchives",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patient_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AppointmentArchives_AppointmentArchiveId",
                table: "Notifications",
                column: "AppointmentArchiveId",
                principalTable: "AppointmentArchives",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patient_PatientId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AppointmentArchives_AppointmentArchiveId",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "AppointmentArchives");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_AppointmentArchiveId",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Patient",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "AppointmentArchiveId",
                table: "Notifications");

            migrationBuilder.RenameTable(
                name: "Patient",
                newName: "Patients");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Patients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Patients",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Patients",
                table: "Patients",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientId",
                table: "Appointments",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
