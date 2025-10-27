using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations.ValuationDb
{
    /// <inheritdoc />
    public partial class InitialValuations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ValuationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Period = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportingCurrency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Value_Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Value_Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    PortfolioId = table.Column<int>(type: "INTEGER", nullable: true),
                    AssetClass = table.Column<int>(type: "INTEGER", nullable: true),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    SecuritiesValue_Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    SecuritiesValue_Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    CashValue_Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    CashValue_Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true),
                    IncomeForDay_Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    IncomeForDay_Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValuationRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ValuationRecords");
        }
    }
}
