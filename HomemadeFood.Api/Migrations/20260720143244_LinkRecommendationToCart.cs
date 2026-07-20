using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomemadeFood.Api.Migrations
{
    /// <inheritdoc />
    public partial class LinkRecommendationToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecommendationSearchId",
                table: "Carts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_RecommendationSearchId",
                table: "Carts",
                column: "RecommendationSearchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_RecommendationSearches_RecommendationSearchId",
                table: "Carts",
                column: "RecommendationSearchId",
                principalTable: "RecommendationSearches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_RecommendationSearches_RecommendationSearchId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_RecommendationSearchId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "RecommendationSearchId",
                table: "Carts");
        }
    }
}
