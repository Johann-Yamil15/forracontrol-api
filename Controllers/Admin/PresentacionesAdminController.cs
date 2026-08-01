using ForraControl.API.Interfaces;
using ForraControl.API.Models.Dtos.Productos;
using Microsoft.AspNetCore.Mvc;

namespace ForraControl.API.Controllers.Admin;

[Route("api/admin/presentaciones")]
public class PresentacionesAdminController(IProductoService productos) : ApiControllerBase
{
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarPresentacionRequest? request)
    {
        if (request == null) return Fail("Datos inválidos");
        try
        {
            if (!await productos.ActualizarPresentacionAsync(id, request))
                return Fail("Presentación no encontrada", StatusCodes.Status404NotFound);
            return Ok<object?>(null);
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            if (!await productos.EliminarPresentacionAsync(id))
                return Fail("Presentación no encontrada", StatusCodes.Status404NotFound);
            return NoContent();
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpPatch("{id:int}/stock")]
    public async Task<IActionResult> AgregarStock(int id, [FromBody] AgregarStockRequest? request)
    {
        if (request == null || request.Cantidad <= 0)
            return Fail("La cantidad debe ser mayor a 0");
        try
        {
            var stockActual = await productos.AgregarStockAsync(id, request.Cantidad);
            if (stockActual == null)
                return Fail("Presentación no encontrada", StatusCodes.Status404NotFound);
            return Ok(new { stockActual });
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }
}
