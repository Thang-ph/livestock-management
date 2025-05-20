using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    public partial class addTableBatchVaccinationProcurement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatchVaccinationProcurement",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BatchVaccinationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProcurementDetailId = table.Column<string>(type: "nvarchar(450)", nullable: false),  // Sửa tên trường ở đây
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchVaccinationProcurement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchVaccinationProcurement_BatchVaccinations_BatchVaccinationId",
                        column: x => x.BatchVaccinationId,
                        principalTable: "BatchVaccinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BatchVaccinationProcurement_ProcurementDetails_ProcurementDetailId",  // Sửa tên ForeignKey ở đây
                        column: x => x.ProcurementDetailId,  // Sửa tên cột ở đây
                        principalTable: "ProcurementDetails",  // Sửa tên bảng ở đây
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchVaccinationProcurement_BatchVaccinationId",
                table: "BatchVaccinationProcurement",
                column: "BatchVaccinationId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchVaccinationProcurement_ProcurementDetailId",  // Sửa tên chỉ mục ở đây
                table: "BatchVaccinationProcurement",
                column: "ProcurementDetailId");  // Sửa tên cột ở đây
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BatchVaccinationProcurement");
        }
    }
}
