using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task11_DotNETBackendWebApi.Data.Migrations;

/// <inheritdoc />
public partial class AddIsIncomeToFinancialType : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IS_INCOME",
            table: "FINANCIAL_TYPES",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IS_INCOME",
            table: "FINANCIAL_TYPES");
    }
}
