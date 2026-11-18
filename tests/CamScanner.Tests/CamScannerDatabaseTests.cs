using CamScanner.Data;
using CamScanner.Models;

namespace CamScanner.Tests;

public sealed class CamScannerDatabaseTests : IDisposable
{
    private readonly string _ruta;
    private readonly CamScannerDatabase _database;

    public CamScannerDatabaseTests()
    {
        _ruta = Path.Combine(
            Path.GetTempPath(),
            "camscanner-tests",
            $"{Guid.NewGuid():N}.db3");
        Directory.CreateDirectory(Path.GetDirectoryName(_ruta)!);
        _database = new CamScannerDatabase(_ruta);
    }

    public void Dispose()
    {
        foreach (var ruta in new[] { _ruta, $"{_ruta}-wal", $"{_ruta}-shm" })
        {
            try
            {
                if (File.Exists(ruta))
                    File.Delete(ruta);
            }
            catch (IOException)
            {
            }
        }
    }

    [Fact]
    public async Task GuardarLoteYDocumento_ConsultaLosRegistrosRelacionados()
    {
        var lote = await CrearLoteAsync("Lote de prueba");
        var documento = new Documento
        {
            Nombre = "Documento 1",
            LoteId = lote.Id,
            RutaImagen = "imagen.jpg"
        };

        await _database.GuardarDocumentoAsync(documento);

        var lotes = await _database.ObtenerLotesAsync();
        var documentos = await _database.ObtenerDocumentosAsync();

        Assert.Contains(lotes, item => item.Id == lote.Id && item.Nombre == "Lote de prueba");
        var guardado = Assert.Single(documentos);
        Assert.Equal(lote.Id, guardado.LoteId);
        Assert.Equal(0, guardado.Orden);
    }

    [Fact]
    public async Task GuardarDocumento_AsignaOrdenConsecutivoPorLote()
    {
        var lote = await CrearLoteAsync("Lote");

        await _database.GuardarDocumentoAsync(CrearDocumento(lote.Id, "Primero"));
        await _database.GuardarDocumentoAsync(CrearDocumento(lote.Id, "Segundo"));

        var documentos = await _database.ObtenerDocumentosPorLoteAsync(lote.Id);

        Assert.Equal(2, documentos.Count);
        Assert.Equal(new[] { 0, 1 }, documentos.OrderBy(d => d.Orden).Select(d => d.Orden));
    }

    [Fact]
    public async Task ActualizarDocumento_PuedeMoverloEntreLotes()
    {
        var origen = await CrearLoteAsync("Origen");
        var destino = await CrearLoteAsync("Destino");
        var movido = CrearDocumento(origen.Id, "Movido");
        var existente = CrearDocumento(destino.Id, "Existente");

        await _database.GuardarDocumentoAsync(movido);
        await _database.GuardarDocumentoAsync(existente);
        await _database.ActualizarDocumentoAvanzadoAsync(
            movido.Id, movido.Id, destino.Id, 0, movido.Nombre);
        await _database.NormalizarOrdenesDocumentosAsync();

        var documentosOrigen = await _database.ObtenerDocumentosPorLoteAsync(origen.Id);
        var documentosDestino = (await _database.ObtenerDocumentosPorLoteAsync(destino.Id))
            .OrderBy(d => d.Orden)
            .ToList();

        Assert.Empty(documentosOrigen);
        Assert.Equal(new[] { "Movido", "Existente" }, documentosDestino.Select(d => d.Nombre));
        Assert.Equal(new[] { 0, 1 }, documentosDestino.Select(d => d.Orden));
    }

    [Fact]
    public async Task NormalizarOrdenes_CorrigeNegativosDuplicadosYHuecos()
    {
        var lote = await CrearLoteAsync("Lote");
        var documentos = new[]
        {
            CrearDocumento(lote.Id, "A"),
            CrearDocumento(lote.Id, "B"),
            CrearDocumento(lote.Id, "C")
        };

        foreach (var documento in documentos)
            await _database.GuardarDocumentoAsync(documento);

        await _database.ActualizarDocumentoAvanzadoAsync(
            documentos[0].Id, documentos[0].Id, lote.Id, -5, documentos[0].Nombre);
        await _database.ActualizarDocumentoAvanzadoAsync(
            documentos[1].Id, documentos[1].Id, lote.Id, 8, documentos[1].Nombre);
        await _database.ActualizarDocumentoAvanzadoAsync(
            documentos[2].Id, documentos[2].Id, lote.Id, 8, documentos[2].Nombre);

        await _database.NormalizarOrdenesDocumentosAsync();

        var resultado = (await _database.ObtenerDocumentosPorLoteAsync(lote.Id))
            .OrderBy(d => d.Orden)
            .ToList();

        Assert.Equal(new[] { 0, 1, 2 }, resultado.Select(d => d.Orden));
        Assert.Equal(new[] { "A", "B", "C" }, resultado.Select(d => d.Nombre));
    }

