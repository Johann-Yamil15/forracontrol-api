using ForraControl.API.Models.Dtos.Admin;

namespace ForraControl.API.Interfaces;

public interface IAdminService
{
    Task<DashboardDto> ObtenerDashboardAsync();
    Task<ReporteDto> ObtenerReporteAsync(string? periodo);
}
