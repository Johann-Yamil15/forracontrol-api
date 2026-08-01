using ForraControl.API.Interfaces;
using ForraControl.API.Models.Dtos.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ForraControl.API.Controllers;

[Route("api/auth")]
public class AuthController(IAuthService auth) : ApiControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest? request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username))
            return Fail("Datos de acceso inválidos");

        try
        {
            var result = await auth.LoginAsync(request.Username, request.Password);
            if (result == null)
                return Fail("Usuario o contraseña incorrectos", StatusCodes.Status401Unauthorized);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return Fail("Error interno: " + ex.Message, StatusCodes.Status500InternalServerError);
        }
    }
}
