using CommunityToolkit.Maui.Storage;

namespace CamScanner.Services;

public static class ArchivoService
{
    public static async Task<bool> GuardarAsync(
        byte[] contenido,
        string nombreArchivo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contenido);

        await using var stream = new MemoryStream(contenido, writable: false);
        var resultado = await FileSaver.Default.SaveAsync(
            nombreArchivo,
            stream,
            cancellationToken);

        if (!resultado.IsSuccessful && resultado.Exception is not null)
            throw resultado.Exception;

        return resultado.IsSuccessful;
    }
}