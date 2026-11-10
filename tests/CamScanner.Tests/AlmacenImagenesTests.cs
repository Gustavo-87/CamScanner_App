using System.Text;
using CamScanner.Services;

namespace CamScanner.Tests;

public class AlmacenImagenesTests : IDisposable
{
    private readonly string _carpeta;
    private readonly AlmacenImagenes _almacen;

    public AlmacenImagenesTests()
    {
        // Carpeta temporal propia de cada prueba: no toca datos reales
        _carpeta = Path.Combine(Path.GetTempPath(), "camscanner-tests", Guid.NewGuid().ToString());
        _almacen = new AlmacenImagenes(_carpeta);
    }

    public void Dispose()
    {
        if (Directory.Exists(_carpeta))
            Directory.Delete(_carpeta, recursive: true);
    }

    private static MemoryStream Contenido(string texto) => new(Encoding.UTF8.GetBytes(texto));

    [Fact]
    public async Task GuardarAsync_CopiaElContenido()
    {
        using var origen = Contenido("contenido-de-prueba");

        var ruta = await _almacen.GuardarAsync(origen, "foto.jpg");

        Assert.True(File.Exists(ruta));
        Assert.Equal("contenido-de-prueba", await File.ReadAllTextAsync(ruta));
    }

    [Theory]
    [InlineData("foto.png", ".png")]
    [InlineData("FOTO.JPEG", ".jpeg")]
    [InlineData("scan.heic", ".heic")]
    [InlineData("virus.exe", ".jpg")]
    [InlineData("sin_extension", ".jpg")]
    [InlineData(null, ".jpg")]
    public async Task GuardarAsync_ConservaLaExtensionConocida_oUsaJpg(string? nombreOriginal, string extensionEsperada)
    {
        using var origen = Contenido("x");

        var ruta = await _almacen.GuardarAsync(origen, nombreOriginal);

        Assert.Equal(extensionEsperada, Path.GetExtension(ruta));
    }

    [Fact]
    public async Task GuardarAsync_CreaLaCarpetaSiNoExiste()
    {
        Assert.False(Directory.Exists(_carpeta));
        using var origen = Contenido("x");

        await _almacen.GuardarAsync(origen, "foto.jpg");

        Assert.True(Directory.Exists(_carpeta));
    }

    [Fact]
    public async Task GuardarAsync_DosVeces_GeneraRutasDistintas()
    {
        using var a = Contenido("a");
        using var b = Contenido("b");

        var ruta1 = await _almacen.GuardarAsync(a, "foto.jpg");
        var ruta2 = await _almacen.GuardarAsync(b, "foto.jpg");

        Assert.NotEqual(ruta1, ruta2);
    }

    [Fact]
    public async Task GuardarAsync_FalloAlCopiar_PropagaLaExcepcion_yNoDejaArchivosAMedias()
    {
        using var origen = new StreamQueFalla();

        await Assert.ThrowsAsync<IOException>(() => _almacen.GuardarAsync(origen, "foto.jpg"));

        Assert.Empty(Directory.GetFiles(_carpeta));
    }

    [Fact]
    public async Task Eliminar_ArchivoExistente_LoBorra()
    {
        using var origen = Contenido("x");
        var ruta = await _almacen.GuardarAsync(origen, "foto.jpg");

        var borrado = _almacen.Eliminar(ruta);

        Assert.True(borrado);
        Assert.False(File.Exists(ruta));
    }

    [Fact]
    public void Eliminar_ArchivoInexistente_DevuelveFalse_SinLanzar()
    {
        var borrado = _almacen.Eliminar(Path.Combine(_carpeta, "no-existe.jpg"));

        Assert.False(borrado);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Eliminar_RutaVacia_DevuelveFalse_SinLanzar(string? ruta)
    {
        Assert.False(_almacen.Eliminar(ruta));
    }

    /// <summary>Simula una lectura que falla (por ejemplo, la foto se corrompe o se corta el acceso).</summary>
    private sealed class StreamQueFalla : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new IOException("Fallo simulado de lectura");
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => throw new IOException("Fallo simulado de lectura");
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
