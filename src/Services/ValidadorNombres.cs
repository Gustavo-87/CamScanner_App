namespace CamScanner.Services;

public record ResultadoValidacion(bool EsValido, IReadOnlyList<string> Errores);

/// <summary>
/// Reglas de nombres de documentos. No depende de la interfaz ni de MAUI,
/// por eso se puede probar de forma automatizada (RF-03, RF-06, RF-07).
/// </summary>
public static class ValidadorNombres
{
    public static string NombreInicial(int consecutivo) => $"Documento {Math.Max(consecutivo, 1)}";

    /// <summary>Primer nombre "Documento N" que no esté en uso (sin importar mayúsculas ni espacios).</summary>
    public static string SiguienteNombre(IEnumerable<string?> existentes)
    {
        var ocupados = new HashSet<string>(
            existentes.Where(NombreEsValido).Select(n => Normalizar(n)),
            StringComparer.OrdinalIgnoreCase);

        var consecutivo = ocupados.Count + 1;
        while (ocupados.Contains(NombreInicial(consecutivo)))
            consecutivo++;

        return NombreInicial(consecutivo);
    }

    public static bool NombreEsValido(string? nombre) => !string.IsNullOrWhiteSpace(nombre);

    public static string Normalizar(string? nombre) => (nombre ?? string.Empty).Trim();

    /// <summary>Valida la lista completa antes de exportar: no vacía, sin nombres vacíos ni repetidos.</summary>
    public static ResultadoValidacion ValidarLista(IEnumerable<string?> nombres)
    {
        var lista = nombres.ToList();
        var errores = new List<string>();

        if (lista.Count == 0)
            errores.Add("La lista de imágenes está vacía.");

        if (lista.Any(n => !NombreEsValido(n)))
            errores.Add("Hay imágenes sin nombre.");

        var repetidos = lista
            .Where(NombreEsValido)
            .Select(n => Normalizar(n))
            .GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (repetidos.Count > 0)
            errores.Add($"Nombres repetidos: {string.Join(", ", repetidos)}.");

        return new ResultadoValidacion(errores.Count == 0, errores);
    }
}
