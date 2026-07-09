using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppDrugsV2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGestoresFarmaceuticos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GestoresFarmaceuticos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreSede = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdEps = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GestoresFarmaceuticos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GestoresFarmaceuticos_NombreSede",
                table: "GestoresFarmaceuticos",
                column: "NombreSede",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GestoresFarmaceuticos");
        }
    }
}
