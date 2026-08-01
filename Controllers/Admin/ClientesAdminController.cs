using ForraControl.API.Interfaces;
using ForraControl.API.Models.Dtos.Clientes;
using Microsoft.AspNetCore.Mvc;

namespace ForraControl.API.Controllers.Admin;

[Route("api/admin/clientes")]
public class ClientesAdminController(IClienteService clientes) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        try { return Ok(await clientes.ObtenerTodosAsync()); }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearClienteRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Nombre))
            return Fail("El nombre del cliente es requerido");
        try
        {
            var id = await clientes.CrearAsync(request);
            return Created(new { id });
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarClienteRequest? request)
    {
        if (request == null) return Fail("Datos inválidos");
        try
        {
            if (!await clientes.ActualizarAsync(id, request))
                return Fail("Cliente no encontrado", StatusCodes.Status404NotFound);
            return Ok<object?>(null);
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            if (!await clientes.EliminarAsync(id))
                return Fail("Cliente no encontrado", StatusCodes.Status404NotFound);
            return NoContent();
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpPut("{id:int}/precios")]
    public async Task<IActionResult> ActualizarPrecios(int id, [FromBody] ActualizarPreciosRequest? request)
    {
        if (request == null) return Fail("Datos inválidos");
        try
        {
            if (!await clientes.ActualizarPreciosAsync(id, request.Precios ?? new List<PrecioClienteDto>()))
                return Fail("Cliente no encontrado", StatusCodes.Status404NotFound);
            return Ok<object?>(null);
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }
}
