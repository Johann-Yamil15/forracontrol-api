using ForraControl.API.Models.Dtos.Clientes;

namespace ForraControl.API.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<ClienteDropdownDto>> ObtenerDropdownAsync();
    Task<IEnumerable<ClienteAdminDto>> ObtenerTodosAsync();
    Task<int> CrearAsync(CrearClienteRequest request);
    Task<bool> ActualizarAsync(int id, ActualizarClienteRequest request);
    Task<bool> EliminarAsync(int id);
    Task<bool> ActualizarPreciosAsync(int id, List<PrecioClienteDto> precios);
}
