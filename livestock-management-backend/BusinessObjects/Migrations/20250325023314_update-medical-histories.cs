using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class updatemedicalhistories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Disease",
                table: "MedicalHistories");

            migrationBuilder.AddColumn<string>(
                name: "DiseaseId",
                table: "MedicalHistories",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_DiseaseId",
                table: "MedicalHistories",
                column: "DiseaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_Diseases_DiseaseId",
                table: "MedicalHistories",
                column: "DiseaseId",
                principalTable: "Diseases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_Diseases_DiseaseId",
                table: "MedicalHistories");

            migrationBuilder.DropIndex(
                name: "IX_MedicalHistories_DiseaseId",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "DiseaseId",
                table: "MedicalHistories");

            migrationBuilder.AddColumn<string>(
                name: "Disease",
                table: "MedicalHistories",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
