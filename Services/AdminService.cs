using ForraControl.API.Data;
using ForraControl.API.Interfaces;
using ForraControl.API.Models.Dtos.Admin;
using ForraControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForraControl.API.Services;

public class AdminService(ForraDbContext db) : IAdminService
{
    public async Task<DashboardDto> ObtenerDashboardAsync()
    {
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);
        var semana = hoy.AddDays(-6);

        var ventasHoy = await db.Ventas
            .Where(v => v.Fecha >= hoy && v.Fecha < manana)
            .ToListAsync();

        var totalSemana = await db.Ventas
            .Where(v => v.Fecha >= semana && v.Fecha < manana)
            .SumAsync(v => (decimal?)v.TotalFinal) ?? 0;

        // Alertas de stock
        var presentacionesAlerta = await db.Presentaciones
            .Include(pr => pr.Producto)
            .Where(pr => pr.Activo && pr.Producto!.Activo && pr.Stock <= pr.StockMinimo)
            .OrderBy(pr => pr.Stock)
            .ToListAsync();

        var alertas = presentacionesAlerta
            .GroupBy(pr => pr.Producto!)
            .Select(g => new AlertaStockProductoDto
            {
                IdProducto = g.Key.Id,
                NombreProducto = g.Key.Nombre,
                Presentaciones = g.Select(pr => new AlertaStockPresentacionDto
                {
                    IdPresentacion = pr.Id,
                    Descripcion = ProductoService.Desc(pr.Unidad, pr.Tamano),
                    Stock = pr.Stock,
                    StockMinimo = pr.StockMinimo
                }).ToList()
            }).ToList();

        // Top 3 productos vendidos
        var top = await (from dv in db.DetallesVenta
                          join pr in db.Presentaciones on dv.IdPresentacion equals pr.Id
                          join p in db.Productos on pr.IdProducto equals p.Id
                          group new { dv, pr, p } by new { dv.IdPresentacion, pr.Unidad, pr.Tamano, p.Nombre } into g
                          orderby g.Sum(x => x.dv.Cantidad) descending
                          select new
                          {
                              NombreProducto = g.Key.Nombre,
                              Unidad = g.Key.Unidad,
                              Tamano = g.Key.Tamano,
                              TotalVendido = g.Sum(x => x.dv.Cantidad)
                          })
                          .Take(3)
                          .ToListAsync();

        var topProductos = top.Select(x => new TopProductoDto
        {
            NombreProducto = x.NombreProducto,
            DescripcionPresentacion = ProductoService.Desc(x.Unidad, x.Tamano),
            TotalVendido = x.TotalVendido
        }).ToList();

        // Ventas recientes (últimas 4)
        var recientes = await db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.DetallesVenta)
            .OrderByDescending(v => v.Fecha)
            .Take(4)
            .ToListAsync();

        var ventasRecientes = recientes.Select(v => new VentaResumenDto
        {
            Id = v.Id.ToString(),
            Fecha = v.Fecha,
            NombreCliente = v.Cliente?.Nombre ?? "Venta al Público",
            NumProductos = v.DetallesVenta.Count,
            TotalFinal = v.TotalFinal
        }).ToList();

        return new DashboardDto
        {
            VentasHoy = ventasHoy.Count,
            TotalHoy = ventasHoy.Sum(v => v.TotalFinal),
            TotalSemana = totalSemana,
            AlertasStock = alertas,
            TopProductos = topProductos,
            VentasRecientes = ventasRecientes
        };
    }

    public async Task<ReporteDto> ObtenerReporteAsync(string? periodo)
    {
        var hoy = DateTime.Today;
        DateTime inicio = periodo?.ToLower() switch
        {
            "semana" => hoy.AddDays(-6),
            "mes" => new DateTime(hoy.Year, hoy.Month, 1),
            _ => hoy
        };
        var fin = hoy.AddDays(1);

        var ventas = await db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.DetallesVenta)
            .Where(v => v.Fecha >= inicio && v.Fecha < fin)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        return new ReporteDto
        {
            Periodo = periodo ?? "hoy",
            TotalVentas = ventas.Sum(v => v.TotalFinal),
            DescuentoTotal = ventas.Sum(v => v.Descuento),
            NumVentas = ventas.Count,
            DesgloseDiario = GenerarDesglose(ventas, periodo, inicio, hoy),
            Ventas = ventas.Select(v => new VentaResumenDto
            {
                Id = v.Id.ToString(),
                Fecha = v.Fecha,
                NombreCliente = v.Cliente?.Nombre ?? "Venta al Público",
                NumProductos = v.DetallesVenta.Count,
                TotalFinal = v.TotalFinal
            }).ToList()
        };
    }

    private static List<DesgloseDiarioDto> GenerarDesglose(
        List<Venta> ventas, string? periodo, DateTime inicio, DateTime hoy)
    {
        switch (periodo?.ToLower())
        {
            case "semana":
                var dias = new[] { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
                return Enumerable.Range(0, 7).Select(i =>
                {
                    var fecha = inicio.AddDays(i);
                    var diaIdx = ((int)fecha.DayOfWeek + 6) % 7;
                    return new DesgloseDiarioDto
                    {
                        Etiqueta = dias[diaIdx],
                        Total = ventas.Where(v => v.Fecha.Date == fecha).Sum(v => (decimal?)v.TotalFinal) ?? 0
                    };
                }).ToList();

            case "mes":
                var diasMes = (hoy - inicio).Days + 1;
                return Enumerable.Range(0, diasMes).Select(i =>
                {
                    var fecha = inicio.AddDays(i);
                    return new DesgloseDiarioDto
                    {
                        Etiqueta = fecha.ToString("dd/MM"),
                        Total = ventas.Where(v => v.Fecha.Date == fecha).Sum(v => (decimal?)v.TotalFinal) ?? 0
                    };
                }).ToList();

            default: // hoy
                return Enumerable.Range(0, 24).Select(h => new DesgloseDiarioDto
                {
                    Etiqueta = $"{h:D2}:00",
                    Total = ventas.Where(v => v.Fecha.Hour == h).Sum(v => (decimal?)v.TotalFinal) ?? 0
                }).ToList();
        }
    }
}
