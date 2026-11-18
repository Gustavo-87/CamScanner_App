using System.Collections.ObjectModel;
using CamScanner.Data;
using CamScanner.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CamScanner;

public partial class DatabasePage : ContentPage
{
    private const int TamanoPagina = 50;
    private readonly CamScannerDatabase _database =
        IPlatformApplication.Current!.Services.GetRequiredService<CamScannerDatabase>();
    private readonly CancellationTokenSource _cierre = new();
    private CollectionView _tabla = null!;
    private SearchBar _buscador = null!;
    private Label _estado = null!;
    private Label _leyenda = null!;
    private Button _anterior = null!;
    private Button _siguiente = null!;
    private bool _modoDocumentos;
    private bool _cargando;
    private int _pagina;
    private int _total;

    public ObservableCollection<LoteRow> Lotes { get; } = new();
    public ObservableCollection<DocumentoRow> Documentos { get; } = new();
    public ObservableCollection<LoteOption> LotesOpciones { get; } = new();

    public DatabasePage()
    {
        BindingContext = this;
        Content = CrearContenido();
    }

    private View CrearContenido()
    {
        _buscador = new SearchBar { Placeholder = "Buscar por nombre, descripción o lote" };
        _buscador.TextChanged += OnBusquedaChanged;

        var lotesButton = new Button { Text = "Lotes" };
        var documentosButton = new Button { Text = "Documentos" };
        lotesButton.Clicked += (_, _) => CambiarModo(false);
        documentosButton.Clicked += (_, _) => CambiarModo(true);

        _estado = new Label { TextColor = Colors.Gray, VerticalOptions = LayoutOptions.Center };
        _anterior = new Button { Text = "Anterior", IsEnabled = false };
        _siguiente = new Button { Text = "Siguiente", IsEnabled = false };
        _anterior.Clicked += async (_, _) => await CambiarPaginaAsync(-1);
        _siguiente.Clicked += async (_, _) => await CambiarPaginaAsync(1);

        var navegacion = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)
            },
            ColumnSpacing = 8
        };
        navegacion.Add(_anterior, 0);
        navegacion.Add(_estado, 1);
        navegacion.Add(_siguiente, 2);

        var selectorModo = new HorizontalStackLayout { Spacing = 8, Children = { lotesButton, documentosButton } };
        selectorModo.SetValue(Grid.RowProperty, 0);
        _buscador.SetValue(Grid.RowProperty, 1);
        navegacion.SetValue(Grid.RowProperty, 2);

        _leyenda = new Label
        {
            Text = "Lotes: ID · Nombre · Descripción",
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Gray
        };
        _leyenda.SetValue(Grid.RowProperty, 3);

        _tabla = CrearTabla();
        var guardar = CrearBotonGuardar();
        guardar.SetValue(Grid.RowProperty, 5);
        return new Grid
        {
            Padding = 12,
            RowDefinitions = new RowDefinitionCollection
            {
                new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)
            },
            RowSpacing = 10,
            Children =
            {
                selectorModo,
                _buscador,
                navegacion,
                _leyenda,
                _tabla,
                guardar
            }
        };
    }

    private Button CrearBotonGuardar()
    {
        var boton = new Button { Text = "Guardar página" };
        boton.Clicked += async (_, _) =>
        {
            if (_modoDocumentos)
                await GuardarDocumentosAsync();
            else
                await GuardarLotesAsync();
        };
        return boton;
    }

    private CollectionView CrearTabla()
    {
        var tabla = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            ItemsSource = _modoDocumentos ? Documentos : Lotes,
            ItemTemplate = _modoDocumentos ? CrearPlantillaDocumento() : CrearPlantillaLote(),
            RemainingItemsThreshold = 5
        };
        tabla.SetValue(Grid.RowProperty, 4);
        return tabla;
    }

    private DataTemplate CrearPlantillaLote() => new(() =>
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(70), new(GridLength.Star), new(GridLength.Star) },
            ColumnSpacing = 6
        };
        var id = new Entry { Placeholder = "ID", Keyboard = Keyboard.Numeric };
        var nombre = new Entry { Placeholder = "Nombre" };
        var descripcion = new Entry { Placeholder = "Descripción" };
        grid.Add(id, 0); grid.Add(nombre, 1); grid.Add(descripcion, 2);
        id.SetBinding(Entry.TextProperty, "IdTexto", BindingMode.TwoWay);
        nombre.SetBinding(Entry.TextProperty, "Nombre", BindingMode.TwoWay);
        descripcion.SetBinding(Entry.TextProperty, "Descripcion", BindingMode.TwoWay);
        return new Border { Padding = 8, Margin = new Thickness(0, 2), StrokeThickness = 1, Content = grid };
    });

    private DataTemplate CrearPlantillaDocumento() => new(() =>
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(60), new(120), new(60), new(GridLength.Star) },
            ColumnSpacing = 4
        };
        var id = new Entry { Placeholder = "ID", Keyboard = Keyboard.Numeric };
        var lote = new Picker { Title = "Lote", ItemsSource = LotesOpciones, ItemDisplayBinding = new Binding(nameof(LoteOption.Nombre)) };
        var orden = new Entry { Placeholder = "Orden", Keyboard = Keyboard.Numeric };
        var nombre = new Entry { Placeholder = "Nombre" };
        grid.Add(id, 0); grid.Add(lote, 1); grid.Add(orden, 2); grid.Add(nombre, 3);
        id.SetBinding(Entry.TextProperty, "IdTexto", BindingMode.TwoWay);
        lote.SetBinding(Picker.SelectedItemProperty, "LoteSeleccionado", BindingMode.TwoWay);
        orden.SetBinding(Entry.TextProperty, "OrdenTexto", BindingMode.TwoWay);
        nombre.SetBinding(Entry.TextProperty, "Nombre", BindingMode.TwoWay);
        return new Border { Padding = 8, Margin = new Thickness(0, 2), StrokeThickness = 1, Content = grid };
    });

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarPaginaAsync(0);
    }

    private void CambiarModo(bool documentos)
    {
        if (_modoDocumentos == documentos)
            return;
        _modoDocumentos = documentos;
        _pagina = 0;
        _leyenda.Text = documentos
            ? "Documentos: ID · Lote · Orden · Nombre"
            : "Lotes: ID · Nombre · Descripción";
        _tabla = CrearTabla();
        ((Grid)Content).Children.RemoveAt(4);
        ((Grid)Content).Children.Insert(4, _tabla);
        _ = CargarPaginaAsync(0);
    }

    private async void OnBusquedaChanged(object? sender, TextChangedEventArgs e)
    {
        try
        {
            await Task.Delay(250, _cierre.Token);
            _pagina = 0;
            await CargarPaginaAsync(0);
        }
        catch (OperationCanceledException) { }
    }

    private async Task CambiarPaginaAsync(int delta)
    {
        if (_cargando)
            return;
        await CargarPaginaAsync(_pagina + delta);
    }

    private async Task CargarPaginaAsync(int pagina)
    {
        if (_cargando)
            return;
        _cargando = true;
        _anterior.IsEnabled = false;
        _siguiente.IsEnabled = false;
        try
        {
            var busqueda = _buscador.Text;
            if (_modoDocumentos)
            {
                if (LotesOpciones.Count == 0)
                    foreach (var lote in await _database.ObtenerLotesAsync())
                        LotesOpciones.Add(new LoteOption(lote.Id, lote.Nombre));
                var resultado = await _database.ObtenerDocumentosPaginaAsync(busqueda, pagina, TamanoPagina);
                Documentos.Clear();
                foreach (var documento in resultado.Items)
                    Documentos.Add(new DocumentoRow(documento, LotesOpciones));
                _total = resultado.Total;
            }
            else
            {
                var resultado = await _database.ObtenerLotesPaginaAsync(busqueda, pagina, TamanoPagina);
                Lotes.Clear();
                foreach (var lote in resultado.Items)
                    Lotes.Add(new LoteRow(lote));
                _total = resultado.Total;
            }

            _pagina = Math.Max(0, pagina);
            ActualizarNavegacion();
        }
        finally
        {
            _cargando = false;
        }
    }

    private void ActualizarNavegacion()
    {
        var inicio = _total == 0 ? 0 : _pagina * TamanoPagina + 1;
        var fin = Math.Min((_pagina + 1) * TamanoPagina, _total);
        _estado.Text = $"{inicio}-{fin} de {_total}";
        _anterior.IsEnabled = _pagina > 0;
        _siguiente.IsEnabled = (_pagina + 1) * TamanoPagina < _total;
    }

    private async Task GuardarLotesAsync()
    {
        try
        {
            foreach (var fila in Lotes)
                await _database.ActualizarLoteAvanzadoAsync(fila.IdOriginal, fila.Id, fila.Nombre.Trim(), fila.Descripcion);
            await CargarPaginaAsync(_pagina);
            await DisplayAlertAsync("Guardado", "La página de lotes se actualizó correctamente.", "OK");
        }
        catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    private async Task GuardarDocumentosAsync()
    {
        try
        {
            foreach (var fila in Documentos)
                await _database.ActualizarDocumentoAvanzadoAsync(fila.IdOriginal, fila.Id, fila.LoteId, fila.Orden, fila.Nombre.Trim());
            await CargarPaginaAsync(_pagina);
            await DisplayAlertAsync("Guardado", "La página de documentos se actualizó correctamente.", "OK");
        }
        catch (Exception ex) { await DisplayAlertAsync("Error", ex.Message, "OK"); }
    }

    public sealed class LoteRow
    {
        public LoteRow(Lote lote) { IdOriginal = lote.Id; Id = lote.Id; Nombre = lote.Nombre; Descripcion = lote.Descripcion; }
        public int IdOriginal { get; set; }
        public int Id { get; set; }
        public string IdTexto { get => Id.ToString(); set { if (int.TryParse(value, out var resultado)) Id = resultado; } }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
    }

    public sealed class DocumentoRow
    {
        private LoteOption? _loteSeleccionado;
        public DocumentoRow(Documento documento, IReadOnlyList<LoteOption> lotes)
        {
            IdOriginal = documento.Id; Id = documento.Id; LoteId = documento.LoteId; Orden = documento.Orden; Nombre = documento.Nombre;
            LoteSeleccionado = lotes.FirstOrDefault(lote => lote.Id == documento.LoteId);
        }
        public int IdOriginal { get; set; }
        public int Id { get; set; }
        public int LoteId { get; set; }
        public int Orden { get; set; }
        public string IdTexto { get => Id.ToString(); set { if (int.TryParse(value, out var resultado)) Id = resultado; } }
        public string OrdenTexto { get => Orden.ToString(); set { if (int.TryParse(value, out var resultado)) Orden = resultado; } }
        public string Nombre { get; set; }
        public LoteOption? LoteSeleccionado { get => _loteSeleccionado; set { _loteSeleccionado = value; if (value is not null) LoteId = value.Id; } }
    }

    public sealed class LoteOption
    {
        public LoteOption(int id, string nombre) { Id = id; Nombre = nombre; }
        public int Id { get; }
        public string Nombre { get; }
    }
}