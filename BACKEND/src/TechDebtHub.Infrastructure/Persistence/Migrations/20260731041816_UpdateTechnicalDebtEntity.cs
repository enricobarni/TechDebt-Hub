using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechDebtHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTechnicalDebtEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DividasTecnicas_Projetos_Id",
                table: "DividasTecnicas");

            migrationBuilder.CreateIndex(
                name: "IX_DividasTecnicas_ProjetoId",
                table: "DividasTecnicas",
                column: "ProjetoId");

            migrationBuilder.AddForeignKey(
                name: "FK_DividasTecnicas_Projetos_ProjetoId",
                table: "DividasTecnicas",
                column: "ProjetoId",
                principalTable: "Projetos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DividasTecnicas_Projetos_ProjetoId",
                table: "DividasTecnicas");

            migrationBuilder.DropIndex(
                name: "IX_DividasTecnicas_ProjetoId",
                table: "DividasTecnicas");

            migrationBuilder.AddForeignKey(
                name: "FK_DividasTecnicas_Projetos_Id",
                table: "DividasTecnicas",
                column: "Id",
                principalTable: "Projetos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
