namespace ForraControl.API.Models.Dtos.Auth;

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResponse
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = "";
    public string Username { get; set; } = "";
    public string Rol { get; set; } = "";
}
