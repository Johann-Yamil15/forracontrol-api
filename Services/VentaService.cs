using ForraControl.API.Data;
using ForraControl.API.Interfaces;
using ForraControl.API.Models.Dtos.Ventas;
using ForraControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForraControl.API.Services;

public class VentaService(ForraDbContext db) : IVentaService
{
    public async Task<(int idVenta, DateTime fecha)> RegistrarAsync(RegistrarVentaRequest request)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        var venta = new Venta
        {
            IdUsuario = request.IdUsuario,
            IdCliente = request.IdCliente,
            Fecha = DateTime.Now,
            TotalOriginal = request.TotalOriginal,
            Descuento = request.Descuento,
            TotalFinal = request.TotalFinal
        };
        db.Ventas.Add(venta);
        await db.SaveChangesAsync();

        foreach (var item in request.Items)
        {
            db.DetallesVenta.Add(new DetalleVenta
            {
                IdVenta = venta.Id,
                IdPresentacion = item.IdPresentacion,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario,
                PrecioEfectivo = item.PrecioEfectivo,
                Subtotal = item.Subtotal
            });

            // Descontar stock
            var pr = await db.Presentaciones.FindAsync(item.IdPresentacion);
            if (pr != null)
                pr.Stock -= item.Cantidad;
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return (venta.Id, venta.Fecha);
    }

    public async Task<IEnumerable<VentaDto>> ObtenerHistorialAsync(int? idUsuario, string? periodo)
    {
        var (inicio, fin) = RangoPeriodo(periodo);

        var query = db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Presentacion)
                    !.ThenInclude(pr => pr!.Producto)
            .Where(v => v.Fecha >= inicio && v.Fecha < fin);

        if (idUsuario.HasValue)
            query = query.Where(v => v.IdUsuario == idUsuario.Value);

        var ventas = await query.OrderByDescending(v => v.Fecha).ToListAsync();

        return ventas.Select(MapVenta);
    }

    public async Task<VentaDto?> ObtenerDetalleAsync(int id)
    {
        var venta = await db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.DetallesVenta)
                .ThenInclude(d => d.Presentacion)
                    !.ThenInclude(pr => pr!.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        return venta == null ? null : MapVenta(venta);
    }

    private static VentaDto MapVenta(Venta v)
    {
        return new VentaDto
        {
            Id = v.Id.ToString(),
            IdVenta = v.Id,
            Fecha = v.Fecha,
            IdCliente = v.IdCliente,
            NombreCliente = v.Cliente?.Nombre ?? "Venta al Público",
            TotalOriginal = v.TotalOriginal,
            Descuento = v.Descuento,
            TotalFinal = v.TotalFinal,
            Items = v.DetallesVenta.Select(d => new VentaItemDto
            {
                IdProducto = d.Presentacion?.Producto?.Id ?? 0,
                NombreProducto = d.Presentacion?.Producto?.Nombre ?? "",
                ImagenUrl = d.Presentacion?.Producto?.ImagenUrl ?? "",
                Unidad = d.Presentacion?.Unidad ?? "",
                Tamano = d.Presentacion?.Tamano ?? 0,
                PrecioUnitario = d.PrecioUnitario,
                PrecioEfectivo = d.PrecioEfectivo,
                Cantidad = d.Cantidad
            }).ToList()
        };
    }

    private static (DateTime inicio, DateTime fin) RangoPeriodo(string? periodo)
    {
        var hoy = DateTime.Today;
        switch (periodo?.ToLower())
        {
            case "semana": return (hoy.AddDays(-6), hoy.AddDays(1));
            case "mes": return (new DateTime(hoy.Year, hoy.Month, 1), hoy.AddDays(1));
            default: return (hoy, hoy.AddDays(1)); // hoy
        }
    }
}