    [Fact]
    public async Task EliminarDocumento_DejaDisponibleElLote()
    {
        var lote = await CrearLoteAsync("Lote");
        var documento = CrearDocumento(lote.Id, "Documento");
        await _database.GuardarDocumentoAsync(documento);

        await _database.EliminarDocumentoAsync(documento);

        Assert.Empty(await _database.ObtenerDocumentosPorLoteAsync(lote.Id));
        Assert.True(await _database.EliminarLoteAsync(lote));
    }

    [Fact]
    public async Task EliminarLoteConDocumentos_ConservaElLote()
    {
        var lote = await CrearLoteAsync("Lote");
        await _database.GuardarDocumentoAsync(CrearDocumento(lote.Id, "Documento"));

        var eliminado = await _database.EliminarLoteAsync(lote);

        Assert.False(eliminado);
        Assert.Contains((await _database.ObtenerLotesAsync()), item => item.Id == lote.Id);
    }

    [Fact]
    public async Task GuardarDocumento_LoteInexistente_LanzaExcepcion()
    {
        var documento = CrearDocumento(99999, "Documento");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _database.GuardarDocumentoAsync(documento));
    }

    [Fact]
    public async Task ActualizarDocumento_IdDuplicado_LanzaExcepcion()
    {
        var lote = await CrearLoteAsync("Lote");
        var primero = CrearDocumento(lote.Id, "Primero");
        var segundo = CrearDocumento(lote.Id, "Segundo");
        await _database.GuardarDocumentoAsync(primero);
        await _database.GuardarDocumentoAsync(segundo);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _database.ActualizarDocumentoAvanzadoAsync(
                segundo.Id, primero.Id, lote.Id, segundo.Orden, segundo.Nombre));
    }

    [Fact]
    public async Task GuardarLote_NombreDuplicado_IgnoraMayusculasYEspacios()
    {
        await CrearLoteAsync("Lote principal");

        var duplicado = new Lote { Nombre = "  LOTE PRINCIPAL  " };

        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _database.GuardarLoteAsync(duplicado));

        Assert.Equal("Ya existe un lote con ese nombre.", excepcion.Message);
    }

    [Fact]
    public async Task GuardarLote_NombreValido_RecortaEspacios()
    {
        var lote = new Lote { Nombre = "  Lote nuevo  " };

        await _database.GuardarLoteAsync(lote);

        Assert.Equal("Lote nuevo", (await _database.ObtenerLotesAsync()).Single().Nombre);
    }

    [Fact]
    public async Task GuardarDocumento_NombreDuplicadoSoloSeRechazaEnElMismoLote()
    {
        var loteA = await CrearLoteAsync("Lote A");
        var loteB = await CrearLoteAsync("Lote B");
        await _database.GuardarDocumentoAsync(CrearDocumento(loteA.Id, "Factura"));

        var duplicado = CrearDocumento(loteA.Id, " factura ");
        var permitido = CrearDocumento(loteB.Id, "Factura");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _database.GuardarDocumentoAsync(duplicado));
        await _database.GuardarDocumentoAsync(permitido);

        Assert.Single(await _database.ObtenerDocumentosPorLoteAsync(loteB.Id));
    }

    [Fact]
    public async Task ActualizarDocumento_NombreDuplicadoEnDestino_LanzaExcepcion()
    {
        var origen = await CrearLoteAsync("Origen");
        var destino = await CrearLoteAsync("Destino");
        var movido = CrearDocumento(origen.Id, "Factura");
        var existente = CrearDocumento(destino.Id, "Factura");
        await _database.GuardarDocumentoAsync(movido);
        await _database.GuardarDocumentoAsync(existente);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _database.ActualizarDocumentoAvanzadoAsync(
                movido.Id, movido.Id, destino.Id, 0, " factura "));

        Assert.Equal(origen.Id, (await _database.ObtenerDocumentosPorLoteAsync(origen.Id)).Single().LoteId);
    }

    [Fact]
    public async Task ActualizarLote_PuedeConservarSuPropioNombre()
    {
        var lote = await CrearLoteAsync("Lote");

        await _database.ActualizarLoteAvanzadoAsync(
            lote.Id, lote.Id, "  LOTE  ", "Descripción");

        Assert.Equal("LOTE", (await _database.ObtenerLotesAsync()).Single().Nombre);
    }

    private async Task<Lote> CrearLoteAsync(string nombre)
    {
        var lote = new Lote { Nombre = nombre, FechaCreacion = DateTime.Now };
        await _database.GuardarLoteAsync(lote);
        return lote;
    }

    private static Documento CrearDocumento(int loteId, string nombre) => new()
    {
        LoteId = loteId,
        Nombre = nombre,
        RutaImagen = $"{nombre}.jpg",
        FechaCreacion = DateTime.Now
    };
}
