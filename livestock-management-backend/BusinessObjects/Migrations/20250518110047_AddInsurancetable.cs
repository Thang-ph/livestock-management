using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddInsurancetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderRequirementId",
                table: "InsuranceRequest",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcurementDetailId",
                table: "InsuranceRequest",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceRequest_OrderRequirementId",
                table: "InsuranceRequest",
                column: "OrderRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceRequest_ProcurementDetailId",
                table: "InsuranceRequest",
                column: "ProcurementDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceRequest_OrderRequirement_OrderRequirementId",
                table: "InsuranceRequest",
                column: "OrderRequirementId",
                principalTable: "OrderRequirement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceRequest_ProcurementDetails_ProcurementDetailId",
                table: "InsuranceRequest",
                column: "ProcurementDetailId",
                principalTable: "ProcurementDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceRequest_OrderRequirement_OrderRequirementId",
                table: "InsuranceRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceRequest_ProcurementDetails_ProcurementDetailId",
                table: "InsuranceRequest");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceRequest_OrderRequirementId",
                table: "InsuranceRequest");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceRequest_ProcurementDetailId",
                table: "InsuranceRequest");

            migrationBuilder.DropColumn(
                name: "OrderRequirementId",
                table: "InsuranceRequest");

            migrationBuilder.DropColumn(
                name: "ProcurementDetailId",
                table: "InsuranceRequest");
        }
    }
}
