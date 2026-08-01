using ForraControl.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ForraControl.API.Controllers.Admin;

[Route("api/admin/dashboard")]
public class DashboardController(IAdminService admin) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtenerDashboard()
    {
        try { return Ok(await admin.ObtenerDashboardAsync()); }
        catch (Exception ex) { return Fail(ex.Message, StatusCodes.Status500InternalServerError); }
    }
}
