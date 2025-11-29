using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trading_platform.Migrations
{
    /// <inheritdoc />
    public partial class RenamePriceColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceAtExcecution",
                table: "Orders",
                newName: "PriceAtExecution");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceAtExecution",
                table: "Orders",
                newName: "PriceAtExcecution");
        }
    }
}
