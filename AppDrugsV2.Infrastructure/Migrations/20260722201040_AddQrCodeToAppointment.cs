using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppDrugsV2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQrCodeToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QrCodeBase64",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QrCodeBase64",
                table: "Appointments");
        }
    }
}
