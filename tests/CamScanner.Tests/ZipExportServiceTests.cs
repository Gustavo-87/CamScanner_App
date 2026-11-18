using System.IO.Compression;
using CamScanner.Models;
using CamScanner.Services;

namespace CamScanner.Tests;

public sealed class ZipExportServiceTests : IDisposable
{
    private readonly string _directorio = Path.Combine(
        Path.GetTempPath(), "camscanner-zip-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task ExportarLotes_CreaExcelEImagenesOrganizadosPorLote()
    {
        Directory.CreateDirectory(_directorio);
        var imagen = Path.Combine(_directorio, "factura.jpg");
        await File.WriteAllBytesAsync(imagen, new byte[] { 1, 2, 3 });

        var lote = new Lote { Id = 1, Nombre = "Lote A" };
        var documento = new Documento
        {
            Id = 2,
            LoteId = lote.Id,
            Nombre = "Factura",
            RutaImagen = imagen
        };

        var archivo = await new ZipExportService().ExportarLotesAsync(
            new[] { (lote, (IEnumerable<Documento>)new[] { documento }) });

        using var zip = new ZipArchive(new MemoryStream(archivo), ZipArchiveMode.Read);

        Assert.Contains(zip.Entries, entrada => entrada.FullName == "CamScanner_Lotes.xlsx");
        Assert.Contains(zip.Entries, entrada => entrada.FullName == "Lote A/Factura.jpg");
    }

    [Fact]
    public async Task ExportarLotes_OmiteLotesSinDocumentos()
    {
        var loteVacio = new Lote { Nombre = "Vacío" };
        var loteConDocumentos = new Lote { Nombre = "Con documentos" };
        var documento = new Documento
        {
            Id = 1,
            LoteId = loteConDocumentos.Id,
            Nombre = "Documento",
            RutaImagen = Path.Combine(_directorio, "no-existe.jpg")
        };

        var archivo = await new ZipExportService().ExportarLotesAsync(new[]
        {
            (loteVacio, (IEnumerable<Documento>)Array.Empty<Documento>()),
            (loteConDocumentos, (IEnumerable<Documento>)new[] { documento })
        });

        using var zip = new ZipArchive(new MemoryStream(archivo), ZipArchiveMode.Read);

        Assert.DoesNotContain(zip.Entries, entrada => entrada.FullName.StartsWith("Vacío/"));
        Assert.Contains(zip.Entries, entrada => entrada.FullName == "CamScanner_Lotes.xlsx");
    }

    [Fact]
    public async Task ExportarLotes_SinDocumentos_LanzaExcepcion()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new ZipExportService().ExportarLotesAsync(new[]
            {
                (new Lote { Nombre = "Vacío" },
                    (IEnumerable<Documento>)Array.Empty<Documento>())
            }));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directorio))
            Directory.Delete(_directorio, recursive: true);
    }
}