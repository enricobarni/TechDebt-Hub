using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechDebtHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdjustCodigoConfirmacaoEmailIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TokensConfirmaçãoEmail_CodigoHash",
                table: "TokensConfirmaçãoEmail");

            migrationBuilder.DropIndex(
                name: "IX_TokensConfirmaçãoEmail_UsuarioId",
                table: "TokensConfirmaçãoEmail");

            migrationBuilder.CreateIndex(
                name: "IX_TokensConfirmaçãoEmail_UsuarioId_CodigoHash",
                table: "TokensConfirmaçãoEmail",
                columns: new[] { "UsuarioId", "CodigoHash" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TokensConfirmaçãoEmail_UsuarioId_CodigoHash",
                table: "TokensConfirmaçãoEmail");

            migrationBuilder.CreateIndex(
                name: "IX_TokensConfirmaçãoEmail_CodigoHash",
                table: "TokensConfirmaçãoEmail",
                column: "CodigoHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokensConfirmaçãoEmail_UsuarioId",
                table: "TokensConfirmaçãoEmail",
                column: "UsuarioId");
        }
    }
}
