using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomemadeFood.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerBusinessHoursAndFoodIngredients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailabilityMode",
                table: "ProducerProfiles",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ForceOpen")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Ingredients",
                table: "Foods",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProducerBusinessHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProducerProfileId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsClosed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OpenTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    CloseTime = table.Column<TimeOnly>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerBusinessHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProducerBusinessHours_ProducerProfiles_ProducerProfileId",
                        column: x => x.ProducerProfileId,
                        principalTable: "ProducerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerBusinessHours_ProducerProfileId_DayOfWeek",
                table: "ProducerBusinessHours",
                columns: new[] { "ProducerProfileId", "DayOfWeek" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProducerBusinessHours");

            migrationBuilder.DropColumn(
                name: "AvailabilityMode",
                table: "ProducerProfiles");

            migrationBuilder.DropColumn(
                name: "Ingredients",
                table: "Foods");
        }
    }
}
