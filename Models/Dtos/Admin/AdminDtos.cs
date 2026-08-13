namespace ForraControl.API.Models.Dtos.Admin;

// ─── Dashboard ─────────────────────────────────────────────────────────

public class DashboardDto
{
    public int VentasHoy { get; set; }
    public decimal TotalHoy { get; set; }
    public decimal TotalSemana { get; set; }
    public List<AlertaStockProductoDto> AlertasStock { get; set; } = new();
    public List<TopProductoDto> TopProductos { get; set; } = new();
    public List<VentaResumenDto> VentasRecientes { get; set; } = new();
}

public class AlertaStockProductoDto
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = "";
    public List<AlertaStockPresentacionDto> Presentaciones { get; set; } = new();
}

public class AlertaStockPresentacionDto
{
    public int IdPresentacion { get; set; }
    public string Descripcion { get; set; } = "";
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
}

public class TopProductoDto
{
    public string NombreProducto { get; set; } = "";
    public string DescripcionPresentacion { get; set; } = "";
    public int TotalVendido { get; set; }
}

public class VentaResumenDto
{
    public string Id { get; set; } = "";
    public DateTime Fecha { get; set; }
    public string NombreCliente { get; set; } = "";
    public int NumProductos { get; set; }
    public decimal TotalFinal { get; set; }
}

// ─── Reportes ──────────────────────────────────────────────────────────

public class ReporteDto
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal DescuentoTotal { get; set; }
    public int NumVentas { get; set; }
    public List<DesgloseDiarioDto> DesgloseDiario { get; set; } = new();
    public List<VentaResumenDto> Ventas { get; set; } = new();
}

public class DesgloseDiarioDto
{
    public string Etiqueta { get; set; } = "";
    public decimal Total { get; set; }
}

// ─── Reporte completo (PDF) ────────────────────────────────────────────

public class ReporteCompletoDto
{
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public DateTime GeneradoEn { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal DescuentoTotal { get; set; }
    public int NumVentas { get; set; }
    public decimal TicketPromedio { get; set; }
    public List<DesgloseDiarioDto> DesgloseDiario { get; set; } = new();
    public List<TopProductoDto> TopProductos { get; set; } = new();
    public List<VentaPorCategoriaDto> VentasPorCategoria { get; set; } = new();
    public List<AlertaStockProductoDto> AlertasStock { get; set; } = new();
    public List<InventarioItemDto> Inventario { get; set; } = new();
    public List<VentaResumenDto> Ventas { get; set; } = new();
}

public class VentaPorCategoriaDto
{
    public string Categoria { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
}

public class InventarioItemDto
{
    public string NombreProducto { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Presentacion { get; set; } = "";
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public string Estado { get; set; } = ""; // "ok" | "alerta"
}
