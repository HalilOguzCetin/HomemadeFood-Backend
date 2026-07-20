using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomemadeFood.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRecommendationLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecommendationSearches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustomerUserId = table.Column<int>(type: "int", nullable: false),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    CustomerLatitude = table.Column<double>(type: "double", nullable: false),
                    CustomerLongitude = table.Column<double>(type: "double", nullable: false),
                    SearchText = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestedQuantity = table.Column<int>(type: "int", nullable: true),
                    SelectedFoodId = table.Column<int>(type: "int", nullable: true),
                    SelectedProducerProfileId = table.Column<int>(type: "int", nullable: true),
                    SelectedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationSearches", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RecommendationCandidates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RecommendationSearchId = table.Column<int>(type: "int", nullable: false),
                    FoodId = table.Column<int>(type: "int", nullable: false),
                    FoodName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProducerProfileId = table.Column<int>(type: "int", nullable: false),
                    BusinessName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    DistanceKm = table.Column<double>(type: "double", nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    ReviewCount = table.Column<int>(type: "int", nullable: false),
                    PreparationTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    RemainingCapacity = table.Column<int>(type: "int", nullable: false),
                    RatingScore = table.Column<double>(type: "double", nullable: false),
                    DistanceScore = table.Column<double>(type: "double", nullable: false),
                    PreparationScore = table.Column<double>(type: "double", nullable: false),
                    TotalScore = table.Column<double>(type: "double", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationCandidates_RecommendationSearches_Recommendati~",
                        column: x => x.RecommendationSearchId,
                        principalTable: "RecommendationSearches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationCandidates_RecommendationSearchId_Rank",
                table: "RecommendationCandidates",
                columns: new[] { "RecommendationSearchId", "Rank" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecommendationCandidates");

            migrationBuilder.DropTable(
                name: "RecommendationSearches");
        }
    }
}
