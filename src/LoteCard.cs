using CamScanner.Models;

namespace CamScanner;

/// <summary>Lote + datos de la vista previa (no toca el modelo ni la base de datos).</summary>
public sealed class LoteCard
{
    public LoteCard(Lote lote, IReadOnlyList<Documento> docsDelLote)
    {
        Lote = lote;

        // Las 3 más recientes, ordenadas de atrás hacia adelante: la más nueva queda al frente
        var fotos = docsDelLote
            .OrderByDescending(d => d.FechaCreacion)
            .Take(3)
            .Reverse()
            .Select(d => d.RutaImagen)
            .ToList();

        Foto1 = fotos.ElementAtOrDefault(0); // fondo
        Foto2 = fotos.ElementAtOrDefault(1); // medio
        Foto3 = fotos.ElementAtOrDefault(2); // frente

        Cantidad = docsDelLote.Count switch
        {
            0 => "Vacío",
            1 => "1 foto",
            var n => $"{n} fotos"
        };
    }

    public Lote Lote { get; }
    public string Nombre => Lote.Nombre;
    public DateTime FechaCreacion => Lote.FechaCreacion;
    public string Cantidad { get; }

    public string? Foto1 { get; }
    public string? Foto2 { get; }
    public string? Foto3 { get; }

    public bool HayFoto1 => Foto1 is not null;
    public bool HayFoto2 => Foto2 is not null;
    public bool HayFoto3 => Foto3 is not null;
    public bool Vacio => Foto1 is null;
}