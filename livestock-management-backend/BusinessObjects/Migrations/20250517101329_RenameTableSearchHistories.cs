using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class RenameTableSearchHistories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SearchHistory_Livestocks_LivestockId",
                table: "SearchHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SearchHistory",
                table: "SearchHistory");

            migrationBuilder.RenameTable(
                name: "SearchHistory",
                newName: "SearchHistories");

            migrationBuilder.RenameIndex(
                name: "IX_SearchHistory_LivestockId",
                table: "SearchHistories",
                newName: "IX_SearchHistories_LivestockId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SearchHistories",
                table: "SearchHistories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchHistories_Livestocks_LivestockId",
                table: "SearchHistories",
                column: "LivestockId",
                principalTable: "Livestocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SearchHistories_Livestocks_LivestockId",
                table: "SearchHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SearchHistories",
                table: "SearchHistories");

            migrationBuilder.RenameTable(
                name: "SearchHistories",
                newName: "SearchHistory");

            migrationBuilder.RenameIndex(
                name: "IX_SearchHistories_LivestockId",
                table: "SearchHistory",
                newName: "IX_SearchHistory_LivestockId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SearchHistory",
                table: "SearchHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchHistory_Livestocks_LivestockId",
                table: "SearchHistory",
                column: "LivestockId",
                principalTable: "Livestocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
