
using SQLite;

namespace CamScanner.Models;

public class Documento
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int LoteId { get; set; }

    public int Orden { get; set; }

    [NotNull]
    public string Nombre { get; set; } = string.Empty;

    public string RutaImagen { get; set; } = string.Empty;

    public string? TextoExtraido { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
