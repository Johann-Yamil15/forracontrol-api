namespace ForraControl.API.Models.Dtos.Ventas;

public class RegistrarVentaRequest
{
    public int IdUsuario { get; set; }
    public int? IdCliente { get; set; }
    public decimal TotalOriginal { get; set; }
    public decimal Descuento { get; set; }
    public decimal TotalFinal { get; set; }
    public List<VentaItemRequest> Items { get; set; } = new();
}

public class VentaItemRequest
{
    public int IdProducto { get; set; }
    public int IdPresentacion { get; set; }
    public string? NombreProducto { get; set; }
    public string? Unidad { get; set; }
    public decimal Tamano { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PrecioEfectivo { get; set; }
    public decimal Subtotal { get; set; }
}

public class VentaDto
{
    public string Id { get; set; } = "";
    public int IdVenta { get; set; }
    public DateTime Fecha { get; set; }
    public int? IdCliente { get; set; }
    public string NombreCliente { get; set; } = "";
    public decimal TotalOriginal { get; set; }
    public decimal Descuento { get; set; }
    public decimal TotalFinal { get; set; }
    public List<VentaItemDto> Items { get; set; } = new();
}

public class VentaItemDto
{
    public int IdProducto { get; set; }
    public string NombreProducto { get; set; } = "";
    public string ImagenUrl { get; set; } = "";
    public string Unidad { get; set; } = "";
    public decimal Tamano { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PrecioEfectivo { get; set; }
    public int Cantidad { get; set; }
}
