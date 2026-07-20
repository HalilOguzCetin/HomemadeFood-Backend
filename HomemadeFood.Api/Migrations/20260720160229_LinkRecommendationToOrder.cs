using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomemadeFood.Api.Migrations
{
    /// <inheritdoc />
    public partial class LinkRecommendationToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecommendationSearchId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RecommendationSearchId",
                table: "Orders",
                column: "RecommendationSearchId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_RecommendationSearches_RecommendationSearchId",
                table: "Orders",
                column: "RecommendationSearchId",
                principalTable: "RecommendationSearches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_RecommendationSearches_RecommendationSearchId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_RecommendationSearchId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RecommendationSearchId",
                table: "Orders");
        }
    }
}
