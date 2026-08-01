using ForraControl.API.Models.Dtos.Productos;

namespace ForraControl.API.Interfaces;

public interface IProductoService
{
    Task<IEnumerable<ProductoCatalogoDto>> ObtenerCatalogoAsync();

    Task<IEnumerable<ProductoAdminDto>> ObtenerTodosAdminAsync();
    Task<int> CrearAsync(CrearProductoRequest request);
    Task<bool> ActualizarAsync(int id, ActualizarProductoRequest request);
    Task<bool> EliminarAsync(int id);
    Task<int> AgregarPresentacionAsync(int idProducto, CrearPresentacionRequest request);
    Task<bool> ActualizarPresentacionAsync(int id, ActualizarPresentacionRequest request);
    Task<bool> EliminarPresentacionAsync(int id);
    Task<int?> AgregarStockAsync(int id, int cantidad);

    /// Valida, sanitiza (quita EXIF/GPS) y redimensiona la imagen, la guarda en
    /// disco y actualiza el producto. Devuelve la ruta relativa (ej. "/uploads/productos/xxx.jpg")
    /// o null si el producto no existe.
    Task<string?> GuardarImagenAsync(int idProducto, Stream contenido);
}
