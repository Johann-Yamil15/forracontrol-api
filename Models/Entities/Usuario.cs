namespace ForraControl.API.Models.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Rol { get; set; } = "";   // 'admin' | 'trabajador'
    public bool Activo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
