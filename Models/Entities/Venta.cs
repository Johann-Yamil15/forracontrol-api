namespace ForraControl.API.Models.Entities;

public class Venta
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public int? IdCliente { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public decimal TotalOriginal { get; set; }
    public decimal Descuento { get; set; }
    public decimal TotalFinal { get; set; }

    public Usuario? Usuario { get; set; }
    public Cliente? Cliente { get; set; }
    public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
}
