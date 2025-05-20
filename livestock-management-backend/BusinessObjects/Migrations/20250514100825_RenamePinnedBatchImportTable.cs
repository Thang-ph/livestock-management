using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class RenamePinnedBatchImportTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PinnedBatchImport_BatchImports_BatchImportId",
                table: "PinnedBatchImport");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PinnedBatchImport",
                table: "PinnedBatchImport");

            migrationBuilder.RenameTable(
                name: "PinnedBatchImport",
                newName: "PinnedBatchImports");

            migrationBuilder.RenameIndex(
                name: "IX_PinnedBatchImport_BatchImportId",
                table: "PinnedBatchImports",
                newName: "IX_PinnedBatchImports_BatchImportId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PinnedBatchImports",
                table: "PinnedBatchImports",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PinnedBatchImports_BatchImports_BatchImportId",
                table: "PinnedBatchImports",
                column: "BatchImportId",
                principalTable: "BatchImports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PinnedBatchImports_BatchImports_BatchImportId",
                table: "PinnedBatchImports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PinnedBatchImports",
                table: "PinnedBatchImports");

            migrationBuilder.RenameTable(
                name: "PinnedBatchImports",
                newName: "PinnedBatchImport");

            migrationBuilder.RenameIndex(
                name: "IX_PinnedBatchImports_BatchImportId",
                table: "PinnedBatchImport",
                newName: "IX_PinnedBatchImport_BatchImportId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PinnedBatchImport",
                table: "PinnedBatchImport",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PinnedBatchImport_BatchImports_BatchImportId",
                table: "PinnedBatchImport",
                column: "BatchImportId",
                principalTable: "BatchImports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
