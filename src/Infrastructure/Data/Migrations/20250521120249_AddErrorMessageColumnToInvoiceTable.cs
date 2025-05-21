using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxReturnAutomation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddErrorMessageColumnToInvoiceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "ProcessedFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "ProcessedFiles");
        }
    }
}
