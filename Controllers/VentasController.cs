using ForraControl.API.Interfaces;
using ForraControl.API.Models.Dtos.Ventas;
using Microsoft.AspNetCore.Mvc;

namespace ForraControl.API.Controllers;

[Route("api/ventas")]
public class VentasController(IVentaService ventas) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarVentaRequest? request)
    {
        if (request == null || request.Items == null || request.Items.Count == 0)
            return Fail("La venta debe contener al menos un producto");

        try
        {
            var (idVenta, fecha) = await ventas.RegistrarAsync(request);
            return Created(new { idVenta, fecha });
        }
        catch (Exception ex)
        {
            return Fail("Error interno: " + ex.Message, StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerHistorial([FromQuery] int? idUsuario = null, [FromQuery] string? periodo = null)
    {
        try
        {
            return Ok(await ventas.ObtenerHistorialAsync(idUsuario, periodo));
        }
        catch (Exception ex)
        {
            return Fail("Error interno: " + ex.Message, StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerDetalle(int id)
    {
        try
        {
            var venta = await ventas.ObtenerDetalleAsync(id);
            if (venta == null) return Fail("Venta no encontrada", StatusCodes.Status404NotFound);
            return Ok(venta);
        }
        catch (Exception ex)
        {
            return Fail("Error interno: " + ex.Message, StatusCodes.Status500InternalServerError);
        }
    }
}
