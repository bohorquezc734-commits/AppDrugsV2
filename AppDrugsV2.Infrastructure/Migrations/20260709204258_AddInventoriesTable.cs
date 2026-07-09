using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppDrugsV2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    GestorFarmaceuticoId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventories_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inventories_GestoresFarmaceuticos_GestorFarmaceuticoId",
                        column: x => x.GestorFarmaceuticoId,
                        principalTable: "GestoresFarmaceuticos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_DrugId_GestorFarmaceuticoId",
                table: "Inventories",
                columns: new[] { "DrugId", "GestorFarmaceuticoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_GestorFarmaceuticoId",
                table: "Inventories",
                column: "GestorFarmaceuticoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventories");
        }
    }
}
