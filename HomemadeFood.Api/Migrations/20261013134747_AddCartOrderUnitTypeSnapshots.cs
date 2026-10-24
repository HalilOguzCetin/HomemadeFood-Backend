using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomemadeFood.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCartOrderUnitTypeSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnitTypeSnapshot",
                table: "OrderItems",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Portion")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UnitTypeSnapshot",
                table: "CartItems",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Portion")
                .Annotation("MySql:CharSet", "utf8mb4");

            /*
             * Mevcut sepet kayıtları migration sırasında önce
             * varsayılan "Portion" değerini alır.
             *
             * Food kaydı hâlâ mevcutsa snapshot değerini mevcut
             * Food.UnitType ile mümkün olduğunca doğru şekilde
             * backfill ediyoruz.
             */
            migrationBuilder.Sql(
                """
                UPDATE CartItems AS ci
                INNER JOIN Foods AS f
                    ON f.Id = ci.FoodId
                SET ci.UnitTypeSnapshot = f.UnitType
                WHERE f.UnitType IS NOT NULL
                  AND TRIM(f.UnitType) <> '';
                """
            );

            /*
             * Eski siparişlerde geçmişte kullanılan gerçek UnitType
             * daha önce snapshot edilmediği için birebir tarihsel
             * değer geri kazanılamaz.
             *
             * En güvenli mevcut veri Food.UnitType'tır.
             * Food artık bulunmuyorsa "Portion" fallback korunur.
             */
            migrationBuilder.Sql(
                """
                UPDATE OrderItems AS oi
                INNER JOIN Foods AS f
                    ON f.Id = oi.FoodId
                SET oi.UnitTypeSnapshot = f.UnitType
                WHERE f.UnitType IS NOT NULL
                  AND TRIM(f.UnitType) <> '';
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitTypeSnapshot",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UnitTypeSnapshot",
                table: "CartItems");
        }
    }
}