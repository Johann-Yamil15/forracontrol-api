using ForraControl.API.Models.Dtos.Auth;

namespace ForraControl.API.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string username, string password);
}
