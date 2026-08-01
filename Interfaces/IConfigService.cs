using ForraControl.API.Models.Dtos.Config;

namespace ForraControl.API.Interfaces;

public interface IConfigService
{
    Task<IEnumerable<string>> ObtenerCategoriasAsync();
    Task<IEnumerable<string>> ObtenerSubcategoriasAsync();
    Task<IEnumerable<string>> ObtenerUnidadesAsync();
    CatalogoItemDto AgregarCategoria(string nombre);
    CatalogoItemDto AgregarSubcategoria(string nombre);
    CatalogoItemDto AgregarUnidad(string nombre);
}
