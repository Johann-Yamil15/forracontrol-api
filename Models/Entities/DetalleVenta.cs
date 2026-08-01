namespace ForraControl.API.Models.Entities;

// La BD NO tiene nombre_producto — se obtiene por join con presentaciones→productos
public class DetalleVenta
{
    public int Id { get; set; }
    public int IdVenta { get; set; }
    public int IdPresentacion { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PrecioEfectivo { get; set; }
    public decimal Subtotal { get; set; }

    public Venta? Venta { get; set; }
    public Presentacion? Presentacion { get; set; }
}
