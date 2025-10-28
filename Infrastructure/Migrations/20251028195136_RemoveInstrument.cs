using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInstrument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Holdings_HoldingId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_HoldingId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "Instrument_Name",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "HoldingId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "Instrument_Name",
                table: "Holdings");

            migrationBuilder.RenameColumn(
                name: "Instrument_Symbol_Value",
                table: "Transactions",
                newName: "Symbol_Value");

            migrationBuilder.RenameColumn(
                name: "Instrument_AssetClass",
                table: "Transactions",
                newName: "Symbol_AssetClass");

            migrationBuilder.RenameColumn(
                name: "Instrument_Symbol_Value",
                table: "Holdings",
                newName: "Symbol_Value");

            migrationBuilder.RenameColumn(
                name: "Instrument_AssetClass",
                table: "Holdings",
                newName: "Symbol_AssetClass");

            migrationBuilder.AddColumn<string>(
                name: "Symbol_Currency",
                table: "Transactions",
                type: "TEXT",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Symbol_Exchange",
                table: "Transactions",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Symbol_Currency",
                table: "Holdings",
                type: "TEXT",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Symbol_Exchange",
                table: "Holdings",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Symbol_Currency",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Symbol_Exchange",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Symbol_Currency",
                table: "Holdings");

            migrationBuilder.DropColumn(
                name: "Symbol_Exchange",
                table: "Holdings");

            migrationBuilder.RenameColumn(
                name: "Symbol_Value",
                table: "Transactions",
                newName: "Instrument_Symbol_Value");

            migrationBuilder.RenameColumn(
                name: "Symbol_AssetClass",
                table: "Transactions",
                newName: "Instrument_AssetClass");

            migrationBuilder.RenameColumn(
                name: "Symbol_Value",
                table: "Holdings",
                newName: "Instrument_Symbol_Value");

            migrationBuilder.RenameColumn(
                name: "Symbol_AssetClass",
                table: "Holdings",
                newName: "Instrument_AssetClass");

            migrationBuilder.AddColumn<string>(
                name: "Instrument_Name",
                table: "Transactions",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "HoldingId",
                table: "Tags",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instrument_Name",
                table: "Holdings",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_HoldingId",
                table: "Tags",
                column: "HoldingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Holdings_HoldingId",
                table: "Tags",
                column: "HoldingId",
                principalTable: "Holdings",
                principalColumn: "Id");
        }
    }
}
