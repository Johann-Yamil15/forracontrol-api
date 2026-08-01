using ForraControl.API.Interfaces;
using ForraControl.API.Models.Dtos.Config;
using Microsoft.AspNetCore.Mvc;

namespace ForraControl.API.Controllers.Config;

[Route("api/config")]
public class ConfigController(IConfigService config) : ApiControllerBase
{
    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias()
    {
        try { return Ok(await config.ObtenerCategoriasAsync()); }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpPost("categorias")]
    public IActionResult PostCategoria([FromBody] CrearCatalogoRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Nombre))
            return Fail("El nombre es requerido");
        try { return Created(config.AgregarCategoria(request.Nombre)); }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpGet("subcategorias")]
    public async Task<IActionResult> GetSubcategorias()
    {
        try { return Ok(await config.ObtenerSubcategoriasAsync()); }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpPost("subcategorias")]
    public IActionResult PostSubcategoria([FromBody] CrearCatalogoRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Nombre))
            return Fail("El nombre es requerido");
        try { return Created(config.AgregarSubcategoria(request.Nombre)); }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpGet("unidades")]
    public async Task<IActionResult> GetUnidades()
    {
        try { return Ok(await config.ObtenerUnidadesAsync()); }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    [HttpPost("unidades")]
    public IActionResult PostUnidad([FromBody] CrearCatalogoRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Nombre))
            return Fail("El nombre es requerido");
        try { return Created(config.AgregarUnidad(request.Nombre)); }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }
}
