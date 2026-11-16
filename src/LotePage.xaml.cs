using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CamScanner.Models;
using CamScanner.Services;
using CamScanner.Data;
using Microsoft.Extensions.DependencyInjection;

namespace CamScanner;

public partial class LotePage : ContentPage
{
    private readonly Lote _lote;
    private readonly IEnumerable<Documento> _origen;
    private readonly Func<Documento, Task> _abrirDetalle;
    private readonly CamScannerDatabase _database =
        IPlatformApplication.Current!.Services.GetRequiredService<CamScannerDatabase>();
    private readonly NotifyCollectionChangedEventHandler? _alCambiarOrigen;
    private bool _reordenando;
    private bool _abriendoDetalle;
    private bool _exportando;
    private Documento? _documentoArrastrado;

    public ObservableCollection<Documento> Fotos { get; } = new();

    private string _resumen = string.Empty;
    public string Resumen
    {
        get => _resumen;
        private set { _resumen = value; OnPropertyChanged(); }
    }

    public string Descripcion => string.IsNullOrWhiteSpace(_lote.Descripcion)
        ? "Sin descripción"
        : _lote.Descripcion;

    public LotePage(Lote lote, IEnumerable<Documento> origen, Func<Documento, Task> abrirDetalle)
    {
        InitializeComponent();
        _lote = lote;
        _origen = origen;
        _abrirDetalle = abrirDetalle;

        Title = lote.Nombre;
        BindingContext = this;

        // MainPage agrega, quita y reinserta (renombrar) documentos en esta colección.
        // Escuchar sus cambios mantiene la vista al día sin importar cuándo termine el borrado.
        if (origen is INotifyCollectionChanged notificador)
        {
            _alCambiarOrigen = (_, _) => MainThread.BeginInvokeOnMainThread(Recargar);
            notificador.CollectionChanged += _alCambiarOrigen;
            Unloaded += (_, _) => notificador.CollectionChanged -= _alCambiarOrigen;
        }

        Recargar();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Recargar();
    }

    private void Recargar()
    {
        var docs = _origen
            .Where(d => d.LoteId == _lote.Id)
            .OrderBy(d => d.Orden)
            .ToList();

        Fotos.Clear();
        foreach (var doc in docs)
            Fotos.Add(doc);

        Resumen = $"{docs.Count} {(docs.Count == 1 ? "foto" : "fotos")} · creado {_lote.FechaCreacion:dd/MM/yyyy}";
    }

    private async void OnFotoTapped(object? sender, TappedEventArgs e)
    {
        if (_abriendoDetalle || sender is not BindableObject { BindingContext: Documento doc })
            return;

        _abriendoDetalle = true;
        try
        {
            await _abrirDetalle(doc);
        }
        finally
        {
            _abriendoDetalle = false;
        }
    }

    private void OnFotoDragStarting(object? sender, DragStartingEventArgs e)
    {
        if (sender is BindableObject { BindingContext: Documento documento })
            _documentoArrastrado = documento;
    }

    private async void OnFotoDropped(object? sender, DropEventArgs e)
    {
        if (_reordenando || _documentoArrastrado is null ||
            sender is not BindableObject { BindingContext: Documento destinoDocumento })
            return;

        var origen = Fotos.IndexOf(_documentoArrastrado);
        var destino = Fotos.IndexOf(destinoDocumento);
        _documentoArrastrado = null;

        if (origen < 0 || destino < 0 || origen == destino)
            return;

        _reordenando = true;
        try
        {
            var documento = Fotos[origen];
            Fotos.RemoveAt(origen);
            Fotos.Insert(destino, documento);
            await _database.ReordenarDocumentosAsync(_lote.Id, Fotos);
        }
        catch (Exception ex)
        {
            Recargar();
            await DisplayAlertAsync("Error", $"No se pudo guardar el orden: {ex.Message}", "OK");
        }
        finally
        {
            _reordenando = false;
        }
    }

    private async void OnExportarExcelClicked(object? sender, EventArgs e)
    {
        if (_exportando)
            return;

        _exportando = true;
        try
        {
            if (Fotos.Count == 0)
            {
                await DisplayAlertAsync(
                    "Sin documentos",
                    "Este lote no tiene documentos para exportar.",
                    "OK");
                return;
            }

            var servicio = new ExcelExportService();
            var contenido = servicio.ExportarLote(_lote, Fotos.ToList());

            var ruta = Path.Combine(
                FileSystem.CacheDirectory,
                $"CamScanner_Lote_{_lote.Id}_{Guid.NewGuid():N}.xlsx");

            await File.WriteAllBytesAsync(ruta, contenido);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Exportar lote {_lote.Nombre}",
                File = new ShareFile(
                    ruta,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error al exportar",
                $"No se pudo exportar el lote: {ex.Message}",
                "OK");
        }
        finally
        {
            _exportando = false;
        }
    }

    private async void OnGuardarExcelClicked(object? sender, EventArgs e)
    {
        if (_exportando)
            return;

        _exportando = true;
        try
        {
            if (Fotos.Count == 0)
            {
                await DisplayAlertAsync("Sin documentos", "Este lote no tiene documentos para guardar.", "OK");
                return;
            }

            var contenido = new ExcelExportService().ExportarLote(_lote, Fotos.ToList());
            var guardado = await ArchivoService.GuardarAsync(
                contenido,
                $"CamScanner_Lote_{_lote.Id}.xlsx");

            if (guardado)
                await DisplayAlertAsync("Guardado", "El archivo Excel se guardó correctamente.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error al guardar", $"No se pudo guardar el Excel: {ex.Message}", "OK");
        }
        finally
        {
            _exportando = false;
        }
    }
}