using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomemadeFood.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerCapacityDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "CapacityDate",
                table: "ProducerProfiles",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapacityDate",
                table: "ProducerProfiles");
        }
    }
}
