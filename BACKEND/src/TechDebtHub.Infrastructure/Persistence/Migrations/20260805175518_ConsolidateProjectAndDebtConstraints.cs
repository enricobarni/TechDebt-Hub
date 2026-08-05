using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechDebtHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateProjectAndDebtConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DividasTecnicas_ProjetoId_Arquivada_Status",
                table: "DividasTecnicas",
                columns: new[] { "ProjetoId", "Arquivada", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DividasTecnicas_ProjetoId_Arquivada_Status",
                table: "DividasTecnicas");
        }
    }
}
