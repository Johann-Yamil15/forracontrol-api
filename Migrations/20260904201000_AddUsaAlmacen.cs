using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForraControl.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUsaAlmacen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "usa_almacen",
                table: "presentaciones",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "usa_almacen",
                table: "presentaciones");
        }
    }
}
