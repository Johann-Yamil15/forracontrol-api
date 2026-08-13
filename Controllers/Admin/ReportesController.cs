using ForraControl.API.Interfaces;
using ForraControl.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ForraControl.API.Controllers.Admin;

[Route("api/admin/reportes")]
public class ReportesController(IAdminService admin) : ApiControllerBase
{
    // Sin desde/hasta, cae en "hoy" (mismo comportamiento que antes).
    private static (DateTime Desde, DateTime Hasta) ResolverRango(DateTime? desde, DateTime? hasta)
    {
        var hoy = DateTime.Today;
        var d = (desde ?? hoy).Date;
        var h = (hasta ?? d).Date;
        return d <= h ? (d, h) : (h, d);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerReporte([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var (d, h) = ResolverRango(desde, hasta);
        try { return Ok(await admin.ObtenerReporteAsync(d, h)); }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }

    // Único endpoint del API que no devuelve el sobre {ok,data}: un PDF es un
    // binario, igual que las imágenes servidas desde /uploads.
    [HttpGet("pdf")]
    public async Task<IActionResult> ObtenerReportePdf([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var (d, h) = ResolverRango(desde, hasta);
        try
        {
            var reporte = await admin.ObtenerReporteCompletoAsync(d, h);
            var bytes = ReportePdfBuilder.Build(reporte);
            var nombreRango = d == h ? d.ToString("yyyyMMdd") : $"{d:yyyyMMdd}_{h:yyyyMMdd}";
            return File(bytes, "application/pdf", $"reporte_forrastore_{nombreRango}.pdf");
        }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }
}
