using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForraControl.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMinimoAlmacen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "stock_minimo_almacen",
                table: "presentaciones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "chk_presentaciones_stock_minimo_almacen",
                table: "presentaciones",
                sql: "stock_minimo_almacen >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_presentaciones_stock_minimo_almacen",
                table: "presentaciones");

            migrationBuilder.DropColumn(
                name: "stock_minimo_almacen",
                table: "presentaciones");
        }
    }
}
