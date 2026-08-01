namespace ForraControl.API.Models.Entities;

// tamano es DECIMAL(10,2) — ej: 50 para "Bulto 50 kg"
public class Presentacion
{
    public int Id { get; set; }
    public int IdProducto { get; set; }
    public string Unidad { get; set; } = "";
    public decimal Tamano { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; } = 5;
    public bool Activo { get; set; } = true;

    public Producto? Producto { get; set; }
    public ICollection<PrecioEspecial> PreciosEspeciales { get; set; } = new List<PrecioEspecial>();
    public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
}
