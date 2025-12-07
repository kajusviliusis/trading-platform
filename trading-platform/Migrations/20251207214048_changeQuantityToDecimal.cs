using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace trading_platform.Migrations
{
    /// <inheritdoc />
    public partial class changeQuantityToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Transactions",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Orders",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Holdings",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Holdings_StockId",
                table: "Holdings",
                column: "StockId");

            migrationBuilder.AddForeignKey(
                name: "FK_Holdings_Stocks_StockId",
                table: "Holdings",
                column: "StockId",
                principalTable: "Stocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Holdings_Stocks_StockId",
                table: "Holdings");

            migrationBuilder.DropIndex(
                name: "IX_Holdings_StockId",
                table: "Holdings");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Transactions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Orders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Holdings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");
        }
    }
}
