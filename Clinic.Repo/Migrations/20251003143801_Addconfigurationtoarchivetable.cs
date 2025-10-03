using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic.Repo.Migrations
{
    /// <inheritdoc />
    public partial class Addconfigurationtoarchivetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentArchives_Appointments_AppointmentId",
                table: "AppointmentArchives");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentArchives_AppointmentId",
                table: "AppointmentArchives");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentArchives_AppointmentId",
                table: "AppointmentArchives",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentArchives_Appointments_AppointmentId",
                table: "AppointmentArchives",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentArchives_Appointments_AppointmentId",
                table: "AppointmentArchives");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentArchives_AppointmentId",
                table: "AppointmentArchives");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentArchives_AppointmentId",
                table: "AppointmentArchives",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentArchives_Appointments_AppointmentId",
                table: "AppointmentArchives",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
