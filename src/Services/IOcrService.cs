namespace CamScanner;

public interface IOcrService
{
    /// <summary>Inicializa el motor si la plataforma lo requiere.</summary>
    Task InitAsync();

    /// <summary>Procesa una imagen desde su ruta local y extrae el texto.</summary>
    Task<string?> RecognizeTextAsync(string rutaImagen);
}
