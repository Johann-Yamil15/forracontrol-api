using ForraControl.API.Data;
using ForraControl.API.Interfaces;
using ForraControl.API.Models.Dtos.Config;
using Microsoft.EntityFrameworkCore;

namespace ForraControl.API.Services;

// Lee valores distintos de productos/presentaciones — no requiere tablas cat_* separadas
public class ConfigService(ForraDbContext db) : IConfigService
{
    public Task<IEnumerable<string>> ObtenerCategoriasAsync() => ObtenerDistinctAsync(
        db.Productos.Where(p => p.Categoria != null && p.Categoria != "").Select(p => p.Categoria!));

    public Task<IEnumerable<string>> ObtenerSubcategoriasAsync() => ObtenerDistinctAsync(
        db.Productos.Where(p => p.Subcategoria != null && p.Subcategoria != "").Select(p => p.Subcategoria!));

    public Task<IEnumerable<string>> ObtenerUnidadesAsync() => ObtenerDistinctAsync(
        db.Presentaciones.Where(pr => pr.Unidad != null && pr.Unidad != "").Select(pr => pr.Unidad));

    private static async Task<IEnumerable<string>> ObtenerDistinctAsync(IQueryable<string> query)
        => await query.Distinct().OrderBy(v => v).ToListAsync();

    // POST devuelve el nombre tal como se envió (sin persistir en tabla separada)
    // Los valores quedarán disponibles al guardar el próximo producto/presentación
    public CatalogoItemDto AgregarCategoria(string nombre) => new() { Id = 0, Nombre = nombre };

    public CatalogoItemDto AgregarSubcategoria(string nombre) => new() { Id = 0, Nombre = nombre };

    public CatalogoItemDto AgregarUnidad(string nombre) => new() { Id = 0, Nombre = nombre };
}
