using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class change_delete_behavior_diseases_and_medicalhistories_to_restrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_Diseases_DiseaseId",
                table: "MedicalHistories");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_Diseases_DiseaseId",
                table: "MedicalHistories",
                column: "DiseaseId",
                principalTable: "Diseases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_Diseases_DiseaseId",
                table: "MedicalHistories");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_Diseases_DiseaseId",
                table: "MedicalHistories",
                column: "DiseaseId",
                principalTable: "Diseases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
