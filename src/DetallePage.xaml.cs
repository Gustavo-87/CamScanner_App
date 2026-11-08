using CamScanner.Models;

namespace CamScanner;

public partial class DetallePage : ContentPage
{
    private readonly Documento _doc;
    private readonly IOcrService _ocrService; // Instancia de abstracción de servicio nativo avanzado

    private bool _ocrEnCurso;
    private bool _avisoAbreEditor;
    private int _avisoId;

    /// <summary>Se dispara después de cambiar el nombre, para que la lista se actualice.</summary>
    public event Action<Documento>? Renombrado;

    /// <summary>Se dispara cuando el usuario confirma eliminar; quien lo recibe borra archivo y registro.</summary>
    public event Action<Documento>? Eliminado;

    // Constructor óptimo utilizando el contenedor de inyección de dependencias de .NET MAUI
    public DetallePage(Documento doc, IOcrService ocrService)
    {
        InitializeComponent();
        _doc = doc;
        _ocrService = ocrService;
        TituloLabel.Text = doc.Nombre;
        Imagen.Source = ImageSource.FromFile(doc.RutaImagen);
    }

    /// <summary>Constructor secundario por si se instancia manualmente sin pasar por el contenedor de dependencias.</summary>
    public DetallePage(Documento doc) : this(doc, App.Current?.Handler?.MauiContext?.Services.GetService<IOcrService>()!)
    {
    }

    private async void OnAtrasClicked(object? sender, EventArgs e) => await Navigation.PopAsync();

    // ================= OCR MEJORADO =================

    private async void OnTextoTapped(object? sender, TappedEventArgs e)
    {
        if (_ocrEnCurso)
            return; // ya está reconociendo; el indicador lo muestra

        // Si ya hay texto guardado, se abre directo el editor
        if (!string.IsNullOrWhiteSpace(_doc.TextoExtraido))
        {
            await AbrirEditorAsync();
            return;
        }

        // Sin await: el OCR corre en segundo plano y el usuario sigue viendo la imagen
        _ = ReconocerTextoAsync();
    }

    private async Task ReconocerTextoAsync()
    {
        _ocrEnCurso = true;
        MostrarProgreso("Reconociendo texto…");

        try
        {
            // Inicializa el motor nativo de la plataforma correspondiente (iOS/Android)
            await _ocrService.InitAsync();

            // Pasamos la ruta del archivo de forma directa. Ahorramos lecturas innecesarias en bytes
            var textoDetectado = await _ocrService.RecognizeTextAsync(_doc.RutaImagen);

            if (string.IsNullOrWhiteSpace(textoDetectado))
            {
                await MostrarAvisoAsync("No se detectó texto", abreEditor: false);
                return;
            }

            // TODO: al integrar SQLite, persistir TextoExtraido (UpdateAsync)
            _doc.TextoExtraido = textoDetectado;
            await MostrarAvisoAsync("Texto listo · Toca para ver", abreEditor: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error de OCR Nativo: {ex}");
            await MostrarAvisoAsync("No se pudo reconocer el texto", abreEditor: false);
        }
        finally
        {
            _ocrEnCurso = false;
        }
    }

    private async Task AbrirEditorAsync()
    {
        var pagina = new TextoPage(_doc.Nombre, _doc.TextoExtraido ?? string.Empty);
        pagina.Guardado += texto =>
        {
            // TODO: al integrar SQLite, persistir TextoExtraido (UpdateAsync)
            _doc.TextoExtraido = texto;
        };
        await Navigation.PushAsync(pagina);
    }

    // ================= Indicador de progreso discreto =================

    private void MostrarProgreso(string texto)
    {
        _avisoId++;
        _avisoAbreEditor = false;

        ProgresoTexto.Text = texto;
        Spinner.IsVisible = true;
        Spinner.IsRunning = true;
        ProgresoPill.InputTransparent = true; // los toques pasan a la imagen
        ProgresoPill.IsVisible = true;
    }

    /// <summary>Muestra el resultado un momento y luego oculta el indicador.</summary>
    private async Task MostrarAvisoAsync(string texto, bool abreEditor)
    {
        var id = ++_avisoId;

        Spinner.IsRunning = false;
        Spinner.IsVisible = false;
        ProgresoTexto.Text = (abreEditor ? "✔ " : "⚠ ") + texto;
        _avisoAbreEditor = abreEditor;
        ProgresoPill.InputTransparent = !abreEditor; // solo es tocable si abre el editor

        await Task.Delay(abreEditor ? 8000 : 3000);

        if (id == _avisoId) // si empezó otro proceso, no lo ocultes
            OcultarProgreso();
    }

    private void OcultarProgreso()
    {
        ProgresoPill.IsVisible = false;
        _avisoAbreEditor = false;
    }

    private async void OnProgresoTapped(object? sender, TappedEventArgs e)
    {
        if (!_avisoAbreEditor)
            return;

        OcultarProgreso();
        await AbrirEditorAsync();
    }

    private async void OnExcelTapped(object? sender, TappedEventArgs e)
    {
        // TODO: exportar la información del documento a Excel
        await DisplayAlertAsync("Próximamente", "Aquí se exportará la información del documento a Excel.", "OK");
    }

    private async void OnCompartirTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            if (!File.Exists(_doc.RutaImagen))
            {
                await DisplayAlertAsync("Archivo no encontrado", "La imagen ya no existe en el dispositivo.", "OK");
                return;
            }

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = _doc.Nombre,
                File = new ShareFile(_doc.RutaImagen)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo compartir: {ex.Message}", "OK");
        }
    }

    private async void OnRenombrarTapped(object? sender, TappedEventArgs e)
    {
        var nuevo = await DisplayPromptAsync("Renombrar", "Nuevo nombre:",
            accept: "Guardar", cancel: "Cancelar", initialValue: _doc.Nombre);

        if (string.IsNullOrWhiteSpace(nuevo))
            return;

        // TODO: al integrar SQLite, persistir el cambio (UpdateAsync)
        _doc.Nombre = nuevo.Trim();
        TituloLabel.Text = _doc.Nombre;
        Renombrado?.Invoke(_doc);
    }

    private async void OnEliminarTapped(object? sender, TappedEventArgs e)
    {
        var confirmar = await DisplayAlertAsync("Eliminar documento",
            $"¿Eliminar \"{_doc.Nombre}\"? Esta acción no se puede deshacer.", "Eliminar", "Cancelar");

        if (!confirmar)
            return;

        Eliminado?.Invoke(_doc);
        await Navigation.PopAsync();
    }
}
