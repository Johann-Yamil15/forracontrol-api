namespace ForraControl.API.Models.Dtos.Clientes;

// ─── Dropdown (trabajador) ──────────────────────────────────────────────

public class ClienteDropdownDto
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = "";
    public string Telefono { get; set; } = "";
    public List<DescuentoClienteDto> Descuentos { get; set; } = new();
}

public class DescuentoClienteDto
{
    public int IdPresentacion { get; set; }
    public decimal PrecioEspecial { get; set; }
}

// ─── Admin ───────────────────────────────────────────────────────────────

public class ClienteAdminDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Telefono { get; set; } = "";
    public bool Activo { get; set; }
    public List<PrecioClienteDto> Precios { get; set; } = new();
}

public class PrecioClienteDto
{
    public int IdProducto { get; set; }
    public int IdPresentacion { get; set; }
    public string ProductoNombre { get; set; } = "";
    public string PresentacionDesc { get; set; } = "";
    public decimal PrecioLista { get; set; }
    public decimal PrecioEspecial { get; set; }
}

public class CrearClienteRequest
{
    public string Nombre { get; set; } = "";
    public string? Telefono { get; set; }
}

public class ActualizarClienteRequest
{
    public string Nombre { get; set; } = "";
    public string? Telefono { get; set; }
}

public class ActualizarPreciosRequest
{
    public List<PrecioClienteDto>? Precios { get; set; }
}
