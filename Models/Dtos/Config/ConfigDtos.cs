namespace ForraControl.API.Models.Dtos.Config;

public class CrearCatalogoRequest
{
    public string Nombre { get; set; } = "";
}

public class CatalogoItemDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}
