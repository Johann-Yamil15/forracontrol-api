namespace ForraControl.API.Models.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Telefono { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<PrecioEspecial> PreciosEspeciales { get; set; } = new List<PrecioEspecial>();
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
