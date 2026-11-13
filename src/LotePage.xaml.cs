using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CamScanner.Models;

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
}