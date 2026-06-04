using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanteenReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddIdToDishAllergen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "DishAllergens",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "DishAllergens");
        }
    }
}
