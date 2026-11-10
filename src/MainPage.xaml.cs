using System.Collections.ObjectModel;
using CamScanner.Models;
using CamScanner.Data;

namespace CamScanner;

public partial class MainPage : ContentPage
{
    public ObservableCollection<Documento> Documentos { get; } = new();
    public ObservableCollection<Lote> Lotes { get; } = new();

    private readonly CamScannerDatabase _database;

    private Documento? _seleccionado;

    public MainPage(CamScannerDatabase database)
    {
        InitializeComponent();
        _database = database;
        BindingContext = this;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        try
        {
            var lotes = await _database.ObtenerLotesAsync();
            var documentos = await _database.ObtenerDocumentosAsync();

            Lotes.Clear();
            Documentos.Clear();

            foreach (var lote in lotes)
                Lotes.Add(lote);

            foreach (var documento in documentos)
                Documentos.Add(documento);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudieron cargar los lotes: {ex.Message}", "OK");
        }
    }

    // ================= Hoja inferior (bottom sheet) =================

    private async Task MostrarHojaAsync(View hoja)
    {
        AddSheet.IsVisible = false;
        ItemSheet.IsVisible = false;

        hoja.IsVisible = true;
        Overlay.IsVisible = true;
        hoja.TranslationY = 400;
        await hoja.TranslateToAsync(0, 0, 200, Easing.CubicOut);
    }

    private async Task OcultarHojaAsync()
    {
        if (!Overlay.IsVisible)
            return;

        var hoja = AddSheet.IsVisible ? (View)AddSheet : ItemSheet;
        await hoja.TranslateToAsync(0, hoja.Height, 150, Easing.CubicIn);

        Overlay.IsVisible = false;
        AddSheet.IsVisible = false;
        ItemSheet.IsVisible = false;
    }

    // El botón "atrás" de Android cierra el menú en lugar de salir de la app
    protected override bool OnBackButtonPressed()
    {
        if (Overlay.IsVisible)
        {
            _ = OcultarHojaAsync();
            return true;
        }
        return base.OnBackButtonPressed();
    }

    private async void OnFabClicked(object? sender, EventArgs e) => await MostrarHojaAsync(AddSheet);

    private async void OnOverlayTapped(object? sender, TappedEventArgs e) => await OcultarHojaAsync();

    // ================= Menú "Agregar" =================

