using ForraControl.API.Models.Dtos.Admin;

namespace ForraControl.API.Interfaces;

public interface IAdminService
{
    Task<DashboardDto> ObtenerDashboardAsync();
    Task<ReporteDto> ObtenerReporteAsync(DateTime desde, DateTime hasta);
    Task<ReporteCompletoDto> ObtenerReporteCompletoAsync(DateTime desde, DateTime hasta);
}
