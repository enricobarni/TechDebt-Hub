using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechDebtHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeparateDebtArchiveFromStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Arquivada",
                table: "DividasTecnicas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Arquivada",
                table: "DividasTecnicas");
        }
    }
}
