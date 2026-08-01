using ForraControl.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForraControl.API.Controllers;

[Route("api/productos")]
public class ProductosController(IProductoService productos) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtenerCatalogo()
    {
        try
        {
            return Ok(await productos.ObtenerCatalogoAsync());
        }
        catch (Exception ex)
        {
            return Fail("Error interno: " + ex.Message, StatusCodes.Status500InternalServerError);
        }
    }
}
