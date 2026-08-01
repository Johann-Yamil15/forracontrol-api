using ForraControl.API.Common;
using ForraControl.API.Data;
using ForraControl.API.Interfaces;
using ForraControl.API.Models.Dtos.Productos;
using ForraControl.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ForraControl.API.Services;

public class ProductoService(ForraDbContext db, IConfiguration configuration) : IProductoService
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

    private const long MaxImagenBytes = 5 * 1024 * 1024; // 5 MB
    private const int MaxDimension = 1600; // px, lado más largo

    public async Task<string?> GuardarImagenAsync(int idProducto, Stream contenido)
    {
        if (contenido.CanSeek && contenido.Length > MaxImagenBytes)
            throw new InvalidOperationException("La imagen no debe superar 5 MB");

        var producto = await db.Productos.FindAsync(idProducto);
        if (producto == null) return null;

        // Decodifica y valida que sea una imagen real (lanza si el contenido no lo es).
        using var image = await Image.LoadAsync(contenido);

        // Redimensiona si excede el máximo permitido (mantiene proporción).
        if (image.Width > MaxDimension || image.Height > MaxDimension)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(MaxDimension, MaxDimension),
            }));
        }

        // Quita metadatos que puedan traer ubicación/fecha/dispositivo.
        // Se conserva el perfil ICC (color) porque no es información sensible.
        image.Metadata.ExifProfile = null;
        image.Metadata.IptcProfile = null;
        image.Metadata.XmpProfile = null;

        var uploadsRoot = UploadPaths.GetRoot(configuration);
        var productosDir = Path.Combine(uploadsRoot, "productos");
        Directory.CreateDirectory(productosDir);

        var fileName = $"{Guid.NewGuid():N}.jpg";
        var fullPath = Path.Combine(productosDir, fileName);
        await image.SaveAsync(fullPath, new JpegEncoder { Quality = 82 });

        // Verificación final de tamaño por si la recodificación superó el límite
        // (muy improbable con el resize + calidad 82, pero se cubre por seguridad).
        var savedInfo = new FileInfo(fullPath);
        if (savedInfo.Length > MaxImagenBytes)
        {
            savedInfo.Delete();
            throw new InvalidOperationException("La imagen no debe superar 5 MB");
        }

        // Borra la imagen anterior si era una subida local (best-effort).
        BorrarImagenLocalSiExiste(producto.ImagenUrl, uploadsRoot);

        var rutaRelativa = $"/uploads/productos/{fileName}";
        producto.ImagenUrl = rutaRelativa;
        await db.SaveChangesAsync();

        return rutaRelativa;
    }

    private static void BorrarImagenLocalSiExiste(string? imagenUrlActual, string uploadsRoot)
    {
        if (string.IsNullOrWhiteSpace(imagenUrlActual) || !imagenUrlActual.StartsWith("/uploads/"))
            return;

        try
        {
            var relativo = imagenUrlActual["/uploads/".Length..].Replace('/', Path.DirectorySeparatorChar);
            var absoluto = Path.Combine(uploadsRoot, relativo);
            if (File.Exists(absoluto)) File.Delete(absoluto);
        }
        catch
        {
            // No crítico: si falla el borrado, la imagen vieja queda huérfana en disco.
        }
    }

    // "Bulto 50" / "Kg" (si tamano=1 solo muestra la unidad)
    internal static string Desc(string unidad, decimal tamano)
        => tamano == 1 ? unidad ?? "" : $"{unidad} {tamano:G}";
}
