using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class modify_fields_table_inspection_code : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SpecieType",
                table: "InspectionCodeRanges",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "InspectionCodeCounters",
                newName: "SpecieType");

            migrationBuilder.AddColumn<string>(
                name: "SpecieTypes",
                table: "InspectionCodeRanges",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecieTypes",
                table: "InspectionCodeRanges");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "InspectionCodeRanges",
                newName: "SpecieType");

            migrationBuilder.RenameColumn(
                name: "SpecieType",
                table: "InspectionCodeCounters",
                newName: "Type");
        }
    }
}
