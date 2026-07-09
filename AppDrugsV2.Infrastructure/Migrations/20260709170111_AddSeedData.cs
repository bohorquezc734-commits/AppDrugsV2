using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppDrugsV2.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insertar usuario admin
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Email", "PasswordHash", "FullName", "Role", "IsActive", "CreatedAt" },
                values: new object[] {
            "admin@test.com",
           "$2a$12$db12JEgN9pc2XFxiz1ov6.odY3uhguo.S85MMIOwkYNIOmy2q8zZ.",
            "Administrador del Sistema",
            "Admin",
            true,
            DateTime.UtcNow
                }
            );

            // Insertar medicamentos
            migrationBuilder.InsertData(
                table: "Drugs",
                columns: new[] { "Name", "GenericName", "Laboratory", "Price", "Stock", "Category", "RequiresPrescription", "ExpirationDate", "IsActive", "CreatedAt" },
                values: new object[,]
                {
            { "Paracetamol", "Acetaminofén", "Bayer", 15.50m, 100, "Analgésico", false, DateTime.UtcNow.AddMonths(12), true, DateTime.UtcNow },
            { "Ibuprofeno", "Ibuprofeno", "Pfizer", 25.30m, 150, "Antiinflamatorio", false, DateTime.UtcNow.AddMonths(18), true, DateTime.UtcNow },
            { "Amoxicilina", "Amoxicilina", "Sandoz", 45.80m, 80, "Antibiótico", true, DateTime.UtcNow.AddMonths(8), true, DateTime.UtcNow }
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
