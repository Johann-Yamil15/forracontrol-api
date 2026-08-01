using ForraControl.API.Data;
using ForraControl.API.Interfaces;
using ForraControl.API.Models.Dtos.Auth;
using Microsoft.EntityFrameworkCore;

namespace ForraControl.API.Services;

public class AuthService(ForraDbContext db) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(string username, string password)
    {
        var user = await db.Usuarios
            .FirstOrDefaultAsync(u => u.Username == username && u.Activo);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return new LoginResponse
        {
            IdUsuario = user.Id,
            Nombre = user.Nombre,
            Username = user.Username,
            Rol = user.Rol
        };
    }
}
