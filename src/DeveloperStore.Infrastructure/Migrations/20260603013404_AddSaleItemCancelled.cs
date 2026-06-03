using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeveloperStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleItemCancelled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "SaleItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "SaleItems");
        }
    }
}
