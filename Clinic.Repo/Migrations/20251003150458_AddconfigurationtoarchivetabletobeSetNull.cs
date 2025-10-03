using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic.Repo.Migrations
{
    /// <inheritdoc />
    public partial class AddconfigurationtoarchivetabletobeSetNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentArchives_Appointments_AppointmentId",
                table: "AppointmentArchives");

            migrationBuilder.AlterColumn<int>(
                name: "AppointmentId",
                table: "AppointmentArchives",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentArchives_Appointments_AppointmentId",
                table: "AppointmentArchives",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentArchives_Appointments_AppointmentId",
                table: "AppointmentArchives");

            migrationBuilder.AlterColumn<int>(
                name: "AppointmentId",
                table: "AppointmentArchives",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentArchives_Appointments_AppointmentId",
                table: "AppointmentArchives",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
