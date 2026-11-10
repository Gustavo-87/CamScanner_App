using CamScanner.Services;

namespace CamScanner.Tests;

public class ValidadorNombresTests
{
    [Fact]
    public void NombreInicial_UsaElConsecutivo()
    {
        Assert.Equal("Documento 1", ValidadorNombres.NombreInicial(1));
        Assert.Equal("Documento 7", ValidadorNombres.NombreInicial(7));
    }

    [Fact]
    public void NombreInicial_ConsecutivoInvalido_UsaUno()
    {
        Assert.Equal("Documento 1", ValidadorNombres.NombreInicial(0));
        Assert.Equal("Documento 1", ValidadorNombres.NombreInicial(-3));
    }

    [Fact]
    public void SiguienteNombre_ListaVacia_EsDocumento1()
    {
        Assert.Equal("Documento 1", ValidadorNombres.SiguienteNombre(Array.Empty<string?>()));
    }

    [Fact]
    public void SiguienteNombre_SaltaLosNombresOcupados()
    {
        // Había 1, 2 y 3; se eliminó el 2. Hay 2 elementos, pero "Documento 3" ya existe.
        var existentes = new string?[] { "Documento 1", "Documento 3" };

        Assert.Equal("Documento 4", ValidadorNombres.SiguienteNombre(existentes));
    }

    [Fact]
    public void SiguienteNombre_IgnoraMayusculasYEspacios()
    {
        var existentes = new string?[] { "  documento 2  " };

        Assert.Equal("Documento 3", ValidadorNombres.SiguienteNombre(existentes));
    }

    [Fact]
    public void ValidarLista_ListaVacia_NoEsValida()
    {
        var resultado = ValidadorNombres.ValidarLista(Array.Empty<string?>());

        Assert.False(resultado.EsValido);
        Assert.Contains(resultado.Errores, e => e.Contains("vacía"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidarLista_NombreVacio_NoEsValida(string? nombreVacio)
    {
        var resultado = ValidadorNombres.ValidarLista(new[] { "Factura", nombreVacio });

        Assert.False(resultado.EsValido);
        Assert.Contains(resultado.Errores, e => e.Contains("sin nombre"));
    }

    [Fact]
    public void ValidarLista_NombresRepetidos_NoEsValida_IgnorandoMayusculasYEspacios()
    {
        var resultado = ValidadorNombres.ValidarLista(new string?[] { "Factura", " factura ", "Recibo" });

        Assert.False(resultado.EsValido);
        Assert.Contains(resultado.Errores, e => e.Contains("repetidos"));
    }

    [Fact]
    public void ValidarLista_NombresUnicos_EsValida()
    {
        var resultado = ValidadorNombres.ValidarLista(new string?[] { "Factura", "Recibo", "Contrato" });

        Assert.True(resultado.EsValido);
        Assert.Empty(resultado.Errores);
    }

    [Theory]
    [InlineData("  Factura  ", "Factura")]
    [InlineData("Recibo", "Recibo")]
    [InlineData(null, "")]
    public void Normalizar_QuitaEspaciosYNulos(string? entrada, string esperado)
    {
        Assert.Equal(esperado, ValidadorNombres.Normalizar(entrada));
    }
}
