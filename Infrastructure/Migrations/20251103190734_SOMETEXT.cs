using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SOMETEXT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Symbol_Value",
                table: "Transactions",
                newName: "Symbol_Code");

            migrationBuilder.RenameColumn(
                name: "Symbol_Value",
                table: "Holdings",
                newName: "Symbol_Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Symbol_Code",
                table: "Transactions",
                newName: "Symbol_Value");

            migrationBuilder.RenameColumn(
                name: "Symbol_Code",
                table: "Holdings",
                newName: "Symbol_Value");
        }
    }
}
