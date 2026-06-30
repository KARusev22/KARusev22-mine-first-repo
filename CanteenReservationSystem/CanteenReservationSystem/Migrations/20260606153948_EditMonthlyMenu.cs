using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanteenReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class EditMonthlyMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonthlyMenus_Dishes_DishId",
                table: "MonthlyMenus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MonthlyMenus",
                table: "MonthlyMenus");

            migrationBuilder.RenameTable(
                name: "MonthlyMenus",
                newName: "MonthlyMenu");

            migrationBuilder.RenameIndex(
                name: "IX_MonthlyMenus_DishId",
                table: "MonthlyMenu",
                newName: "IX_MonthlyMenu_DishId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MonthlyMenu",
                table: "MonthlyMenu",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlyMenu_Dishes_DishId",
                table: "MonthlyMenu",
                column: "DishId",
                principalTable: "Dishes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonthlyMenu_Dishes_DishId",
                table: "MonthlyMenu");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MonthlyMenu",
                table: "MonthlyMenu");

            migrationBuilder.RenameTable(
                name: "MonthlyMenu",
                newName: "MonthlyMenus");

            migrationBuilder.RenameIndex(
                name: "IX_MonthlyMenu_DishId",
                table: "MonthlyMenus",
                newName: "IX_MonthlyMenus_DishId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MonthlyMenus",
                table: "MonthlyMenus",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlyMenus_Dishes_DishId",
                table: "MonthlyMenus",
                column: "DishId",
                principalTable: "Dishes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
