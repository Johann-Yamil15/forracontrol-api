using ForraControl.API.Data;
using ForraControl.API.Interfaces;
using ForraControl.API.Models.Dtos.Productos;
using ForraControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForraControl.API.Services;

public class ProductoService(ForraDbContext db) : IProductoService
{
    // ── Catálogo trabajador ────────────────────────────────────────────────

    public async Task<IEnumerable<ProductoCatalogoDto>> ObtenerCatalogoAsync()
    {
        var presentaciones = await db.Presentaciones
            .Include(pr => pr.Producto)
            .Where(pr => pr.Activo && pr.Producto!.Activo)
            .OrderBy(pr => pr.Producto!.Nombre)
            .ThenBy(pr => pr.Precio)
            .ToListAsync();

        return presentaciones
            .GroupBy(pr => pr.Producto!)
            .Select(g => new ProductoCatalogoDto
            {
                IdProducto = g.Key.Id,
                NombreProducto = g.Key.Nombre,
                DescripcionProducto = g.Key.Descripcion ?? "",
                Categoria = g.Key.Categoria ?? "",
                Subcategoria = g.Key.Subcategoria ?? "",
                Uso = g.Key.Uso ?? "",
                ImagenUrl = g.Key.ImagenUrl ?? "",
                Presentaciones = g.Select(pr => new PresentacionCatalogoDto
                {
                    IdPresentacion = pr.Id,
                    Unidad = pr.Unidad,
                    Tamano = pr.Tamano,
                    Cantidad = Desc(pr.Unidad, pr.Tamano),
                    Precio = pr.Precio,
                    Stock = pr.Stock
                }).ToList()
            });
    }

    // ── Admin productos ────────────────────────────────────────────────────

    public async Task<IEnumerable<ProductoAdminDto>> ObtenerTodosAdminAsync()
    {
        var productos = await db.Productos
            .Include(p => p.Presentaciones)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        return productos.Select(p => new ProductoAdminDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion ?? "",
            Categoria = p.Categoria ?? "",
            Subcategoria = p.Subcategoria ?? "",
            Uso = p.Uso ?? "",
            ImagenUrl = p.ImagenUrl ?? "",
            Activo = p.Activo,
            Presentaciones = p.Presentaciones.Select(pr => new PresentacionAdminDto
            {
                Id = pr.Id,
                Unidad = pr.Unidad,
                Tamano = pr.Tamano,
                Cantidad = Desc(pr.Unidad, pr.Tamano),
                Precio = pr.Precio,
                Stock = pr.Stock,
                StockMinimo = pr.StockMinimo,
                EnAlerta = pr.Stock <= pr.StockMinimo
            }).ToList()
        });
    }

    public async Task<int> CrearAsync(CrearProductoRequest request)
    {
        var producto = new Producto
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion ?? "",
            Categoria = request.Categoria ?? "",
            Subcategoria = request.Subcategoria ?? "",
            Uso = request.Uso ?? "",
            ImagenUrl = request.ImagenUrl ?? "",
            Activo = true
        };

        if (request.Presentaciones != null)
        {
            foreach (var pr in request.Presentaciones)
            {
                producto.Presentaciones.Add(new Presentacion
                {
                    Unidad = pr.Unidad,
                    Tamano = pr.Tamano,
                    Precio = pr.Precio,
                    Stock = pr.Stock,
                    StockMinimo = pr.StockMinimo,
                    Activo = true
                });
            }
        }

        db.Productos.Add(producto);
        await db.SaveChangesAsync();
        return producto.Id;
    }

    public async Task<bool> ActualizarAsync(int id, ActualizarProductoRequest request)
    {
        var producto = await db.Productos.FindAsync(id);
        if (producto == null) return false;

        producto.Nombre = request.Nombre;
        producto.Descripcion = request.Descripcion ?? "";
        producto.Categoria = request.Categoria ?? "";
        producto.Subcategoria = request.Subcategoria ?? "";
        producto.Uso = request.Uso ?? "";
        producto.ImagenUrl = request.ImagenUrl ?? "";
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var producto = await db.Productos.FindAsync(id);
        if (producto == null) return false;

        producto.Activo = false;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<int> AgregarPresentacionAsync(int idProducto, CrearPresentacionRequest request)
    {
        var presentacion = new Presentacion
        {
            IdProducto = idProducto,
            Unidad = request.Unidad,
            Tamano = request.Tamano,
            Precio = request.Precio,
            Stock = request.Stock,
            StockMinimo = request.StockMinimo,
            Activo = true
        };
        db.Presentaciones.Add(presentacion);
        await db.SaveChangesAsync();
        return presentacion.Id;
    }

    public async Task<bool> ActualizarPresentacionAsync(int id, ActualizarPresentacionRequest request)
    {
        var pr = await db.Presentaciones.FindAsync(id);
        if (pr == null) return false;

        pr.Unidad = request.Unidad;
        pr.Tamano = request.Tamano;
        pr.Precio = request.Precio;
        pr.Stock = request.Stock;
        pr.StockMinimo = request.StockMinimo;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarPresentacionAsync(int id)
    {
        var pr = await db.Presentaciones.FindAsync(id);
        if (pr == null) return false;

        var precios = await db.PreciosEspeciales.Where(p => p.IdPresentacion == id).ToListAsync();
        db.PreciosEspeciales.RemoveRange(precios);
        db.Presentaciones.Remove(pr);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<int?> AgregarStockAsync(int id, int cantidad)
    {
        var pr = await db.Presentaciones.FindAsync(id);
        if (pr == null) return null;

        pr.Stock += cantidad;
        await db.SaveChangesAsync();
        return pr.Stock;
    }

    // "Bulto 50" / "Kg" (si tamano=1 solo muestra la unidad)
    internal static string Desc(string unidad, decimal tamano)
        => tamano == 1 ? unidad ?? "" : $"{unidad} {tamano:G}";
}
