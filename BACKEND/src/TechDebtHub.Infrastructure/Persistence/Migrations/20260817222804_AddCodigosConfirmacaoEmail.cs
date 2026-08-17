using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechDebtHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCodigosConfirmacaoEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TokensConfirmaçãoEmail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodigoHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataExpiracao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataUtilizacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataRevogacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TentativasFalhas = table.Column<int>(type: "int", nullable: false),
                    MaximoTentativas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokensConfirmaçãoEmail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokensConfirmaçãoEmail_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokensConfirmaçãoEmail");
        }
    }
}
