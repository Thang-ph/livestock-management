using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class AddInsurancetablev3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceRequests_OrderRequirements_OrderRequirementId",
                table: "InsuranceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceRequests_ProcurementDetails_ProcurementDetailId",
                table: "InsuranceRequests");

            migrationBuilder.RenameColumn(
                name: "ProcurementDetailId",
                table: "InsuranceRequests",
                newName: "ProcurementId");

            migrationBuilder.RenameColumn(
                name: "OrderRequirementId",
                table: "InsuranceRequests",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceRequests_ProcurementDetailId",
                table: "InsuranceRequests",
                newName: "IX_InsuranceRequests_ProcurementId");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceRequests_OrderRequirementId",
                table: "InsuranceRequests",
                newName: "IX_InsuranceRequests_OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceRequests_Order_OrderId",
                table: "InsuranceRequests",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceRequests_ProcurementPackages_ProcurementId",
                table: "InsuranceRequests",
                column: "ProcurementId",
                principalTable: "ProcurementPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceRequests_Order_OrderId",
                table: "InsuranceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceRequests_ProcurementPackages_ProcurementId",
                table: "InsuranceRequests");

            migrationBuilder.RenameColumn(
                name: "ProcurementId",
                table: "InsuranceRequests",
                newName: "ProcurementDetailId");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "InsuranceRequests",
                newName: "OrderRequirementId");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceRequests_ProcurementId",
                table: "InsuranceRequests",
                newName: "IX_InsuranceRequests_ProcurementDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceRequests_OrderId",
                table: "InsuranceRequests",
                newName: "IX_InsuranceRequests_OrderRequirementId");

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
        }
    }
}
