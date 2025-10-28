using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.ValuationDb
{
    /// <inheritdoc />
    public partial class FxRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FxRates",
                columns: table => new
                {
                    FromCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    ToCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Rate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FxRates", x => new { x.FromCurrency, x.ToCurrency, x.Date });
                });

            migrationBuilder.CreateIndex(
                name: "IX_FxRates_FromCurrency_ToCurrency",
                table: "FxRates",
                columns: new[] { "FromCurrency", "ToCurrency" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FxRates");
        }
    }
}
