using ClosedXML.Excel;
using CamScanner.Models;

namespace CamScanner.Services;

public class ExcelExportService
{
    public byte[] ExportarDocumento(Documento documento, string nombreLote)
    {
        ArgumentNullException.ThrowIfNull(documento);

        return ExportarLotes(new[]
        {
            (new Lote { Nombre = nombreLote }, (IEnumerable<Documento>)new[] { documento })
        });
    }

    public byte[] ExportarLote(Lote lote, IEnumerable<Documento> documentos)
    {
        ArgumentNullException.ThrowIfNull(lote);
        ArgumentNullException.ThrowIfNull(documentos);

        return ExportarLotes(new[] { (lote, documentos) });
    }

    public byte[] ExportarLotes(IEnumerable<(Lote Lote, IEnumerable<Documento> Documentos)> lotes)
    {
        ArgumentNullException.ThrowIfNull(lotes);

        using var libro = new XLWorkbook();
        var nombresUsados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var hayHojas = false;

        foreach (var (lote, documentos) in lotes)
        {
            var hoja = libro.Worksheets.Add(ObtenerNombreHoja(lote.Nombre, nombresUsados));
            hayHojas = true;

            hoja.Cell(1, 1).Value = "Lote";
            hoja.Cell(1, 2).Value = lote.Nombre;
            hoja.Cell(2, 1).Value = "Descripción";
            hoja.Cell(2, 2).Value = lote.Descripcion ?? "Sin descripción";
            hoja.Range(1, 1, 2, 1).Style.Font.Bold = true;

            var encabezados = new[]
            {
                "Índice", "ID original", "Documento", "Lote", "Fecha", "Texto OCR"
            };

            const int filaEncabezado = 4;
            for (int columna = 0; columna < encabezados.Length; columna++)
                hoja.Cell(filaEncabezado, columna + 1).Value = encabezados[columna];

            int fila = filaEncabezado + 1;
            int indice = 0;
            foreach (var documento in documentos.OrderBy(d => d.Orden))
            {
                hoja.Cell(fila, 1).Value = indice++;
                hoja.Cell(fila, 2).Value = documento.Id;
                hoja.Cell(fila, 3).Value = documento.Nombre;
                hoja.Cell(fila, 4).Value = lote.Nombre;
                hoja.Cell(fila, 5).Value = documento.FechaCreacion;
                hoja.Cell(fila, 6).Value = documento.TextoExtraido ?? "";

                fila++;
            }

            var encabezado = hoja.Range(filaEncabezado, 1, filaEncabezado, encabezados.Length);
            encabezado.Style.Font.Bold = true;
            encabezado.SetAutoFilter();
            hoja.Columns().AdjustToContents();
        }

        if (!hayHojas)
            throw new InvalidOperationException("No hay lotes para exportar.");

        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);

        return memoria.ToArray();
    }

    private static string ObtenerNombreHoja(string nombre, HashSet<string> nombresUsados)
    {
        var limpio = string.IsNullOrWhiteSpace(nombre) ? "Lote" : nombre.Trim();
        foreach (var caracter in new[] { ':', '\\', '/', '?', '*', '[', ']' })
            limpio = limpio.Replace(caracter, '_');

        limpio = limpio[..Math.Min(limpio.Length, 31)];
        var baseNombre = limpio;
        var sufijo = 2;
        while (!nombresUsados.Add(limpio))
        {
            var textoSufijo = $" ({sufijo++})";
            limpio = baseNombre[..Math.Min(baseNombre.Length, 31 - textoSufijo.Length)] + textoSufijo;
        }

        return limpio;
    }
}