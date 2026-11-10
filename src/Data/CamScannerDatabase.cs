
using SQLite;
using CamScanner.Models;

namespace CamScanner.Data;

public class CamScannerDatabase
{
    private SQLiteAsyncConnection? _database;

    private async Task InitAsync()
    {
        if (_database != null)
            return;

        string dbPath = Path.Combine(
            FileSystem.AppDataDirectory,
            "camscanner.db3"
        );

        var database = new SQLiteAsyncConnection(dbPath);

        await database.CreateTableAsync<Lote>();
        await database.CreateTableAsync<Documento>();

        _database = database;
    }

    public async Task<List<Lote>> ObtenerLotesAsync()
    {
        await InitAsync();
        return await _database!.Table<Lote>()
            .OrderByDescending(lote => lote.FechaCreacion)
            .ToListAsync();
    }

    public async Task<int> GuardarLoteAsync(Lote lote)
    {
        await InitAsync();

        return lote.Id == 0
            ? await _database!.InsertAsync(lote)
            : await _database!.UpdateAsync(lote);
    }

    public async Task<int> GuardarDocumentoAsync(Documento documento)
    {
        await InitAsync();

        var loteExiste = await _database!.Table<Lote>()
            .Where(lote => lote.Id == documento.LoteId)
            .FirstOrDefaultAsync();

        if (loteExiste is null)
            throw new InvalidOperationException("El lote seleccionado ya no existe.");

        return documento.Id == 0
            ? await _database!.InsertAsync(documento)
            : await _database!.UpdateAsync(documento);
    }

    public async Task<List<Documento>> ObtenerDocumentosAsync()
    {
        await InitAsync();
        return await _database!.Table<Documento>()
            .OrderByDescending(documento => documento.FechaCreacion)
            .ToListAsync();
    }

    public async Task<List<Documento>> ObtenerDocumentosPorLoteAsync(int loteId)
    {
        await InitAsync();
        return await _database!.Table<Documento>()
            .Where(documento => documento.LoteId == loteId)
            .OrderByDescending(documento => documento.FechaCreacion)
            .ToListAsync();
    }

    public async Task EliminarDocumentoAsync(Documento documento)
    {
        await InitAsync();
        await _database!.DeleteAsync(documento);
    }

    public async Task<bool> EliminarLoteAsync(Lote lote)
    {
        await InitAsync();

        var tieneDocumentos = await _database!.Table<Documento>()
            .Where(documento => documento.LoteId == lote.Id)
            .CountAsync() > 0;

        if (tieneDocumentos)
            return false;

        await _database!.DeleteAsync(lote);
        return true;
    }
}
