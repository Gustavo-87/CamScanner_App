using System.Collections.ObjectModel;
using CamScanner.Models;
using CamScanner.Data;
using CamScanner.Services;

namespace CamScanner;

public partial class MainPage : ContentPage
{
    public ObservableCollection<Documento> Documentos { get; } = new();
    public ObservableCollection<Lote> Lotes { get; } = new();

    public ObservableCollection<LoteCard> LotesVista { get; } = new();
    private bool _cargando;
    private bool _navegando;
    private bool _exportandoLotes;
    private bool _modoSeleccion;
    private bool _vistaInicializada;
    private bool _recargarAlVolver;
    private int? _loteActivadoRecientemente;
    private readonly HashSet<int> _lotesSeleccionados = new();



    private readonly CamScannerDatabase _database;

    private Documento? _seleccionado;

    public MainPage(CamScannerDatabase database)
    {
        InitializeComponent();
        Lotes.CollectionChanged += (_, _) => ReconstruirLotesVista();
        Documentos.CollectionChanged += (_, _) => ReconstruirLotesVista();
        _database = database;
        BindingContext = this;
        Loaded += OnLoaded;
    }

    private async void OnLoteDragStarting(object? sender, DragStartingEventArgs e)
    {
        e.Cancel = true;

        if (!_modoSeleccion && sender is BindableObject { BindingContext: LoteCard card })
            await ActivarModoSeleccionAsync(card);

    }

    private async Task ActivarModoSeleccionAsync(LoteCard loteInicial)
    {
        _modoSeleccion = true;
        _lotesSeleccionados.Clear();
        _lotesSeleccionados.Add(loteInicial.Lote.Id);
        _loteActivadoRecientemente = loteInicial.Lote.Id;
        this.FindByName<Button>("ExportarLotesButton").IsVisible = EsPlataformaMovil();
        this.FindByName<Button>("GuardarLotesButton").IsVisible = EsPlataformaMovil();
        this.FindByName<Button>("ExportarZipButton").IsVisible = true;
        this.FindByName<Button>("CompartirZipButton").IsVisible = EsPlataformaMovil();
        this.FindByName<Button>("BaseDatosButton").IsVisible = EsPlataformaMovil();
        this.FindByName<Button>("CancelarSeleccionButton").IsVisible = true;

        await Task.Delay(300);
        ReconstruirLotesVista();

        await Task.Delay(400);
        if (_loteActivadoRecientemente == loteInicial.Lote.Id)
            _loteActivadoRecientemente = null;
    }

    private void OnCancelarSeleccionClicked(object? sender, EventArgs e)
    {
        _lotesSeleccionados.Clear();
        _modoSeleccion = false;
        _loteActivadoRecientemente = null;
        this.FindByName<Button>("ExportarLotesButton").IsVisible = false;
        this.FindByName<Button>("GuardarLotesButton").IsVisible = false;
        this.FindByName<Button>("ExportarZipButton").IsVisible = false;
        this.FindByName<Button>("CompartirZipButton").IsVisible = false;
        this.FindByName<Button>("BaseDatosButton").IsVisible = false;
        this.FindByName<Button>("CancelarSeleccionButton").IsVisible = false;
        ReconstruirLotesVista();
    }

    private async void OnExportarZipClicked(object? sender, EventArgs e)
    {
        await ProcesarZipAsync(compartir: false);
    }

    private async void OnCompartirZipClicked(object? sender, EventArgs e)
    {
        await ProcesarZipAsync(compartir: true);
    }

