using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class add_field_type_table_disease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiseaseMedicine_Disease_DiseaseId",
                table: "DiseaseMedicine");

            migrationBuilder.DropForeignKey(
                name: "FK_DiseaseMedicine_Medicines_MedicineId",
                table: "DiseaseMedicine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiseaseMedicine",
                table: "DiseaseMedicine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Disease",
                table: "Disease");

            migrationBuilder.RenameTable(
                name: "DiseaseMedicine",
                newName: "DiseaseMedicines");

            migrationBuilder.RenameTable(
                name: "Disease",
                newName: "Diseases");

            migrationBuilder.RenameIndex(
                name: "IX_DiseaseMedicine_MedicineId",
                table: "DiseaseMedicines",
                newName: "IX_DiseaseMedicines_MedicineId");

            migrationBuilder.RenameIndex(
                name: "IX_DiseaseMedicine_DiseaseId",
                table: "DiseaseMedicines",
                newName: "IX_DiseaseMedicines_DiseaseId");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Diseases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiseaseMedicines",
                table: "DiseaseMedicines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Diseases",
                table: "Diseases",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DiseaseMedicines_Diseases_DiseaseId",
                table: "DiseaseMedicines",
                column: "DiseaseId",
                principalTable: "Diseases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DiseaseMedicines_Medicines_MedicineId",
                table: "DiseaseMedicines",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiseaseMedicines_Diseases_DiseaseId",
                table: "DiseaseMedicines");

            migrationBuilder.DropForeignKey(
                name: "FK_DiseaseMedicines_Medicines_MedicineId",
                table: "DiseaseMedicines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Diseases",
                table: "Diseases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DiseaseMedicines",
                table: "DiseaseMedicines");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Diseases");

            migrationBuilder.RenameTable(
                name: "Diseases",
                newName: "Disease");

            migrationBuilder.RenameTable(
                name: "DiseaseMedicines",
                newName: "DiseaseMedicine");

            migrationBuilder.RenameIndex(
                name: "IX_DiseaseMedicines_MedicineId",
                table: "DiseaseMedicine",
                newName: "IX_DiseaseMedicine_MedicineId");

            migrationBuilder.RenameIndex(
                name: "IX_DiseaseMedicines_DiseaseId",
                table: "DiseaseMedicine",
                newName: "IX_DiseaseMedicine_DiseaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Disease",
                table: "Disease",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DiseaseMedicine",
                table: "DiseaseMedicine",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DiseaseMedicine_Disease_DiseaseId",
                table: "DiseaseMedicine",
                column: "DiseaseId",
                principalTable: "Disease",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DiseaseMedicine_Medicines_MedicineId",
                table: "DiseaseMedicine",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
