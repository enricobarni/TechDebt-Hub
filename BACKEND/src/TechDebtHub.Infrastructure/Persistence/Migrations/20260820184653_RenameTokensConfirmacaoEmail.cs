using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechDebtHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameTokensConfirmacaoEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokensConfirmaçãoEmail_Usuarios_UsuarioId",
                table: "TokensConfirmaçãoEmail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TokensConfirmaçãoEmail",
                table: "TokensConfirmaçãoEmail");

            migrationBuilder.RenameTable(
                name: "TokensConfirmaçãoEmail",
                newName: "CodigosConfirmaçãoEmail");

            migrationBuilder.RenameIndex(
                name: "IX_TokensConfirmaçãoEmail_UsuarioId_CodigoHash",
                table: "CodigosConfirmaçãoEmail",
                newName: "IX_CodigosConfirmaçãoEmail_UsuarioId_CodigoHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CodigosConfirmaçãoEmail",
                table: "CodigosConfirmaçãoEmail",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CodigosConfirmaçãoEmail_Usuarios_UsuarioId",
                table: "CodigosConfirmaçãoEmail",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CodigosConfirmaçãoEmail_Usuarios_UsuarioId",
                table: "CodigosConfirmaçãoEmail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CodigosConfirmaçãoEmail",
                table: "CodigosConfirmaçãoEmail");

            migrationBuilder.RenameTable(
                name: "CodigosConfirmaçãoEmail",
                newName: "TokensConfirmaçãoEmail");

            migrationBuilder.RenameIndex(
                name: "IX_CodigosConfirmaçãoEmail_UsuarioId_CodigoHash",
                table: "TokensConfirmaçãoEmail",
                newName: "IX_TokensConfirmaçãoEmail_UsuarioId_CodigoHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TokensConfirmaçãoEmail",
                table: "TokensConfirmaçãoEmail",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TokensConfirmaçãoEmail_Usuarios_UsuarioId",
                table: "TokensConfirmaçãoEmail",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
