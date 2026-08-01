using ForraControl.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForraControl.API.Controllers;

[Route("api/clientes")]
public class ClientesController(IClienteService clientes) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtenerDropdown()
    {
        try
        {
            return Ok(await clientes.ObtenerDropdownAsync());
        }
        catch (Exception ex)
        {
            return Fail("Error interno: " + ex.Message, StatusCodes.Status500InternalServerError);
        }
    }
}
