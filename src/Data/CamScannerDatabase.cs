using SQLite;
using CamScanner.Models;

namespace CamScanner.Data;

public class CamScannerDatabase
{
    private SQLiteAsyncConnection? _database;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private readonly string? _databasePath;

    public CamScannerDatabase(string? databasePath = null)
    {
        _databasePath = databasePath;
    }

    private async Task InitAsync()
    {
        if (_database != null)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_database != null)
                return;

            string dbPath = _databasePath
                ?? Path.Combine(FileSystem.AppDataDirectory, "camscanner.db3");
            var database = new SQLiteAsyncConnection(dbPath);

            await database.CreateTableAsync<Lote>();
            await database.CreateTableAsync<Documento>();

            try
            {
                await database.ExecuteAsync("ALTER TABLE Documento ADD COLUMN Orden INTEGER NOT NULL DEFAULT 0");
            }
            catch (SQLiteException ex) when (ex.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase))
            {
            }

            _database = database;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<(List<Lote> Items, int Total)> ObtenerLotesPaginaAsync(
        string? busqueda, int pagina, int tamanoPagina)
    {
        await InitAsync();
        var filtro = (busqueda ?? string.Empty).Trim();
        var like = $"%{filtro}%";
        var offset = Math.Max(0, pagina) * tamanoPagina;

        var total = await _database!.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Lote WHERE ? = '' OR Nombre LIKE ? OR COALESCE(Descripcion, '') LIKE ?",
            filtro, like, like);
        var items = await _database.QueryAsync<Lote>(
            "SELECT * FROM Lote WHERE ? = '' OR Nombre LIKE ? OR COALESCE(Descripcion, '') LIKE ? " +
            "ORDER BY FechaCreacion DESC LIMIT ? OFFSET ?",
            filtro, like, like, tamanoPagina, offset);

        return (items, total);
    }

    public async Task<(List<Documento> Items, int Total)> ObtenerDocumentosPaginaAsync(
        string? busqueda, int pagina, int tamanoPagina)
    {
        await InitAsync();
        var filtro = (busqueda ?? string.Empty).Trim();
        var like = $"%{filtro}%";
        var offset = Math.Max(0, pagina) * tamanoPagina;

        const string where = "? = '' OR d.Nombre LIKE ? OR " +
            "d.LoteId IN (SELECT Id FROM Lote WHERE Nombre LIKE ?)";
        var total = await _database!.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM Documento d WHERE {where}", filtro, like, like);
        var items = await _database.QueryAsync<Documento>(
            $"SELECT d.* FROM Documento d WHERE {where} " +
            "ORDER BY d.Orden, d.Id LIMIT ? OFFSET ?",
            filtro, like, like, tamanoPagina, offset);

        return (items, total);
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
        ArgumentNullException.ThrowIfNull(lote);

        lote.Nombre = ValidarNombre(lote.Nombre, "lote");
        if (await ExisteLoteConNombreAsync(lote.Nombre, lote.Id == 0 ? null : lote.Id))
            throw new InvalidOperationException("Ya existe un lote con ese nombre.");

        return lote.Id == 0
            ? await _database!.InsertAsync(lote)
            : await _database!.UpdateAsync(lote);
    }

    public async Task<int> GuardarDocumentoAsync(Documento documento)
    {
        await InitAsync();
        ArgumentNullException.ThrowIfNull(documento);

        var loteExiste = await _database!.Table<Lote>()
            .Where(lote => lote.Id == documento.LoteId)
            .FirstOrDefaultAsync();

        if (loteExiste is null)
            throw new InvalidOperationException("El lote seleccionado ya no existe.");

        documento.Nombre = ValidarNombre(documento.Nombre, "documento");
        if (await ExisteDocumentoConNombreAsync(
                documento.LoteId,
                documento.Nombre,
                documento.Id == 0 ? null : documento.Id))
            throw new InvalidOperationException(
                "Ya existe un documento con ese nombre en el lote seleccionado.");

        if (documento.Id == 0)
        {
            if (documento.Orden < 0)
                documento.Orden = 0;

            var ultimo = await _database!.Table<Documento>()
                .Where(d => d.LoteId == documento.LoteId)
                .OrderByDescending(d => d.Orden)
                .FirstOrDefaultAsync();

            documento.Orden = ultimo is null ? 0 : ultimo.Orden + 1;
            return await _database.InsertAsync(documento);
        }

        return await _database!.UpdateAsync(documento);
    }

    public async Task<List<Documento>> ObtenerDocumentosAsync()
    {
        await InitAsync();
        return await _database!.Table<Documento>()
            .OrderBy(documento => documento.Orden)
            .ToListAsync();
    }

    public async Task ReordenarDocumentosAsync(int loteId, IReadOnlyList<Documento> documentos)
    {
        await InitAsync();

        for (var indice = 0; indice < documentos.Count; indice++)
        {
            var documento = documentos[indice];
            documento.Orden = indice;
            await _database!.ExecuteAsync(
                "UPDATE Documento SET Orden = ? WHERE Id = ? AND LoteId = ?",
                indice, documento.Id, loteId);
        }
    }

    public async Task<bool> ExisteLoteConIdAsync(int id, int? excluirId = null)
    {
        await InitAsync();
        var lote = await _database!.Table<Lote>().Where(l => l.Id == id).FirstOrDefaultAsync();
        return lote is not null && lote.Id != excluirId;
    }

    public async Task<bool> ExisteDocumentoConIdAsync(int id, int? excluirId = null)
    {
        await InitAsync();
        var documento = await _database!.Table<Documento>().Where(d => d.Id == id).FirstOrDefaultAsync();
        return documento is not null && documento.Id != excluirId;
    }

    public async Task ActualizarLoteAvanzadoAsync(int idOriginal, int nuevoId, string nombre, string? descripcion)
    {
        await InitAsync();
        nombre = ValidarNombre(nombre, "lote");

        if (nuevoId <= 0 || await ExisteLoteConIdAsync(nuevoId, idOriginal))
            throw new InvalidOperationException("El ID del lote ya existe o no es válido.");

        if (await ExisteLoteConNombreAsync(nombre, idOriginal))
            throw new InvalidOperationException("Ya existe un lote con ese nombre.");

        if (idOriginal != nuevoId)
        {
            await _database!.ExecuteAsync("UPDATE Lote SET Id = ?, Nombre = ?, Descripcion = ? WHERE Id = ?",
                nuevoId, nombre, descripcion, idOriginal);
            await _database.ExecuteAsync("UPDATE Documento SET LoteId = ? WHERE LoteId = ?",
                nuevoId, idOriginal);
        }
        else
        {
            await _database!.ExecuteAsync("UPDATE Lote SET Nombre = ?, Descripcion = ? WHERE Id = ?",
                nombre, descripcion, idOriginal);
        }
    }

    public async Task ActualizarDocumentoAvanzadoAsync(
        int idOriginal, int nuevoId, int loteId, int orden, string nombre)
    {
        await InitAsync();
        if (nuevoId <= 0 || await ExisteDocumentoConIdAsync(nuevoId, idOriginal))
            throw new InvalidOperationException("El ID del documento ya existe o no es válido.");

        if (!await ExisteLoteConIdAsync(loteId))
            throw new InvalidOperationException("El lote indicado no existe.");

        nombre = ValidarNombre(nombre, "documento");
        if (await ExisteDocumentoConNombreAsync(loteId, nombre, idOriginal))
            throw new InvalidOperationException(
                "Ya existe un documento con ese nombre en el lote seleccionado.");

        await _database!.ExecuteAsync(
            "UPDATE Documento SET Id = ?, LoteId = ?, Orden = ?, Nombre = ? WHERE Id = ?",
            nuevoId, loteId, Math.Max(0, orden), nombre, idOriginal);
    }

    private async Task<bool> ExisteLoteConNombreAsync(string nombre, int? excluirId)
    {
        var lotes = await _database!.Table<Lote>().ToListAsync();
        return lotes.Any(lote => lote.Id != excluirId &&
            string.Equals(lote.Nombre.Trim(), nombre, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> ExisteDocumentoConNombreAsync(
        int loteId, string nombre, int? excluirId)
    {
        var documentos = await _database!.Table<Documento>()
            .Where(documento => documento.LoteId == loteId)
            .ToListAsync();

        return documentos.Any(documento => documento.Id != excluirId &&
            string.Equals(documento.Nombre.Trim(), nombre, StringComparison.OrdinalIgnoreCase));
    }

    private static string ValidarNombre(string? nombre, string tipo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new InvalidOperationException($"El nombre del {tipo} no puede estar vacío.");

        return nombre.Trim();
    }

    public async Task NormalizarOrdenesDocumentosAsync()
    {
        await InitAsync();

        var lotes = await _database!.Table<Lote>().ToListAsync();
        foreach (var lote in lotes)
        {
            var documentos = await _database.Table<Documento>()
                .Where(d => d.LoteId == lote.Id)
                .OrderBy(d => d.Orden)
                .ThenBy(d => d.FechaCreacion)
                .ThenBy(d => d.Id)
                .ToListAsync();

            if (documentos.Count == 0)
                continue;

            for (var indice = 0; indice < documentos.Count; indice++)
            {
                documentos[indice].Orden = indice;
                await _database.ExecuteAsync(
                    "UPDATE Documento SET Orden = ? WHERE Id = ?",
                    indice, documentos[indice].Id);
            }
        }
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
