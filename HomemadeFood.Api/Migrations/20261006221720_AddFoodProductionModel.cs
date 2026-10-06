using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomemadeFood.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFoodProductionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyCapacity",
                table: "Foods",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "MinimumOrderLeadTimeMinutes",
                table: "Foods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UnitType",
                table: "Foods",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Portion")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FoodAllergens",
                columns: table => new
                {
                    FoodId = table.Column<int>(type: "int", nullable: false),
                    AllergenCode = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodAllergens", x => new { x.FoodId, x.AllergenCode });
                    table.ForeignKey(
                        name: "FK_FoodAllergens_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FoodAllergens");

            migrationBuilder.DropColumn(
                name: "DailyCapacity",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "MinimumOrderLeadTimeMinutes",
                table: "Foods");

            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "Foods");
        }
    }
}