    private async void OnTomarFotoTapped(object? sender, TappedEventArgs e)
    {
        await OcultarHojaAsync();
        try
        {
            var lote = await SeleccionarLoteAsync();
            if (lote is null)
                return;

            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await DisplayAlertAsync("Cámara no disponible", "Este dispositivo no permite capturar fotos.", "OK");
                return;
            }

            var permiso = await Permissions.RequestAsync<Permissions.Camera>();
            if (permiso != PermissionStatus.Granted)
            {
                await DisplayAlertAsync("Permiso requerido", "Activa el permiso de cámara para tomar fotos.", "OK");
                return;
            }

            var foto = await MediaPicker.Default.CapturePhotoAsync();
            await GuardarFotoAsync(foto, lote);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo tomar la foto: {ex.Message}", "OK");
        }
    }

    private async void OnImportarGaleriaTapped(object? sender, TappedEventArgs e)
    {
        await OcultarHojaAsync();
        try
        {
            var lote = await SeleccionarLoteAsync();
            if (lote is null)
                return;

            var fotos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                Title = "Selecciona documentos",
                SelectionLimit = 10 // permite importar varias imágenes a la vez
            });

            if (fotos is null)
                return; // el usuario canceló

            foreach (var foto in fotos)
                await GuardarFotoAsync(foto, lote);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo importar la imagen: {ex.Message}", "OK");
        }
    }

    private async void OnNuevoLoteTapped(object? sender, TappedEventArgs e)
    {
        await OcultarHojaAsync();

        var nombre = await DisplayPromptAsync(
            "Nuevo lote",
            "Nombre del lote:",
            accept: "Guardar",
            cancel: "Cancelar");

        if (string.IsNullOrWhiteSpace(nombre))
            return;

        try
        {
            var lote = new Lote
            {
                Nombre = nombre.Trim(),
                FechaCreacion = DateTime.Now
            };

            await _database.GuardarLoteAsync(lote);
            Lotes.Insert(0, lote);

            await DisplayAlertAsync("Lote creado", "El lote se guardó correctamente.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo guardar el lote: {ex.Message}", "OK");
        }
    }

    private async Task<Lote?> SeleccionarLoteAsync()
    {
        if (Lotes.Count == 0)
        {
            await DisplayAlertAsync("Crea un lote", "Crea un lote antes de agregar documentos.", "OK");
            return null;
        }

        var opciones = Lotes
            .Select(lote => $"{lote.Nombre} · {lote.FechaCreacion:dd/MM/yyyy}")
            .ToArray();
        var seleccion = await DisplayActionSheetAsync("Selecciona un lote", "Cancelar", null, opciones);

        if (seleccion is null || seleccion == "Cancelar")
            return null;

        return Lotes.FirstOrDefault(lote =>
            $"{lote.Nombre} · {lote.FechaCreacion:dd/MM/yyyy}" == seleccion);
    }

    private async void OnLoteSeleccionado(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Lote lote)
            return;

        LotesView.SelectedItem = null;

        var accion = await DisplayActionSheetAsync(lote.Nombre, "Cancelar", null, "Editar", "Eliminar");
        if (accion == "Editar")
        {
            var nombre = await DisplayPromptAsync(
                "Editar lote",
                "Nombre del lote:",
                accept: "Guardar",
                cancel: "Cancelar",
                initialValue: lote.Nombre);

            if (string.IsNullOrWhiteSpace(nombre))
                return;

            try
            {
                lote.Nombre = nombre.Trim();
                await _database.GuardarLoteAsync(lote);
                RefrescarLote(lote);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo actualizar el lote: {ex.Message}", "OK");
            }
        }
        else if (accion == "Eliminar")
        {
            var confirmar = await DisplayAlertAsync(
                "Eliminar lote",
                $"¿Eliminar \"{lote.Nombre}\"?",
                "Eliminar",
                "Cancelar");

            if (!confirmar)
                return;

            try
            {
                if (!await _database.EliminarLoteAsync(lote))
                {
                    await DisplayAlertAsync(
                        "No se puede eliminar",
                        "El lote tiene documentos asociados. Elimínalos o reasígnalos primero.",
                        "OK");
                    return;
                }

                Lotes.Remove(lote);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo eliminar el lote: {ex.Message}", "OK");
            }
        }
    }

    /// <summary>Copia la imagen al almacenamiento de la app y guarda su registro en SQLite.</summary>
    private async Task GuardarFotoAsync(FileResult? foto, Lote lote)
    {
        if (foto is null)
            return; // el usuario canceló

        var carpeta = Path.Combine(FileSystem.AppDataDirectory, "documentos");
        Directory.CreateDirectory(carpeta);

        var id = Guid.NewGuid();
        var ruta = Path.Combine(carpeta, $"{id}.jpg");

        await using (var origen = await foto.OpenReadAsync())
        await using (var destino = File.Create(ruta))
        {
            await origen.CopyToAsync(destino);
        }

        var documento = new Documento
        {
            Nombre = Path.GetFileNameWithoutExtension(foto.FileName),
            RutaImagen = ruta,
            LoteId = lote.Id,
            FechaCreacion = DateTime.Now
        };

        try
        {
            await _database.GuardarDocumentoAsync(documento);
            Documentos.Insert(0, documento);
        }
        catch
        {
            if (File.Exists(ruta))
                File.Delete(ruta);

            throw;
        }
    }

    // ================= Menú contextual del documento =================

    private async void OnMenuDocumentoClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: Documento doc })
        {
            _seleccionado = doc;
            ItemTitle.Text = doc.Nombre;
            await MostrarHojaAsync(ItemSheet);
        }
    }

    private async void OnVerEditarTapped(object? sender, TappedEventArgs e)
    {
        await OcultarHojaAsync();
        await DisplayAlertAsync("Próximamente", "Aquí se mostrará el texto reconocido por OCR para editarlo.", "OK");
    }

    private async void OnExportarExcelTapped(object? sender, TappedEventArgs e)
    {
        await OcultarHojaAsync();
        await DisplayAlertAsync("Próximamente", "Aquí se exportará la información del documento a Excel.", "OK");
    }

    private async void OnRenombrarTapped(object? sender, TappedEventArgs e)
    {
        await OcultarHojaAsync();
        if (_seleccionado is null)
            return;

        var nuevo = await DisplayPromptAsync("Renombrar", "Nuevo nombre:",
            accept: "Guardar", cancel: "Cancelar", initialValue: _seleccionado.Nombre);

        if (string.IsNullOrWhiteSpace(nuevo))
            return;

        _seleccionado.Nombre = nuevo.Trim();
        await ActualizarDocumentoAsync(_seleccionado);
    }

    private async void OnEliminarTapped(object? sender, TappedEventArgs e)
    {
        await OcultarHojaAsync();
        if (_seleccionado is null)
            return;

        var confirmar = await DisplayAlertAsync("Eliminar documento",
            $"¿Eliminar \"{_seleccionado.Nombre}\"? Esta acción no se puede deshacer.", "Eliminar", "Cancelar");

        if (!confirmar)
            return;

        await EliminarDocumentoAsync(_seleccionado);
        _seleccionado = null;
    }

    // ================= Detalle a pantalla completa =================

    private async void OnDocumentoTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: Documento doc })
            await AbrirDetalleAsync(doc);
    }

    private async Task AbrirDetalleAsync(Documento doc)
    {
        var pagina = new DetallePage(doc);
        pagina.Actualizado += documento => _ = ActualizarDocumentoAsync(documento);
        pagina.Eliminado += documento => _ = EliminarDocumentoAsync(documento);
        await Navigation.PushAsync(pagina);
    }

    // ================= Operaciones compartidas =================

    /// <summary>Documento no notifica cambios, así que se reinserta para refrescar la lista.</summary>
    private void RefrescarDocumento(Documento doc)
    {
        var indice = Documentos.IndexOf(doc);
        if (indice < 0)
            return;

        Documentos.RemoveAt(indice);
        Documentos.Insert(indice, doc);
    }

    private void RefrescarLote(Lote lote)
    {
        var indice = Lotes.IndexOf(lote);
        if (indice < 0)
            return;

        Lotes.RemoveAt(indice);
        Lotes.Insert(indice, lote);
    }

    private async Task ActualizarDocumentoAsync(Documento documento)
    {
        try
        {
            await _database.GuardarDocumentoAsync(documento);
            RefrescarDocumento(documento);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo actualizar el documento: {ex.Message}", "OK");
        }
    }

    /// <summary>Borra el archivo de imagen y quita el documento de la lista.</summary>
    private async Task EliminarDocumentoAsync(Documento doc)
    {
        try
        {
            await _database.EliminarDocumentoAsync(doc);

            if (File.Exists(doc.RutaImagen))
                File.Delete(doc.RutaImagen);

            Documentos.Remove(doc);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo eliminar el documento: {ex.Message}", "OK");
        }
    }
}
