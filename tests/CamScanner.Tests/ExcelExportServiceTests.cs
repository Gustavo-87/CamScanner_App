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
        var hoja = libro.Worksheet("Lote A");

        Assert.Equal("Lote A", hoja.Cell(1, 2).GetString());
        Assert.Equal("Índice", hoja.Cell(4, 1).GetString());
        Assert.Equal("ID original", hoja.Cell(4, 2).GetString());
        Assert.Equal("Factura prueba", hoja.Cell(5, 3).GetString());
        Assert.Equal("Lote A", hoja.Cell(5, 4).GetString());
        Assert.Equal("Total: 50000", hoja.Cell(5, 6).GetString());
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
        var hoja = libro.Worksheet("Lote A");

        Assert.Equal("Documento 1", hoja.Cell(5, 3).GetString());
        Assert.Equal("Documento 2", hoja.Cell(6, 3).GetString());
        Assert.Equal(6, hoja.LastRowUsed()!.RowNumber());
    }

    [Fact]
    public void ExportarLote_OrdenaLosDocumentosPorOrden()
    {
        var documentos = new[]
        {
            new Documento { Id = 2, Nombre = "Segundo", Orden = 1 },
            new Documento { Id = 1, Nombre = "Primero", Orden = 0 }
        };

        var archivo = new ExcelExportService().ExportarLote(
            new Lote { Nombre = "Orden" }, documentos);

        using var libro = new XLWorkbook(new MemoryStream(archivo));
        var hoja = libro.Worksheet("Orden");

        Assert.Equal("Primero", hoja.Cell(5, 3).GetString());
        Assert.Equal("Segundo", hoja.Cell(6, 3).GetString());
        Assert.Equal(0, hoja.Cell(5, 1).GetValue<int>());
        Assert.Equal(1, hoja.Cell(6, 1).GetValue<int>());
    }

    [Fact]
    public void ExportarLotes_CreaUnaHojaPorLoteYUnificaNombresRepetidos()
    {
        var lotes = new[]
        {
            (new Lote { Nombre = "Lote" }, (IEnumerable<Documento>)Array.Empty<Documento>()),
            (new Lote { Nombre = "Lote" }, (IEnumerable<Documento>)Array.Empty<Documento>())
        };

        var archivo = new ExcelExportService().ExportarLotes(lotes);

        using var libro = new XLWorkbook(new MemoryStream(archivo));

        Assert.Equal(2, libro.Worksheets.Count);
        Assert.Equal("Lote", libro.Worksheet(1).Name);
        Assert.Equal("Lote (2)", libro.Worksheet(2).Name);
    }

    [Fact]
    public void ExportarLotes_SinLotes_LanzaExcepcion()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new ExcelExportService().ExportarLotes(Array.Empty<(Lote, IEnumerable<Documento>)>()));
    }

    [Fact]
    public void ExportarLotes_LimpiaNombresInvalidosYLimitaNombreDeHoja()
    {
        var archivo = new ExcelExportService().ExportarLote(
            new Lote { Nombre = "Lote/Con:Nombre*Invalido" },
            Array.Empty<Documento>());

        using var libro = new XLWorkbook(new MemoryStream(archivo));
        var nombreHoja = libro.Worksheet(1).Name;

        Assert.DoesNotContain("/", nombreHoja);
        Assert.DoesNotContain(":", nombreHoja);
        Assert.DoesNotContain("*", nombreHoja);
        Assert.True(nombreHoja.Length <= 31);
    }

    [Fact]
    public void ExportarDocumento_SinTextoOcr_DejaCeldaVacia()
    {
        var archivo = new ExcelExportService().ExportarDocumento(
            new Documento { Id = 4, Nombre = "Imagen" }, "Lote");

        using var libro = new XLWorkbook(new MemoryStream(archivo));

        Assert.Equal(string.Empty, libro.Worksheet("Lote").Cell(5, 6).GetString());
    }
}