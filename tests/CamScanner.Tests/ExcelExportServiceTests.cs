using CamScanner.Models;
using CamScanner.Services;
using ClosedXML.Excel;

namespace CamScanner.Tests;

public class ExcelExportServiceTests
{
    [Fact]
    public void ExportarDocumento_GeneraExcelConDatosCorrectos()
    {
        var servicio = new ExcelExportService();
        var documento = new Documento
        {
            Id = 1,
            LoteId = 10,
            Nombre = "Factura prueba",
            TextoExtraido = "Total: 50000",
            FechaCreacion = new DateTime(2026, 11, 13)
        };

        var archivo = servicio.ExportarDocumento(documento, "Lote A");

        using var memoria = new MemoryStream(archivo);
        using var libro = new XLWorkbook(memoria);
        var hoja = libro.Worksheet("Documentos");

        Assert.Equal("Factura prueba", hoja.Cell(2, 2).GetString());
        Assert.Equal("Lote A", hoja.Cell(2, 3).GetString());
        Assert.Equal("Total: 50000", hoja.Cell(2, 5).GetString());
    }

    [Fact]
    public void ExportarLote_IncluyeTodosLosDocumentos()
    {
        var servicio = new ExcelExportService();
        var lote = new Lote { Id = 10, Nombre = "Lote A" };

        var documentos = new[]
        {
            new Documento { Id = 1, Nombre = "Documento 1", LoteId = 10 },
            new Documento { Id = 2, Nombre = "Documento 2", LoteId = 10 }
        };

        var archivo = servicio.ExportarLote(lote, documentos);

        using var memoria = new MemoryStream(archivo);
        using var libro = new XLWorkbook(memoria);
        var hoja = libro.Worksheet("Documentos");

        Assert.Equal("Documento 1", hoja.Cell(2, 2).GetString());
        Assert.Equal("Documento 2", hoja.Cell(3, 2).GetString());
        Assert.Equal(3, hoja.LastRowUsed()!.RowNumber());
    }
}