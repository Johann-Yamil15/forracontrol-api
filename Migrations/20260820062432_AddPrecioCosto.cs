using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForraControl.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPrecioCosto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "precio_costo",
                table: "presentaciones",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "precio_costo",
                table: "detalles_venta",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "precio_costo",
                table: "presentaciones");

            migrationBuilder.DropColumn(
                name: "precio_costo",
                table: "detalles_venta");
        }
    }
}
