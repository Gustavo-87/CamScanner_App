namespace CamScanner.Services;

/// <summary>
/// Guarda y elimina las imágenes de los documentos en una carpeta.
/// No depende de MAUI: la carpeta se recibe por constructor, así se prueba con una carpeta temporal.
/// </summary>
public class AlmacenImagenes
{
    private static readonly HashSet<string> ExtensionesPermitidas =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".heic" };

    private readonly string _carpeta;

    public AlmacenImagenes(string carpeta)
    {
        _carpeta = carpeta ?? throw new ArgumentNullException(nameof(carpeta));
    }

    /// <summary>Conserva la extensión original si es de imagen conocida; si no, usa .jpg.</summary>
    public static string ExtensionSegura(string? nombreArchivo)
    {
        var extension = Path.GetExtension(nombreArchivo ?? string.Empty);
        return ExtensionesPermitidas.Contains(extension) ? extension.ToLowerInvariant() : ".jpg";
    }

    /// <summary>Copia el contenido a un archivo nuevo y devuelve su ruta. Si falla, no deja un archivo a medias.</summary>
    public async Task<string> GuardarAsync(Stream origen, string? nombreOriginal = null)
    {
        ArgumentNullException.ThrowIfNull(origen);

        Directory.CreateDirectory(_carpeta);
        var ruta = Path.Combine(_carpeta, $"{Guid.NewGuid()}{ExtensionSegura(nombreOriginal)}");

        try
        {
            await using var destino = File.Create(ruta);
            await origen.CopyToAsync(destino);
        }
        catch
        {
            Eliminar(ruta);
            throw;
        }

        return ruta;
    }

    /// <summary>Borra el archivo si existe. Devuelve false si no existía o no se pudo borrar (nunca lanza).</summary>
    public bool Eliminar(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
            return false;

        try
        {
            if (!File.Exists(ruta))
                return false;

            File.Delete(ruta);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
