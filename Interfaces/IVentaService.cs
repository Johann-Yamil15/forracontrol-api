using ForraControl.API.Models.Dtos.Ventas;

namespace ForraControl.API.Interfaces;

public interface IVentaService
{
    Task<(int idVenta, DateTime fecha)> RegistrarAsync(RegistrarVentaRequest request);
    Task<IEnumerable<VentaDto>> ObtenerHistorialAsync(int? idUsuario, string? periodo);
    Task<VentaDto?> ObtenerDetalleAsync(int id);
}
