using ForraControl.API.Interfaces;
using ForraControl.API.Models.Dtos.Productos;
using Microsoft.AspNetCore.Mvc;

namespace ForraControl.API.Controllers.Admin;

[Route("api/admin/productos")]
public class ProductosAdminController(IProductoService productos) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        try { return Ok(await productos.ObtenerTodosAdminAsync()); }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearProductoRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Nombre))
            return Fail("El nombre del producto es requerido");
        try
        {
            var id = await productos.CrearAsync(request);
            return Created(new { id });
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarProductoRequest? request)
    {
        if (request == null) return Fail("Datos inválidos");
        try
        {
            if (!await productos.ActualizarAsync(id, request))
                return Fail("Producto no encontrado", StatusCodes.Status404NotFound);
            return Ok<object?>(null);
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            if (!await productos.EliminarAsync(id))
                return Fail("Producto no encontrado", StatusCodes.Status404NotFound);
            return NoContent();
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpPost("{idProducto:int}/presentaciones")]
    public async Task<IActionResult> AgregarPresentacion(int idProducto, [FromBody] CrearPresentacionRequest? request)
    {
        if (request == null) return Fail("Datos inválidos");
        try
        {
            var id = await productos.AgregarPresentacionAsync(idProducto, request);
            return Created(new { id });
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }
}
