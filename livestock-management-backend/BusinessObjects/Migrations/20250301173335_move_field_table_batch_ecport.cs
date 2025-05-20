using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class move_field_table_batch_ecport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerAddress",
                table: "BatchExportDetails");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "BatchExportDetails");

            migrationBuilder.DropColumn(
                name: "CustomerNote",
                table: "BatchExportDetails");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                table: "BatchExportDetails");

            migrationBuilder.AddColumn<string>(
                name: "CustomerAddress",
                table: "BatchExports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "BatchExports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerNote",
                table: "BatchExports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "BatchExports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerAddress",
                table: "BatchExports");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "BatchExports");

            migrationBuilder.DropColumn(
                name: "CustomerNote",
                table: "BatchExports");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                table: "BatchExports");

            migrationBuilder.AddColumn<string>(
                name: "CustomerAddress",
                table: "BatchExportDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "BatchExportDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerNote",
                table: "BatchExportDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "BatchExportDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
