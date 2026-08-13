using ForraControl.API.Interfaces;
using ForraControl.API.Services;
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

    // Único endpoint del API que no devuelve el sobre {ok,data}: un PDF es un
    // binario, igual que las imágenes servidas desde /uploads.
    [HttpGet("pdf")]
    public async Task<IActionResult> ObtenerReportePdf([FromQuery] string periodo = "hoy")
    {
        try
        {
            var reporte = await admin.ObtenerReporteCompletoAsync(periodo);
            var bytes = ReportePdfBuilder.Build(reporte);
            var fecha = DateTime.Now.ToString("yyyyMMdd_HHmm");
            return File(bytes, "application/pdf", $"reporte_forrastore_{periodo}_{fecha}.pdf");
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }
}
