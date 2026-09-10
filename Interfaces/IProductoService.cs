using ForraControl.API.Models.Dtos.Productos;

namespace ForraControl.API.Interfaces;

public interface IProductoService
{
    Task<IEnumerable<ProductoCatalogoDto>> ObtenerCatalogoAsync();

    Task<IEnumerable<ProductoAdminDto>> ObtenerTodosAdminAsync();
    Task<int> CrearAsync(CrearProductoRequest request);
    Task<bool> ActualizarAsync(int id, ActualizarProductoRequest request);

    /// Da de baja el producto (Activo = false) sin borrarlo: conserva su
    /// historial de ventas. Devuelve false si no existe.
    Task<bool> EliminarAsync(int id);

    /// Reactiva un producto dado de baja (Activo = true). Devuelve false si
    /// no existe.
    Task<bool> ReactivarAsync(int id);

    Task<int> AgregarPresentacionAsync(int idProducto, CrearPresentacionRequest request);
    Task<bool> ActualizarPresentacionAsync(int id, ActualizarPresentacionRequest request);

    /// Da de baja la presentación (Activo = false) sin borrarla: si alguna
    /// tiene ventas asociadas, un borrado físico violaría la FK. Devuelve
    /// false si no existe.
    Task<bool> EliminarPresentacionAsync(int id);
    Task<int?> AgregarStockAsync(int id, int cantidad);

    /// Suma (o resta, si cantidad es negativa — sirve para corregir un
    /// exceso capturado por error) al stock de almacén. Devuelve null si la
    /// presentación no existe; lanza InvalidOperationException si el
    /// resultado quedaría negativo.
    Task<int?> AgregarStockAlmacenAsync(int id, int cantidad);

    /// Transfiere cantidad de almacén a tienda. Devuelve null si la presentación
    /// no existe; lanza InvalidOperationException si no hay suficiente stock en almacén.
    Task<(int stock, int stockAlmacen)?> MoverAlmacenATiendaAsync(int id, int cantidad);

    /// Marca si la presentación se guarda en almacén o no (algunas siempre
    /// entregan directo en tienda). Devuelve false si no existe.
    Task<bool> CambiarUsaAlmacenAsync(int id, bool activo);

    /// Valida, sanitiza (quita EXIF/GPS) y redimensiona la imagen, la guarda en
    /// disco y actualiza el producto. Devuelve la ruta relativa (ej. "/uploads/productos/xxx.jpg")
    /// o null si el producto no existe.
    Task<string?> GuardarImagenAsync(int idProducto, Stream contenido);
}
