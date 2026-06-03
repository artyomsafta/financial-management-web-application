using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task11_DotNETBackendWebApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FINANCIAL_OPERATIONS_FINANCIAL_TYPES_FINANCIAL_TYPE_ID",
                table: "FINANCIAL_OPERATIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_FINANCIAL_OPERATIONS_WALLETS_WALLET_ID",
                table: "FINANCIAL_OPERATIONS");

            migrationBuilder.DropForeignKey(
                name: "FK_WALLETS_USERS_USER_ID",
                table: "WALLETS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WALLETS",
                table: "WALLETS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_USERS",
                table: "USERS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FINANCIAL_TYPES",
                table: "FINANCIAL_TYPES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FINANCIAL_OPERATIONS",
                table: "FINANCIAL_OPERATIONS");

            migrationBuilder.DropColumn(
                name: "BALANCE",
                table: "WALLETS");

            migrationBuilder.DropColumn(
                name: "BASE_CURRENCY",
                table: "WALLETS");

            migrationBuilder.DropColumn(
                name: "CURRENT_CURRENCY",
                table: "FINANCIAL_OPERATIONS");

            migrationBuilder.RenameTable(
                name: "WALLETS",
                newName: "Wallets");

            migrationBuilder.RenameTable(
                name: "USERS",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "FINANCIAL_TYPES",
                newName: "FinancialTypes");

            migrationBuilder.RenameTable(
                name: "FINANCIAL_OPERATIONS",
                newName: "FinancialOperations");

            migrationBuilder.RenameColumn(
                name: "NAME",
                table: "Wallets",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Wallets",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "USER_ID",
                table: "Wallets",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "IS_DELETED",
                table: "Wallets",
                newName: "IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_WALLETS_USER_ID",
                table: "Wallets",
                newName: "IX_Wallets_UserId");

            migrationBuilder.RenameColumn(
                name: "USERNAME",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "ROLE",
                table: "Users",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PASSWORD_HASH",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "IS_DELETED",
                table: "Users",
                newName: "IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_USERS_USERNAME",
                table: "Users",
                newName: "IX_Users_Username");

            migrationBuilder.RenameColumn(
                name: "NAME",
                table: "FinancialTypes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "DESCRIPTION",
                table: "FinancialTypes",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "FinancialTypes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "IS_INCOME",
                table: "FinancialTypes",
                newName: "IsIncome");

            migrationBuilder.RenameColumn(
                name: "IS_DELETED",
                table: "FinancialTypes",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "NOTE",
                table: "FinancialOperations",
                newName: "Note");

            migrationBuilder.RenameColumn(
                name: "DATE",
                table: "FinancialOperations",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "AMOUNT",
                table: "FinancialOperations",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "FinancialOperations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "WALLET_ID",
                table: "FinancialOperations",
                newName: "WalletId");

            migrationBuilder.RenameColumn(
                name: "IS_DELETED",
                table: "FinancialOperations",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "FINANCIAL_TYPE_ID",
                table: "FinancialOperations",
                newName: "FinancialTypeId");

            migrationBuilder.RenameColumn(
                name: "TRANSACTION_COMMENT",
                table: "FinancialOperations",
                newName: "Comment");

            migrationBuilder.RenameIndex(
                name: "IX_FINANCIAL_OPERATIONS_WALLET_ID",
                table: "FinancialOperations",
                newName: "IX_FinancialOperations_WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_FINANCIAL_OPERATIONS_FINANCIAL_TYPE_ID",
                table: "FinancialOperations",
                newName: "IX_FinancialOperations_FinancialTypeId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Wallets",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "Wallets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "FinancialTypes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "FinancialOperations",
                type: "decimal(20,4)",
                precision: 20,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "FinancialOperations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "FinancialOperations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Wallets",
                table: "Wallets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FinancialTypes",
                table: "FinancialTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FinancialOperations",
                table: "FinancialOperations",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CurrencyId",
                table: "Wallets",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTypes_Name",
                table: "FinancialTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialOperations_CurrencyId",
                table: "FinancialOperations",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialOperations_Currencies_CurrencyId",
                table: "FinancialOperations",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialOperations_FinancialTypes_FinancialTypeId",
                table: "FinancialOperations",
                column: "FinancialTypeId",
                principalTable: "FinancialTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialOperations_Wallets_WalletId",
                table: "FinancialOperations",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Currencies_CurrencyId",
                table: "Wallets",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Users_UserId",
                table: "Wallets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialOperations_Currencies_CurrencyId",
                table: "FinancialOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialOperations_FinancialTypes_FinancialTypeId",
                table: "FinancialOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialOperations_Wallets_WalletId",
                table: "FinancialOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Currencies_CurrencyId",
                table: "Wallets");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Users_UserId",
                table: "Wallets");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Wallets",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_CurrencyId",
                table: "Wallets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FinancialTypes",
                table: "FinancialTypes");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTypes_Name",
                table: "FinancialTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FinancialOperations",
                table: "FinancialOperations");

            migrationBuilder.DropIndex(
                name: "IX_FinancialOperations_CurrencyId",
                table: "FinancialOperations");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "FinancialOperations");

            migrationBuilder.RenameTable(
                name: "Wallets",
                newName: "WALLETS");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "USERS");

            migrationBuilder.RenameTable(
                name: "FinancialTypes",
                newName: "FINANCIAL_TYPES");

            migrationBuilder.RenameTable(
                name: "FinancialOperations",
                newName: "FINANCIAL_OPERATIONS");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "WALLETS",
                newName: "NAME");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "WALLETS",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "WALLETS",
                newName: "USER_ID");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "WALLETS",
                newName: "IS_DELETED");

            migrationBuilder.RenameIndex(
                name: "IX_Wallets_UserId",
                table: "WALLETS",
                newName: "IX_WALLETS_USER_ID");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "USERS",
                newName: "USERNAME");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "USERS",
                newName: "ROLE");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "USERS",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "USERS",
                newName: "PASSWORD_HASH");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "USERS",
                newName: "IS_DELETED");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Username",
                table: "USERS",
                newName: "IX_USERS_USERNAME");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "FINANCIAL_TYPES",
                newName: "NAME");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "FINANCIAL_TYPES",
                newName: "DESCRIPTION");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "FINANCIAL_TYPES",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "IsIncome",
                table: "FINANCIAL_TYPES",
                newName: "IS_INCOME");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "FINANCIAL_TYPES",
                newName: "IS_DELETED");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "FINANCIAL_OPERATIONS",
                newName: "NOTE");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "FINANCIAL_OPERATIONS",
                newName: "DATE");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "FINANCIAL_OPERATIONS",
                newName: "AMOUNT");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "FINANCIAL_OPERATIONS",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "WalletId",
                table: "FINANCIAL_OPERATIONS",
                newName: "WALLET_ID");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "FINANCIAL_OPERATIONS",
                newName: "IS_DELETED");

            migrationBuilder.RenameColumn(
                name: "FinancialTypeId",
                table: "FINANCIAL_OPERATIONS",
                newName: "FINANCIAL_TYPE_ID");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "FINANCIAL_OPERATIONS",
                newName: "TRANSACTION_COMMENT");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialOperations_WalletId",
                table: "FINANCIAL_OPERATIONS",
                newName: "IX_FINANCIAL_OPERATIONS_WALLET_ID");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialOperations_FinancialTypeId",
                table: "FINANCIAL_OPERATIONS",
                newName: "IX_FINANCIAL_OPERATIONS_FINANCIAL_TYPE_ID");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_DELETED",
                table: "WALLETS",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<decimal>(
                name: "BALANCE",
                table: "WALLETS",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BASE_CURRENCY",
                table: "WALLETS",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "IS_DELETED",
                table: "USERS",
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
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<decimal>(
                name: "AMOUNT",
                table: "FINANCIAL_OPERATIONS",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,4)",
                oldPrecision: 20,
                oldScale: 4);

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_WALLETS",
                table: "WALLETS",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_USERS",
                table: "USERS",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FINANCIAL_TYPES",
                table: "FINANCIAL_TYPES",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FINANCIAL_OPERATIONS",
                table: "FINANCIAL_OPERATIONS",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_FINANCIAL_OPERATIONS_FINANCIAL_TYPES_FINANCIAL_TYPE_ID",
                table: "FINANCIAL_OPERATIONS",
                column: "FINANCIAL_TYPE_ID",
                principalTable: "FINANCIAL_TYPES",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FINANCIAL_OPERATIONS_WALLETS_WALLET_ID",
                table: "FINANCIAL_OPERATIONS",
                column: "WALLET_ID",
                principalTable: "WALLETS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WALLETS_USERS_USER_ID",
                table: "WALLETS",
                column: "USER_ID",
                principalTable: "USERS",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
