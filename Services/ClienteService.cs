using ForraControl.API.Data;
using ForraControl.API.Interfaces;
using ForraControl.API.Models.Dtos.Clientes;
using ForraControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForraControl.API.Services;

public class ClienteService(ForraDbContext db) : IClienteService
{
    public async Task<IEnumerable<ClienteDropdownDto>> ObtenerDropdownAsync()
    {
        var clientes = await db.Clientes
            .Where(c => c.Activo)
            .Include(c => c.PreciosEspeciales)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return clientes.Select(c => new ClienteDropdownDto
        {
            IdCliente = c.Id,
            Nombre = c.Nombre,
            Telefono = c.Telefono ?? "",
            Descuentos = c.PreciosEspeciales.Select(p => new DescuentoClienteDto
            {
                IdPresentacion = p.IdPresentacion,
                PrecioEspecial = p.Precio
            }).ToList()
        });
    }

    public async Task<IEnumerable<ClienteAdminDto>> ObtenerTodosAsync()
    {
        var clientes = await db.Clientes
            .Include(c => c.PreciosEspeciales)
                .ThenInclude(pe => pe.Presentacion)
                    .ThenInclude(pr => pr!.Producto)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return clientes.Select(c => new ClienteAdminDto
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Telefono = c.Telefono ?? "",
            Activo = c.Activo,
            Precios = c.PreciosEspeciales.Select(pe => new PrecioClienteDto
            {
                IdProducto = pe.Presentacion?.Producto?.Id ?? 0,
                IdPresentacion = pe.IdPresentacion,
                ProductoNombre = pe.Presentacion?.Producto?.Nombre ?? "",
                PresentacionDesc = ProductoService.Desc(pe.Presentacion?.Unidad ?? "", pe.Presentacion?.Tamano ?? 0),
                PrecioLista = pe.Presentacion?.Precio ?? 0,
                PrecioEspecial = pe.Precio
            }).ToList()
        });
    }

    public async Task<int> CrearAsync(CrearClienteRequest request)
    {
        var cliente = new Cliente
        {
            Nombre = request.Nombre,
            Telefono = request.Telefono ?? "",
            Activo = true
        };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();
        return cliente.Id;
    }

    public async Task<bool> ActualizarAsync(int id, ActualizarClienteRequest request)
    {
        var cliente = await db.Clientes.FindAsync(id);
        if (cliente == null) return false;

        cliente.Nombre = request.Nombre;
        cliente.Telefono = request.Telefono ?? "";
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var cliente = await db.Clientes.FindAsync(id);
        if (cliente == null) return false;

        db.Clientes.Remove(cliente);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarPreciosAsync(int id, List<PrecioClienteDto> precios)
    {
        var cliente = await db.Clientes.FindAsync(id);
        if (cliente == null) return false;

        await using var tx = await db.Database.BeginTransactionAsync();

        var existentes = await db.PreciosEspeciales.Where(p => p.IdCliente == id).ToListAsync();
        db.PreciosEspeciales.RemoveRange(existentes);

        foreach (var p in precios)
        {
            db.PreciosEspeciales.Add(new PrecioEspecial
            {
                IdCliente = id,
                IdPresentacion = p.IdPresentacion,
                Precio = p.PrecioEspecial
            });
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return true;
    }
}
