using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task11_DotNETBackendWebApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialDbCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FINANCIAL_TYPES",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NAME = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FINANCIAL_TYPES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FINANCIAL_OPERATIONS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AMOUNT = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DATE = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NOTE = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IS_DELETED = table.Column<bool>(type: "bit", nullable: false),
                    FINANCIAL_TYPE_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FINANCIAL_OPERATIONS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FINANCIAL_OPERATIONS_FINANCIAL_TYPES_FINANCIAL_TYPE_ID",
                        column: x => x.FINANCIAL_TYPE_ID,
                        principalTable: "FINANCIAL_TYPES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FINANCIAL_OPERATIONS_FINANCIAL_TYPE_ID",
                table: "FINANCIAL_OPERATIONS",
                column: "FINANCIAL_TYPE_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FINANCIAL_OPERATIONS");

            migrationBuilder.DropTable(
                name: "FINANCIAL_TYPES");
        }
    }
}
