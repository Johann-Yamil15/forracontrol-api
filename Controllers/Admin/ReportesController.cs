using ForraControl.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForraControl.API.Controllers.Admin;

[Route("api/admin/reportes")]
public class ReportesController(IAdminService admin) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtenerReporte([FromQuery] string periodo = "hoy")
    {
        try { return Ok(await admin.ObtenerReporteAsync(periodo)); }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }
}
