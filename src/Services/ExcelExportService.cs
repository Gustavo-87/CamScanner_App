using ClosedXML.Excel;
using CamScanner.Models;

namespace CamScanner.Services;

public class ExcelExportService
{
    public byte[] ExportarDocumento(Documento documento, string nombreLote)
    {
        ArgumentNullException.ThrowIfNull(documento);

        return GenerarExcel(
            new[] { documento },
            nombreLote
        );
    }

    public byte[] ExportarLote(Lote lote, IEnumerable<Documento> documentos)
    {
        ArgumentNullException.ThrowIfNull(lote);
        ArgumentNullException.ThrowIfNull(documentos);

        return GenerarExcel(documentos, lote.Nombre);
    }

    private static byte[] GenerarExcel(
        IEnumerable<Documento> documentos,
        string nombreLote)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Documentos");

        var encabezados = new[]
        {
            "ID", "Documento", "Lote", "Fecha", "Texto OCR"
        };

        for (int columna = 0; columna < encabezados.Length; columna++)
            hoja.Cell(1, columna + 1).Value = encabezados[columna];

        int fila = 2;

        foreach (var documento in documentos)
        {
            hoja.Cell(fila, 1).Value = documento.Id;
            hoja.Cell(fila, 2).Value = documento.Nombre;
            hoja.Cell(fila, 3).Value = nombreLote;
            hoja.Cell(fila, 4).Value = documento.FechaCreacion;
            hoja.Cell(fila, 5).Value = documento.TextoExtraido ?? "";

            fila++;
        }

        var encabezado = hoja.Range(1, 1, 1, 5);
        encabezado.Style.Font.Bold = true;
        encabezado.SetAutoFilter();

        hoja.Columns().AdjustToContents();

        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);

        return memoria.ToArray();
    }
}