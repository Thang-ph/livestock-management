using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class update_database_version11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainingInsurance",
                table: "BatchExportDetails");

            migrationBuilder.RenameColumn(
                name: "RequireWeightMin",
                table: "ProcurementDetails",
                newName: "RequiredWeightMin");

            migrationBuilder.RenameColumn(
                name: "RequireWeightMax",
                table: "ProcurementDetails",
                newName: "RequiredWeightMax");

            migrationBuilder.RenameColumn(
                name: "RequireAgeMin",
                table: "ProcurementDetails",
                newName: "RequiredAgeMin");

            migrationBuilder.RenameColumn(
                name: "RequireAgeMax",
                table: "ProcurementDetails",
                newName: "RequiredAgeMax");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ProcurementPackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "ProcurementPackages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExpiredDuration",
                table: "ProcurementPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "ProcurementPackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SuccessDate",
                table: "ProcurementPackages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequiredInsurance",
                table: "ProcurementDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequiredQuantity",
                table: "ProcurementDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Remaining",
                table: "BatchExports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Total",
                table: "BatchExports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CustomerNote",
                table: "BatchExportDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredInsuranceDate",
                table: "BatchExportDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExportDate",
                table: "BatchExportDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HandoverDate",
                table: "BatchExportDetails",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "ProcurementPackages");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "ProcurementPackages");

            migrationBuilder.DropColumn(
                name: "ExpiredDuration",
                table: "ProcurementPackages");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "ProcurementPackages");

            migrationBuilder.DropColumn(
                name: "SuccessDate",
                table: "ProcurementPackages");

            migrationBuilder.DropColumn(
                name: "RequiredInsurance",
                table: "ProcurementDetails");

            migrationBuilder.DropColumn(
                name: "RequiredQuantity",
                table: "ProcurementDetails");

            migrationBuilder.DropColumn(
                name: "Remaining",
                table: "BatchExports");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "BatchExports");

            migrationBuilder.DropColumn(
                name: "CustomerNote",
                table: "BatchExportDetails");

            migrationBuilder.DropColumn(
                name: "ExpiredInsuranceDate",
                table: "BatchExportDetails");

            migrationBuilder.DropColumn(
                name: "ExportDate",
                table: "BatchExportDetails");

            migrationBuilder.DropColumn(
                name: "HandoverDate",
                table: "BatchExportDetails");

            migrationBuilder.RenameColumn(
                name: "RequiredWeightMin",
                table: "ProcurementDetails",
                newName: "RequireWeightMin");

            migrationBuilder.RenameColumn(
                name: "RequiredWeightMax",
                table: "ProcurementDetails",
                newName: "RequireWeightMax");

            migrationBuilder.RenameColumn(
                name: "RequiredAgeMin",
                table: "ProcurementDetails",
                newName: "RequireAgeMin");

            migrationBuilder.RenameColumn(
                name: "RequiredAgeMax",
                table: "ProcurementDetails",
                newName: "RequireAgeMax");

            migrationBuilder.AddColumn<int>(
                name: "RemainingInsurance",
                table: "BatchExportDetails",
                type: "int",
                nullable: true);
        }
    }
}
