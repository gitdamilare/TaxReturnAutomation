using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxReturnAutomation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerIdToInvoiceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "Invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Invoices");
        }
    }
}
