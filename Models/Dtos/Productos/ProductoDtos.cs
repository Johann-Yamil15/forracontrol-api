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
    public int StockAlmacen { get; set; }
    public int StockMinimo { get; set; }
    public int StockMinimoAlmacen { get; set; }
    public bool UsaAlmacen { get; set; }
    public bool EnAlerta { get; set; }
    public bool EnAlertaAlmacen { get; set; }
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
    public int StockAlmacen { get; set; }
    public int StockMinimo { get; set; }
    public int StockMinimoAlmacen { get; set; }
}

public class ActualizarPresentacionRequest
{
    public string Unidad { get; set; } = "";
    public decimal Tamano { get; set; }
    public decimal Precio { get; set; }
    public decimal? PrecioCosto { get; set; }
    public int Stock { get; set; }
    // StockAlmacen no se toca acá a propósito: el formulario general de
    // edición no lo rastrea, así que si se incluyera se pisaría a 0 en cada
    // guardado. Se maneja solo por los endpoints dedicados de almacén.
    // StockMinimoAlmacen sí es seguro incluirlo: es un umbral configurado a
    // propósito por el admin, no una cantidad viva que se mueve sola.
    public int StockMinimo { get; set; }
    public int StockMinimoAlmacen { get; set; }
}

public class AgregarStockRequest
{
    public int Cantidad { get; set; }
}

public class MoverAlmacenRequest
{
    public int Cantidad { get; set; }
}

public class CambiarUsaAlmacenRequest
{
    public bool Activo { get; set; }
}