    private async Task ProcesarZipAsync(bool compartir)
    {
        if (_exportandoLotes)
            return;

        var lotesConDocumentos = ObtenerLotesSeleccionadosConDocumentos();
        if (lotesConDocumentos.Count == 0)
        {
            await DisplayAlertAsync(
                "Sin documentos",
                "Selecciona al menos un lote con documentos para crear el ZIP.",
                "OK");
            return;
        }

        _exportandoLotes = true;
        try
        {
            var contenido = await new ZipExportService().ExportarLotesAsync(lotesConDocumentos);
            var nombre = $"CamScanner_Lotes_{DateTime.Now:yyyyMMdd_HHmm}.zip";

            if (compartir)
            {
                var ruta = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid():N}_{nombre}");
                await File.WriteAllBytesAsync(ruta, contenido);
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Compartir lotes como ZIP",
                    File = new ShareFile(ruta, "application/zip")
                });
            }
            else if (await ArchivoService.GuardarAsync(contenido, nombre))
            {
                await DisplayAlertAsync("Guardado", "El ZIP se guardó correctamente.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error al crear ZIP", ex.Message, "OK");
        }
        finally
        {
            _exportandoLotes = false;
        }
    }

    private List<(Lote Lote, IEnumerable<Documento> Documentos)> ObtenerLotesSeleccionadosConDocumentos()
    {
        return Lotes
            .Where(lote => _lotesSeleccionados.Contains(lote.Id))
            .Select(lote => (Lote: lote,
                Documentos: (IEnumerable<Documento>)Documentos
                    .Where(documento => documento.LoteId == lote.Id)
                    .OrderBy(documento => documento.Orden)
                    .ToList()))
            .Where(grupo => grupo.Documentos.Any())
            .ToList();
    }

    private async void OnGuardarLotesClicked(object? sender, EventArgs e)
    {
        if (_exportandoLotes)
            return;

        var lotesSeleccionados = Lotes
            .Where(lote => _lotesSeleccionados.Contains(lote.Id))
            .ToList();

        if (lotesSeleccionados.Count == 0)
        {
            await DisplayAlertAsync("Selecciona lotes", "Marca al menos un lote para guardarlo.", "OK");
            return;
        }

        _exportandoLotes = true;
        try
        {
            var lotesConDocumentos = lotesSeleccionados
                .Select(lote => (Lote: lote,
                    Documentos: (IEnumerable<Documento>)Documentos
                        .Where(documento => documento.LoteId == lote.Id)
                        .OrderBy(documento => documento.Orden)
                        .ToList()))
                .Where(grupo => grupo.Documentos.Any())
                .ToList();

            if (lotesConDocumentos.Count == 0)
            {
                await DisplayAlertAsync("Sin documentos", "No hay documentos para guardar.", "OK");
                return;
            }

            var contenido = new ExcelExportService().ExportarLotes(lotesConDocumentos);
            var guardado = await ArchivoService.GuardarAsync(
                contenido,
                "CamScanner_Lotes.xlsx");

            if (guardado)
            {
                _lotesSeleccionados.Clear();
                ReconstruirLotesVista();
                await DisplayAlertAsync("Guardado", "El archivo Excel se guardó correctamente.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error al guardar", $"No se pudieron guardar los lotes: {ex.Message}", "OK");
        }
        finally
        {
            _exportandoLotes = false;
        }
    }

    private static bool EsPlataformaMovil()
    {
#if ANDROID || IOS
        return true;
#else
        return false;
#endif
    }

    private async void OnBaseDatosClicked(object? sender, EventArgs e)
    {
        _recargarAlVolver = true;
        await Navigation.PushAsync(new DatabasePage());
    }

    private async void OnExportarLotesClicked(object? sender, EventArgs e)
    {
        if (_exportandoLotes)
            return;

        var lotesSeleccionados = Lotes
            .Where(lote => _lotesSeleccionados.Contains(lote.Id))
            .ToList();

        if (lotesSeleccionados.Count == 0)
        {
            await DisplayAlertAsync("Selecciona lotes", "Marca al menos un lote para exportarlo.", "OK");
            return;
        }

        _exportandoLotes = true;
        try
        {
            var lotesConDocumentos = lotesSeleccionados
                .Select(lote => (Lote: lote,
                    Documentos: (IEnumerable<Documento>)Documentos
                        .Where(documento => documento.LoteId == lote.Id)
                        .OrderBy(documento => documento.Orden)
                        .ToList()))
                .Where(grupo => grupo.Documentos.Any())
                .ToList();

            if (lotesConDocumentos.Count == 0)
            {
                await DisplayAlertAsync("Sin documentos", "No hay documentos para exportar.", "OK");
                return;
            }

            var contenido = new ExcelExportService().ExportarLotes(lotesConDocumentos);
            var ruta = Path.Combine(
                FileSystem.CacheDirectory,
                $"CamScanner_Lotes_{Guid.NewGuid():N}.xlsx");

            await File.WriteAllBytesAsync(ruta, contenido);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Exportar lotes a Excel",
                File = new ShareFile(
                    ruta,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            });
            _lotesSeleccionados.Clear();
            ReconstruirLotesVista();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error al exportar", $"No se pudieron exportar los lotes: {ex.Message}", "OK");
        }
        finally
        {
            _exportandoLotes = false;
        }
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        await RecargarDatosAsync();
        _vistaInicializada = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        LimpiarEstadoVisual();
        RestaurarEstadoVisual();

        if (_vistaInicializada && _recargarAlVolver && !_cargando)
        {
            _recargarAlVolver = false;
            await RecargarDatosAsync();
        }

    }

    protected override void OnDisappearing()
    {
        LimpiarEstadoVisual();
        base.OnDisappearing();
    }

    private void LimpiarEstadoVisual()
    {
        _loteActivadoRecientemente = null;
        Overlay.IsVisible = false;
        AddSheet.IsVisible = false;
        ItemSheet.IsVisible = false;
    }

    private void RestaurarEstadoVisual()
    {
        MainRoot.Opacity = 1;
        LotesView.Opacity = 1;
        Overlay.Opacity = 1;
    }

    private async Task RecargarDatosAsync()
    {
        if (_cargando)
            return;

        _cargando = true; // evita reconstruir la vista por cada elemento agregado
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
        finally
        {
            _cargando = false;
            ReconstruirLotesVista();
        }
    }



    private void ReconstruirLotesVista()
    {
        if (_cargando)
            return;

        LotesVista.Clear();
        foreach (var lote in Lotes)
        {
            var docs = Documentos.Where(d => d.LoteId == lote.Id).ToList();
            var tarjeta = new LoteCard(lote, docs, _lotesSeleccionados.Contains(lote.Id))
            {
                ModoSeleccion = _modoSeleccion
            };
            LotesVista.Add(tarjeta);
        }
    }

    private void OnLoteSelectionChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not BindableObject { BindingContext: LoteCard card })
            return;

        card.Seleccionado = e.Value;
        if (e.Value)
            _lotesSeleccionados.Add(card.Lote.Id);
        else
            _lotesSeleccionados.Remove(card.Lote.Id);
    }

    // Tocar el lote = entrar a la "carpeta"
    private async void OnLoteTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not BindableObject { BindingContext: LoteCard card })
            return;

        if (_modoSeleccion)
        {
            if (_loteActivadoRecientemente == card.Lote.Id)
            {
                _loteActivadoRecientemente = null;
                return;
            }

            card.Seleccionado = !card.Seleccionado;
            if (card.Seleccionado)
                _lotesSeleccionados.Add(card.Lote.Id);
            else
                _lotesSeleccionados.Remove(card.Lote.Id);
            ReconstruirLotesVista();
            return;
        }

        if (_navegando)
            return;

        _navegando = true;
        try
        {
            LimpiarEstadoVisual();
            await Navigation.PushAsync(new LotePage(card.Lote, Documentos, doc => AbrirDetalleAsync(doc)));
        }
        finally
        {
            _navegando = false;
        }
    }

    // Botón ⋮ de la tarjeta
    private async void OnMenuLoteClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: LoteCard card })
            await MostrarOpcionesLoteAsync(card.Lote);
    }

    private async Task MostrarOpcionesLoteAsync(Lote lote)
    {
        var accion = await DisplayActionSheetAsync(
            lote.Nombre,
            "Cancelar",
            null,
            "Cambiar nombre",
            "Cambiar descripción",
            "Eliminar");

        if (accion == "Cambiar nombre")
        {
            var nombre = await DisplayPromptAsync(
                "Cambiar nombre",
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
        else if (accion == "Cambiar descripción")
        {
            var descripcion = await DisplayPromptAsync(
                "Cambiar descripción",
                "Descripción del lote:",
                accept: "Guardar",
                cancel: "Cancelar",
                initialValue: lote.Descripcion ?? string.Empty);

            if (descripcion is null)
                return;

            try
            {
                lote.Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
                await _database.GuardarLoteAsync(lote);
                RefrescarLote(lote);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo actualizar la descripción: {ex.Message}", "OK");
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



        var accion = await DisplayActionSheetAsync(
            lote.Nombre,
            "Cancelar",
            null,
            "Cambiar nombre",
            "Cambiar descripción",
            "Eliminar");

        if (accion == "Ver fotos")
        {
            await Navigation.PushAsync(new LotePage(lote, Documentos, doc => AbrirDetalleAsync(doc)));
        }


        if (accion == "Cambiar nombre")
        {
            var nombre = await DisplayPromptAsync(
                "Cambiar nombre",
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
        else if (accion == "Cambiar descripción")
        {
            var descripcion = await DisplayPromptAsync(
                "Cambiar descripción",
                "Descripción del lote:",
                accept: "Guardar",
                cancel: "Cancelar",
                initialValue: lote.Descripcion ?? string.Empty);

            if (descripcion is null)
                return;

            try
            {
                lote.Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
                await _database.GuardarLoteAsync(lote);
                RefrescarLote(lote);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"No se pudo actualizar la descripción: {ex.Message}", "OK");
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
            Nombre = $"Foto_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}",
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
        if (_seleccionado is null)
            return;

        await AbrirDetalleAsync(_seleccionado, iniciarOcr: true);
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

    private async Task AbrirDetalleAsync(Documento doc, bool iniciarOcr = false)
    {
        if (_navegando)
            return;

        _navegando = true;
        var pagina = new DetallePage(doc) { IniciarOcrAlAbrir = iniciarOcr };
        pagina.Actualizado += documento => _ = ActualizarDocumentoAsync(documento);
        pagina.Eliminado += documento => _ = EliminarDocumentoAsync(documento);
        try
        {
            await Navigation.PushAsync(pagina);
        }
        finally
        {
            _navegando = false;
        }
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
