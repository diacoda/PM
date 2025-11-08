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
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Rate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FxRates", x => new { x.FromCurrency, x.ToCurrency, x.Date });
                });

            migrationBuilder.CreateTable(
                name: "Prices",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prices", x => new { x.Symbol, x.Date });
                });

            migrationBuilder.CreateTable(
                name: "ValuationSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Period = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportingCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Value_Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Value_Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    PortfolioId = table.Column<int>(type: "INTEGER", nullable: true),
                    AssetClass = table.Column<int>(type: "INTEGER", nullable: true),
                    Percentage = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    SecuritiesValue_Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: true),
                    SecuritiesValue_Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    CashValue_Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: true),
                    CashValue_Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    IncomeForDay_Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: true),
                    IncomeForDay_Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValuationSnapshots", x => x.Id);
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

            migrationBuilder.DropTable(
                name: "Prices");

            migrationBuilder.DropTable(
                name: "ValuationSnapshots");
        }
    }
}
