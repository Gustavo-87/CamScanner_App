using System.Collections.ObjectModel;
using CamScanner.Data;
using CamScanner.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CamScanner;

public partial class DatabasePage : ContentPage
{
    private readonly CamScannerDatabase _database =
        IPlatformApplication.Current!.Services.GetRequiredService<CamScannerDatabase>();

    public ObservableCollection<LoteRow> Lotes { get; } = new();
    public ObservableCollection<DocumentoRow> Documentos { get; } = new();
    public ObservableCollection<LoteOption> LotesOpciones { get; } = new();

    public DatabasePage()
    {
        Content = CrearContenido();
        BindingContext = this;
    }

    private View CrearContenido()
    {
        var lotes = new CollectionView { ItemsSource = Lotes, SelectionMode = SelectionMode.None, ItemTemplate =
            new DataTemplate(() =>
            {
                var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new(70), new(GridLength.Star), new(GridLength.Star)
                }, ColumnSpacing = 6 };
                grid.Add(new Entry { Placeholder = "ID", Keyboard = Keyboard.Numeric }, 0);
                grid.Add(new Entry { Placeholder = "Nombre" }, 1);
                grid.Add(new Entry { Placeholder = "Descripción" }, 2);
                ((Entry)grid[0]).SetBinding(Entry.TextProperty, "IdTexto", BindingMode.TwoWay);
                ((Entry)grid[1]).SetBinding(Entry.TextProperty, "Nombre", BindingMode.TwoWay);
                ((Entry)grid[2]).SetBinding(Entry.TextProperty, "Descripcion", BindingMode.TwoWay);
                return new Border { Padding = 8, Margin = new Thickness(0, 2), StrokeThickness = 1, Content = grid };
            }) };

        var documentos = new CollectionView { ItemsSource = Documentos, SelectionMode = SelectionMode.None, ItemTemplate =
            new DataTemplate(() =>
            {
                var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new(60), new(160), new(60), new(GridLength.Star)
                }, ColumnSpacing = 4 };
                grid.Add(new Entry { Placeholder = "ID", Keyboard = Keyboard.Numeric }, 0);
                var selectorLote = new Picker
                {
                    Title = "Seleccionar lote",
                    ItemsSource = LotesOpciones,
                    ItemDisplayBinding = new Binding(nameof(LoteOption.Nombre))
                };
                grid.Add(selectorLote, 1);
                grid.Add(new Entry { Placeholder = "Orden", Keyboard = Keyboard.Numeric }, 2);
                grid.Add(new Entry { Placeholder = "Nombre" }, 3);
                ((Entry)grid[0]).SetBinding(Entry.TextProperty, "IdTexto", BindingMode.TwoWay);
                selectorLote.SetBinding(Picker.SelectedItemProperty, "LoteSeleccionado", BindingMode.TwoWay);
                ((Entry)grid[2]).SetBinding(Entry.TextProperty, "OrdenTexto", BindingMode.TwoWay);
                ((Entry)grid[3]).SetBinding(Entry.TextProperty, "Nombre", BindingMode.TwoWay);
                return new Border { Padding = 8, Margin = new Thickness(0, 2), StrokeThickness = 1, Content = grid };
            }) };

        return new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 12,
                Spacing = 16,
                Children =
                {
                    CrearCabecera("Lotes", "OnGuardarLotesClicked"),
                    new Label { Text = "ID · Nombre · Descripción", FontAttributes = FontAttributes.Bold, TextColor = Colors.Gray },
                   lotes,
                    CrearCabecera("Documentos", "OnGuardarDocumentosClicked"),
                    new Label { Text = "ID · Lote (nombre) · Orden · Nombre", FontAttributes = FontAttributes.Bold, TextColor = Colors.Gray },
                  documentos
                }
            }
        };
    }

    private View CrearCabecera(string titulo, string nombreEvento)
    {
        var cabecera = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(GridLength.Star), new(GridLength.Auto)
            },
            ColumnSpacing = 8
        };

        cabecera.Add(new Label
        {
            Text = titulo,
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        }, 0);

        var boton = new Button { Text = "Guardar", Padding = new Thickness(12, 6) };
        if (nombreEvento == "OnGuardarLotesClicked")
            boton.Clicked += OnGuardarLotesClicked;
        else
            boton.Clicked += OnGuardarDocumentosClicked;

        cabecera.Add(boton, 1);
        return cabecera;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarAsync();
    }

    private async Task CargarAsync()
    {
        Lotes.Clear();
        LotesOpciones.Clear();
        var lotes = await _database.ObtenerLotesAsync();
        foreach (var lote in lotes)
        {
            Lotes.Add(new LoteRow(lote));
            LotesOpciones.Add(new LoteOption(lote.Id, lote.Nombre));
        }

        Documentos.Clear();
        foreach (var documento in await _database.ObtenerDocumentosAsync())
            Documentos.Add(new DocumentoRow(documento, LotesOpciones));
    }

    private async void OnGuardarLotesClicked(object? sender, EventArgs e)
    {
        try
        {
            foreach (var fila in Lotes)
            {
                await _database.ActualizarLoteAvanzadoAsync(
                    fila.IdOriginal, fila.Id, fila.Nombre.Trim(), fila.Descripcion);
            }

            await CargarAsync();
            await DisplayAlertAsync("Guardado", "La tabla de lotes se actualizó correctamente.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
            await CargarAsync();
        }
    }

    private async void OnGuardarDocumentosClicked(object? sender, EventArgs e)
    {
        try
        {
            foreach (var fila in Documentos)
            {
                await _database.ActualizarDocumentoAvanzadoAsync(
                    fila.IdOriginal, fila.Id, fila.LoteId, fila.Orden, fila.Nombre.Trim());
            }

            await CargarAsync();
            await DisplayAlertAsync("Guardado", "La tabla de documentos se actualizó correctamente.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", ex.Message, "OK");
            await CargarAsync();
        }
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
            IdOriginal = documento.Id;
            Id = documento.Id;
            LoteId = documento.LoteId;
            Orden = documento.Orden;
            Nombre = documento.Nombre;
            LoteSeleccionado = lotes.FirstOrDefault(lote => lote.Id == documento.LoteId);
        }

        public int IdOriginal { get; set; }
        public int Id { get; set; }
        public int LoteId { get; set; }
        public int Orden { get; set; }
        public string IdTexto { get => Id.ToString(); set { if (int.TryParse(value, out var resultado)) Id = resultado; } }
        public string OrdenTexto { get => Orden.ToString(); set { if (int.TryParse(value, out var resultado)) Orden = resultado; } }
        public string Nombre { get; set; }
        public LoteOption? LoteSeleccionado
        {
            get => _loteSeleccionado;
            set
            {
                _loteSeleccionado = value;
                if (value is not null)
                    LoteId = value.Id;
            }
        }
    }

    public sealed class LoteOption
    {
        public LoteOption(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }

        public int Id { get; }
        public string Nombre { get; }
    }
}