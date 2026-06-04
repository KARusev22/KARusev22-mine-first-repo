using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanteenReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddIdToDishIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "DishIngredients",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "DishIngredients");
        }
    }
}
