namespace ForraControl.API.Common;

// Resuelve la carpeta raíz de imágenes subidas. En Railway, Uploads:Path
// (env var Uploads__Path) debe apuntar a un Volume montado (ej. /data/uploads)
// para que sobreviva reinicios/redeploys — el disco del contenedor es efímero.
public static class UploadPaths
{
    public static string GetRoot(IConfiguration configuration)
    {
        var configured = configuration["Uploads:Path"] ?? "uploads";
        return Path.GetFullPath(configured);
    }
}
