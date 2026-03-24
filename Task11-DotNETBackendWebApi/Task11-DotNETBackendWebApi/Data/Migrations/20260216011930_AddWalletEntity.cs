using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task11_DotNETBackendWebApi.Data.Migrations;

/// <inheritdoc />
public partial class AddWalletEntity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<bool>(
            name: "IS_DELETED",
            table: "USERS",
            type: "bit",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "bit");

        migrationBuilder.AlterColumn<bool>(
            name: "IS_INCOME",
            table: "FINANCIAL_TYPES",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IS_DELETED",
            table: "FINANCIAL_TYPES",
            type: "bit",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "bit");

        migrationBuilder.AlterColumn<bool>(
            name: "IS_DELETED",
            table: "FINANCIAL_OPERATIONS",
            type: "bit",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "bit");

        migrationBuilder.AddColumn<string>(
            name: "CURRENT_CURRENCY",
            table: "FINANCIAL_OPERATIONS",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "TRANSACTION_COMMENT",
            table: "FINANCIAL_OPERATIONS",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<Guid>(
            name: "WALLET_ID",
            table: "FINANCIAL_OPERATIONS",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.CreateTable(
            name: "WALLETS",
            columns: table => new
            {
                ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                NAME = table.Column<string>(type: "nvarchar(max)", nullable: false),
                BALANCE = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                BASE_CURRENCY = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IS_DELETED = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                USER_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WALLETS", x => x.ID);
                table.ForeignKey(
                    name: "FK_WALLETS_USERS_USER_ID",
                    column: x => x.USER_ID,
                    principalTable: "USERS",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_FINANCIAL_OPERATIONS_WALLET_ID",
            table: "FINANCIAL_OPERATIONS",
            column: "WALLET_ID");

        migrationBuilder.CreateIndex(
            name: "IX_WALLETS_USER_ID",
            table: "WALLETS",
            column: "USER_ID");

        migrationBuilder.AddForeignKey(
            name: "FK_FINANCIAL_OPERATIONS_WALLETS_WALLET_ID",
            table: "FINANCIAL_OPERATIONS",
            column: "WALLET_ID",
            principalTable: "WALLETS",
            principalColumn: "ID",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_FINANCIAL_OPERATIONS_WALLETS_WALLET_ID",
            table: "FINANCIAL_OPERATIONS");

        migrationBuilder.DropTable(
            name: "WALLETS");

        migrationBuilder.DropIndex(
            name: "IX_FINANCIAL_OPERATIONS_WALLET_ID",
            table: "FINANCIAL_OPERATIONS");

        migrationBuilder.DropColumn(
            name: "CURRENT_CURRENCY",
            table: "FINANCIAL_OPERATIONS");

        migrationBuilder.DropColumn(
            name: "TRANSACTION_COMMENT",
            table: "FINANCIAL_OPERATIONS");

        migrationBuilder.DropColumn(
            name: "WALLET_ID",
            table: "FINANCIAL_OPERATIONS");

        migrationBuilder.AlterColumn<bool>(
            name: "IS_DELETED",
            table: "USERS",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IS_INCOME",
            table: "FINANCIAL_TYPES",
            type: "bit",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "bit");

        migrationBuilder.AlterColumn<bool>(
            name: "IS_DELETED",
            table: "FINANCIAL_TYPES",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IS_DELETED",
            table: "FINANCIAL_OPERATIONS",
            type: "bit",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "bit",
            oldDefaultValue: false);
    }
}
