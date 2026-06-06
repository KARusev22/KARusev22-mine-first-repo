using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanteenReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class MakeDishIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonthlyMenu_Dishes_DishId",
                table: "MonthlyMenu");

            migrationBuilder.AlterColumn<int>(
                name: "DishId",
                table: "MonthlyMenu",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableForDate",
                table: "CartItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlyMenu_Dishes_DishId",
                table: "MonthlyMenu",
                column: "DishId",
                principalTable: "Dishes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonthlyMenu_Dishes_DishId",
                table: "MonthlyMenu");

            migrationBuilder.DropColumn(
                name: "IsAvailableForDate",
                table: "CartItems");

            migrationBuilder.AlterColumn<int>(
                name: "DishId",
                table: "MonthlyMenu",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlyMenu_Dishes_DishId",
                table: "MonthlyMenu",
                column: "DishId",
                principalTable: "Dishes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
