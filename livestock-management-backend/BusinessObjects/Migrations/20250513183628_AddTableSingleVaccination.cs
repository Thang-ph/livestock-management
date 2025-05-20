using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddTableSingleVaccination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SingleVaccination",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BatchImportId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LivestockId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MedicineId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleVaccination", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SingleVaccination_BatchImports_BatchImportId",
                        column: x => x.BatchImportId,
                        principalTable: "BatchImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SingleVaccination_Livestocks_LivestockId",
                        column: x => x.LivestockId,
                        principalTable: "Livestocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SingleVaccination_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SingleVaccination_BatchImportId",
                table: "SingleVaccination",
                column: "BatchImportId");

            migrationBuilder.CreateIndex(
                name: "IX_SingleVaccination_LivestockId",
                table: "SingleVaccination",
                column: "LivestockId");

            migrationBuilder.CreateIndex(
                name: "IX_SingleVaccination_MedicineId",
                table: "SingleVaccination",
                column: "MedicineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SingleVaccination");
        }
    }
}
