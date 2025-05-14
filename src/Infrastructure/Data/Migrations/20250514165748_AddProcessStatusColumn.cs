using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxReturnAutomation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ProcessedFiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ProcessedFiles");
        }
    }
}
