using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddMigrationOrderDetails3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Weight",
                table: "OrderRequirements",
                newName: "WeightTo");

            migrationBuilder.AddColumn<decimal>(
                name: "WeightFrom",
                table: "OrderRequirements",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeightFrom",
                table: "OrderRequirements");

            migrationBuilder.RenameColumn(
                name: "WeightTo",
                table: "OrderRequirements",
                newName: "Weight");
        }
    }
}
