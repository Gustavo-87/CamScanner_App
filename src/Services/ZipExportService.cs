using System.IO.Compression;
using CamScanner.Models;

namespace CamScanner.Services;

public sealed class ZipExportService
{
    private readonly ExcelExportService _excel = new();

    public async Task<byte[]> ExportarLotesAsync(
        IEnumerable<(Lote Lote, IEnumerable<Documento> Documentos)> lotes)
    {
        ArgumentNullException.ThrowIfNull(lotes);

        var grupos = lotes
            .Select(grupo => (grupo.Lote, Documentos: grupo.Documentos.ToList()))
            .Where(grupo => grupo.Documentos.Count > 0)
            .ToList();

        if (grupos.Count == 0)
            throw new InvalidOperationException("No hay lotes con documentos para exportar.");

        var excel = _excel.ExportarLotes(grupos.Select(grupo =>
            (grupo.Lote, (IEnumerable<Documento>)grupo.Documentos)));

        using var memoria = new MemoryStream();
        using (var archivoZip = new ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entradaExcel = archivoZip.CreateEntry("CamScanner_Lotes.xlsx", CompressionLevel.Fastest);
            using (var destino = entradaExcel.Open())
                destino.Write(excel, 0, excel.Length);

            var nombresLote = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var grupo in grupos)
            {
                var carpetaLote = ObtenerNombreUnico(
                    LimpiarNombre(grupo.Lote.Nombre, "Lote"), nombresLote);
                var nombresImagen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var documento in grupo.Documentos.OrderBy(documento => documento.Orden))
                {
                    if (!File.Exists(documento.RutaImagen))
                        continue;

                    var extension = AlmacenImagenes.ExtensionSegura(documento.RutaImagen);
                    var nombreDocumento = string.IsNullOrWhiteSpace(documento.Nombre)
                        ? $"Documento_{documento.Id}"
                        : Path.GetFileNameWithoutExtension(documento.Nombre.Trim());
                    var nombreImagen = LimpiarNombre(
                        $"{nombreDocumento}{extension}",
                        $"Documento_{documento.Id}{extension}");
                    nombreImagen = ObtenerNombreUnico(nombreImagen, nombresImagen);
                    var entrada = archivoZip.CreateEntry(
                        $"{carpetaLote}/{nombreImagen}",
                        CompressionLevel.Fastest);

                    await using var origen = File.OpenRead(documento.RutaImagen);
                    await using var destino = entrada.Open();
                    await origen.CopyToAsync(destino);
                }
            }
        }

        return memoria.ToArray();
    }

    private static string LimpiarNombre(string? nombre, string predeterminado)
    {
        var limpio = string.IsNullOrWhiteSpace(nombre) ? predeterminado : nombre.Trim();
        foreach (var caracter in Path.GetInvalidFileNameChars())
            limpio = limpio.Replace(caracter, '_');
        return string.IsNullOrWhiteSpace(limpio) ? predeterminado : limpio;
    }

    private static string ObtenerNombreUnico(string nombre, HashSet<string> usados)
    {
        var baseNombre = nombre;
        var indice = 2;
        while (!usados.Add(nombre))
        {
            var extension = Path.GetExtension(baseNombre);
            var sinExtension = Path.GetFileNameWithoutExtension(baseNombre);
            nombre = $"{sinExtension} ({indice++}){extension}";
        }

        return nombre;
    }
}
