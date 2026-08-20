namespace ForraControl.API.Models.Dtos.Productos;

// ─── Catálogo (trabajador) ─────────────────────────────────────────────────

public class ProductoCatalogoDto
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = "";
    public string DescripcionProducto { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Subcategoria { get; set; } = "";
    public string Uso { get; set; } = "";
    public string ImagenUrl { get; set; } = "";
    public List<PresentacionCatalogoDto> Presentaciones { get; set; } = new();
}

public class PresentacionCatalogoDto
{
    public int IdPresentacion { get; set; }
    public string Unidad { get; set; } = "";
    public decimal Tamano { get; set; }
    public string Cantidad { get; set; } = "";   // "Bulto 50" / "Kg" — calculado
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}

// ─── Admin ───────────────────────────────────────────────────────────────

public class ProductoAdminDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Subcategoria { get; set; } = "";
    public string Uso { get; set; } = "";
    public string ImagenUrl { get; set; } = "";
    public bool Activo { get; set; }
    public List<PresentacionAdminDto> Presentaciones { get; set; } = new();
}

public class PresentacionAdminDto
{
    public int Id { get; set; }
    public string Unidad { get; set; } = "";
    public decimal Tamano { get; set; }
    public string Cantidad { get; set; } = "";   // "Bulto 50" / "Kg" — calculado
    public decimal Precio { get; set; }
    public decimal? PrecioCosto { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public bool EnAlerta { get; set; }
}

public class CrearProductoRequest
{
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }
    public string? Subcategoria { get; set; }
    public string? Uso { get; set; }
    public string? ImagenUrl { get; set; }
    public List<CrearPresentacionRequest>? Presentaciones { get; set; }
}

public class ActualizarProductoRequest
{
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }
    public string? Subcategoria { get; set; }
    public string? Uso { get; set; }
    public string? ImagenUrl { get; set; }
}

public class CrearPresentacionRequest
{
    public string Unidad { get; set; } = "";
    public decimal Tamano { get; set; }
    public decimal Precio { get; set; }
    public decimal? PrecioCosto { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
}

public class ActualizarPresentacionRequest
{
    public string Unidad { get; set; } = "";
    public decimal Tamano { get; set; }
    public decimal Precio { get; set; }
    public decimal? PrecioCosto { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
}

public class AgregarStockRequest
{
    public int Cantidad { get; set; }
}
