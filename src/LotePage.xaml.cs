using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CamScanner.Models;
using CamScanner.Services;

namespace CamScanner;

public partial class LotePage : ContentPage
{
    private readonly Lote _lote;
    private readonly IEnumerable<Documento> _origen;
    private readonly Func<Documento, Task> _abrirDetalle;
    private readonly NotifyCollectionChangedEventHandler? _alCambiarOrigen;

    public ObservableCollection<Documento> Fotos { get; } = new();

    private string _resumen = string.Empty;
    public string Resumen
    {
        get => _resumen;
        private set { _resumen = value; OnPropertyChanged(); }
    }

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
            .OrderByDescending(d => d.FechaCreacion)
            .ToList();

        Fotos.Clear();
        foreach (var doc in docs)
            Fotos.Add(doc);

        Resumen = $"{docs.Count} {(docs.Count == 1 ? "foto" : "fotos")} · creado {_lote.FechaCreacion:dd/MM/yyyy}";
    }

    private async void OnFotoTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: Documento doc })
            await _abrirDetalle(doc);
    }

    private async void OnExportarExcelClicked(object? sender, EventArgs e)
    {
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
    }
}