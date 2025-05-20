using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddInsurancetableVersion2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceRequest_Diseases_DiseaseId",
                table: "InsuranceRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceRequest_Livestocks_NewLivestockId",
                table: "InsuranceRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceRequest_OrderRequirement_OrderRequirementId",
                table: "InsuranceRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceRequest_ProcurementDetails_ProcurementDetailId",
                table: "InsuranceRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderRequirement_Order_OrderId",
                table: "OrderRequirement");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderRequirement_Species_SpecieId",
                table: "OrderRequirement");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRequirement_OrderRequirement_OrderRequirementId",
                table: "VaccinationRequirement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderRequirement",
                table: "OrderRequirement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InsuranceRequest",
                table: "InsuranceRequest");

            migrationBuilder.RenameTable(
                name: "OrderRequirement",
                newName: "OrderRequirements");

            migrationBuilder.RenameTable(
                name: "InsuranceRequest",
                newName: "InsuranceRequests");

            migrationBuilder.RenameIndex(
                name: "IX_OrderRequirement_SpecieId",
                table: "OrderRequirements",
                newName: "IX_OrderRequirements_SpecieId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderRequirement_OrderId",
                table: "OrderRequirements",
                newName: "IX_OrderRequirements_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceRequest_ProcurementDetailId",
                table: "InsuranceRequests",
                newName: "IX_InsuranceRequests_ProcurementDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceRequest_OrderRequirementId",
                table: "InsuranceRequests",
                newName: "IX_InsuranceRequests_OrderRequirementId");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceRequest_NewLivestockId",
                table: "InsuranceRequests",
                newName: "IX_InsuranceRequests_NewLivestockId");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceRequest_DiseaseId",
                table: "InsuranceRequests",
                newName: "IX_InsuranceRequests_DiseaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderRequirements",
                table: "OrderRequirements",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InsuranceRequests",
                table: "InsuranceRequests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceRequests_Diseases_DiseaseId",
                table: "InsuranceRequests",
                column: "DiseaseId",
                principalTable: "Diseases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceRequests_Livestocks_NewLivestockId",
                table: "InsuranceRequests",
                column: "NewLivestockId",
                principalTable: "Livestocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceRequests_OrderRequirements_OrderRequirementId",
                table: "InsuranceRequests",
                column: "OrderRequirementId",
                principalTable: "OrderRequirements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceRequests_ProcurementDetails_ProcurementDetailId",
                table: "InsuranceRequests",
                column: "ProcurementDetailId",
                principalTable: "ProcurementDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderRequirements_Order_OrderId",
                table: "OrderRequirements",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderRequirements_Species_SpecieId",
                table: "OrderRequirements",
                column: "SpecieId",
                principalTable: "Species",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRequirement_OrderRequirements_OrderRequirementId",
                table: "VaccinationRequirement",
                column: "OrderRequirementId",
                principalTable: "OrderRequirements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceRequests_Diseases_DiseaseId",
                table: "InsuranceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceRequests_Livestocks_NewLivestockId",
                table: "InsuranceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceRequests_OrderRequirements_OrderRequirementId",
                table: "InsuranceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceRequests_ProcurementDetails_ProcurementDetailId",
                table: "InsuranceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderRequirements_Order_OrderId",
                table: "OrderRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderRequirements_Species_SpecieId",
                table: "OrderRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRequirement_OrderRequirements_OrderRequirementId",
                table: "VaccinationRequirement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderRequirements",
                table: "OrderRequirements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InsuranceRequests",
                table: "InsuranceRequests");

            migrationBuilder.RenameTable(
                name: "OrderRequirements",
                newName: "OrderRequirement");

            migrationBuilder.RenameTable(
                name: "InsuranceRequests",
                newName: "InsuranceRequest");

            migrationBuilder.RenameIndex(
                name: "IX_OrderRequirements_SpecieId",
                table: "OrderRequirement",
                newName: "IX_OrderRequirement_SpecieId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderRequirements_OrderId",
                table: "OrderRequirement",
                newName: "IX_OrderRequirement_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceRequests_ProcurementDetailId",
                table: "InsuranceRequest",
                newName: "IX_InsuranceRequest_ProcurementDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceRequests_OrderRequirementId",
                table: "InsuranceRequest",
                newName: "IX_InsuranceRequest_OrderRequirementId");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceRequests_NewLivestockId",
                table: "InsuranceRequest",
                newName: "IX_InsuranceRequest_NewLivestockId");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceRequests_DiseaseId",
                table: "InsuranceRequest",
                newName: "IX_InsuranceRequest_DiseaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderRequirement",
                table: "OrderRequirement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InsuranceRequest",
                table: "InsuranceRequest",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceRequest_Diseases_DiseaseId",
                table: "InsuranceRequest",
                column: "DiseaseId",
                principalTable: "Diseases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceRequest_Livestocks_NewLivestockId",
                table: "InsuranceRequest",
                column: "NewLivestockId",
                principalTable: "Livestocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AddForeignKey(
                name: "FK_OrderRequirement_Order_OrderId",
                table: "OrderRequirement",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderRequirement_Species_SpecieId",
                table: "OrderRequirement",
                column: "SpecieId",
                principalTable: "Species",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRequirement_OrderRequirement_OrderRequirementId",
                table: "VaccinationRequirement",
                column: "OrderRequirementId",
                principalTable: "OrderRequirement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
