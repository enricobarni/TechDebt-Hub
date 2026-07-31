using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechDebtHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedTechnicalDebtTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DividasTecnicas_ProjetoId",
                table: "DividasTecnicas");

            migrationBuilder.AddColumn<string>(
                name: "TituloNormalizado",
                table: "DividasTecnicas",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DividasTecnicas_ProjetoId_TituloNormalizado",
                table: "DividasTecnicas",
                columns: new[] { "ProjetoId", "TituloNormalizado" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DividasTecnicas_ProjetoId_TituloNormalizado",
                table: "DividasTecnicas");

            migrationBuilder.DropColumn(
                name: "TituloNormalizado",
                table: "DividasTecnicas");

            migrationBuilder.CreateIndex(
                name: "IX_DividasTecnicas_ProjetoId",
                table: "DividasTecnicas",
                column: "ProjetoId");
        }
    }
}
