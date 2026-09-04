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

    [HttpPatch("{id:int}/stock-almacen")]
    public async Task<IActionResult> AgregarStockAlmacen(int id, [FromBody] AgregarStockRequest? request)
    {
        if (request == null || request.Cantidad <= 0)
            return Fail("La cantidad debe ser mayor a 0");
        try
        {
            var stockAlmacenActual = await productos.AgregarStockAlmacenAsync(id, request.Cantidad);
            if (stockAlmacenActual == null)
                return Fail("Presentación no encontrada", StatusCodes.Status404NotFound);
            return Ok(new { stockAlmacenActual });
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpPost("{id:int}/mover-a-tienda")]
    public async Task<IActionResult> MoverATienda(int id, [FromBody] MoverAlmacenRequest? request)
    {
        if (request == null || request.Cantidad <= 0)
            return Fail("La cantidad debe ser mayor a 0");
        try
        {
            var resultado = await productos.MoverAlmacenATiendaAsync(id, request.Cantidad);
            if (resultado == null)
                return Fail("Presentación no encontrada", StatusCodes.Status404NotFound);
            return Ok(new { stock = resultado.Value.stock, stockAlmacen = resultado.Value.stockAlmacen });
        }
        catch (InvalidOperationException ex) { return Fail(ex.Message); }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpPatch("{id:int}/usa-almacen")]
    public async Task<IActionResult> CambiarUsaAlmacen(int id, [FromBody] CambiarUsaAlmacenRequest? request)
    {
        if (request == null) return Fail("Datos inválidos");
        try
        {
            if (!await productos.CambiarUsaAlmacenAsync(id, request.Activo))
                return Fail("Presentación no encontrada", StatusCodes.Status404NotFound);
            return Ok<object?>(null);
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }
}
