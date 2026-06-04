using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanteenReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Nutritions_DishId",
                table: "Nutritions");

            migrationBuilder.CreateIndex(
                name: "IX_Nutritions_DishId",
                table: "Nutritions",
                column: "DishId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Nutritions_DishId",
                table: "Nutritions");

            migrationBuilder.CreateIndex(
                name: "IX_Nutritions_DishId",
                table: "Nutritions",
                column: "DishId");
        }
    }
}
