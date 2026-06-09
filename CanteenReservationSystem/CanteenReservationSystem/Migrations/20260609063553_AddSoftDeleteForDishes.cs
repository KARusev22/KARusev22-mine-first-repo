using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CanteenReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteForDishes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Dishes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Dishes");
        }
    }
}
