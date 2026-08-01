namespace ForraControl.API.Models.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }
    public string? Subcategoria { get; set; }
    public string? Uso { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<Presentacion> Presentaciones { get; set; } = new List<Presentacion>();
}
