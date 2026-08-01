namespace ForraControl.API.Models.Entities;

public class PrecioEspecial
{
    public int Id { get; set; }
    public int IdCliente { get; set; }
    public int IdPresentacion { get; set; }
    public decimal Precio { get; set; }   // columna: precio_especial (ver ForraDbContext)

    public Cliente? Cliente { get; set; }
    public Presentacion? Presentacion { get; set; }
}
