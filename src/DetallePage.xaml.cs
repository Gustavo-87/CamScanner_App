using CamScanner.Models;
using Microsoft.Extensions.DependencyInjection;
using CamScanner.Data;
using CamScanner.Services;

namespace CamScanner;

/// <summary>El OCR no está registrado o la plataforma no lo soporta (único caso en que se avisa "no disponible").</summary>
public sealed class OcrNoDisponibleException : Exception
{
    public OcrNoDisponibleException(string mensaje, Exception? inner = null) : base(mensaje, inner) { }
}

public partial class DetallePage : ContentPage
{
    private readonly Documento _doc;
    private readonly IOcrService? _ocrService;

    // El motor se inicializa una sola vez por sesión, no en cada toque
    private static readonly SemaphoreSlim _initLock = new(1, 1);
    private static bool _ocrInicializado;

    private bool _ocrEnCurso;
    private bool _avisoAbreEditor;
    private bool _ocrAutoLanzado;
    private int _avisoId;
    private readonly CamScannerDatabase _database =
        IPlatformApplication.Current!.Services
              .GetRequiredService<CamScannerDatabase>();

    private readonly ExcelExportService _excelExportService = new();

    /// <summary>Si es true, al abrir la página se lanza el OCR (o el editor si ya hay texto).</summary>
    public bool IniciarOcrAlAbrir { get; init; }

    public event Action<Documento>? Actualizado;
    public event Action<Documento>? Eliminado;

    public DetallePage(Documento doc, IOcrService? ocrService)
    {
        InitializeComponent();
        _doc = doc;
        _ocrService = ocrService;
        TituloLabel.Text = doc.Nombre;
        Imagen.Source = ImageSource.FromFile(doc.RutaImagen);
    }

    public DetallePage(Documento doc) : this(doc, ResolverOcr()) { }

    private static IOcrService? ResolverOcr() =>
        IPlatformApplication.Current?.Services.GetService<IOcrService>()
        ?? Application.Current?.Handler?.MauiContext?.Services.GetService<IOcrService>();

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (IniciarOcrAlAbrir && !_ocrAutoLanzado)
        {
            _ocrAutoLanzado = true;
            _ = IniciarOcrAsync();
        }
    }

    private async void OnAtrasClicked(object? sender, EventArgs e) => await Navigation.PopAsync();

    // ================= OCR =================

    private async void OnTextoTapped(object? sender, TappedEventArgs e) => await IniciarOcrAsync();

    private async Task IniciarOcrAsync()
    {
        if (_ocrEnCurso)
            return;

        // Si ya hay texto guardado, se abre directo el editor
        if (!string.IsNullOrWhiteSpace(_doc.TextoExtraido))
        {
            await AbrirEditorAsync();
            return;
        }

        _ = ReconocerTextoAsync();
    }

    /// <summary>Devuelve el servicio listo para usar o lanza OcrNoDisponibleException.</summary>
    private async Task<IOcrService> ObtenerOcrListoAsync()
    {
        var ocr = _ocrService
            ?? throw new OcrNoDisponibleException("IOcrService no está registrado en MauiProgram.");

        if (_ocrInicializado)
            return ocr;

        await _initLock.WaitAsync();
        try
        {
            if (!_ocrInicializado)
            {
                try
                {
                    await ocr.InitAsync();
                }
                catch (Exception ex) when (ex is NotSupportedException or FeatureNotSupportedException)
                {
                    throw new OcrNoDisponibleException("La plataforma no soporta OCR.", ex);
                }

                _ocrInicializado = true; // si InitAsync falla, no se marca y se reintenta
            }
        }
        finally
        {
            _initLock.Release();
        }

        return ocr;
    }

    private async Task ReconocerTextoAsync()
    {
        _ocrEnCurso = true;
        MostrarProgreso("Reconociendo texto…");

        string? texto = null;
        string? error = null;

        try
        {
            if (!File.Exists(_doc.RutaImagen))
                throw new FileNotFoundException("La imagen no existe.", _doc.RutaImagen);

            var ocr = await ObtenerOcrListoAsync();
            texto = await ocr.RecognizeTextAsync(_doc.RutaImagen);
        }
        catch (OcrNoDisponibleException ex)
        {
            System.Diagnostics.Debug.WriteLine($"OCR no disponible: {ex}");
            error = "El OCR no está disponible en este dispositivo";
        }
        catch (FileNotFoundException)
        {
            error = "La imagen ya no existe en el dispositivo";
        }
        catch (OperationCanceledException)
        {
            OcultarProgreso();
            return;
        }
        catch (Exception ex)
        {
            // Fallo real y temporal: NO se dice "no disponible"
            System.Diagnostics.Debug.WriteLine($"Error de OCR: {ex}");
            error = "No se pudo reconocer el texto. Inténtalo de nuevo";
        }
        finally
        {
            _ocrEnCurso = false; // se libera antes del aviso para poder reintentar
        }

        if (error is not null)
        {
            await MostrarAvisoAsync(error, abreEditor: false);
            return;
        }

        if (string.IsNullOrWhiteSpace(texto))
        {
            await MostrarAvisoAsync("No se detectó texto", abreEditor: false);
            return;
        }

        _doc.TextoExtraido = texto;
        Actualizado?.Invoke(_doc); // MainPage lo persiste en SQLite (resuelve el TODO)
        await MostrarAvisoAsync("Texto listo · Toca para ver", abreEditor: true);
    }

    private async Task AbrirEditorAsync()
    {
        var pagina = new TextoPage(_doc.Nombre, _doc.TextoExtraido ?? string.Empty);
        pagina.Guardado += texto =>
        {
            _doc.TextoExtraido = texto;
            Actualizado?.Invoke(_doc);
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
        try
        {
            var lotes = await _database.ObtenerLotesAsync();
            var lote = lotes.FirstOrDefault(l => l.Id == _doc.LoteId);

            if (lote is null)
            {
                await DisplayAlertAsync("Error",
                    "No se encontró el lote asociado al documento.", "OK");
                return;
            }

            var contenido = _excelExportService.ExportarDocumento(_doc, lote.Nombre);
            var nombreArchivo = $"CamScanner_{_doc.Id}_{Guid.NewGuid():N}.xlsx";
            var ruta = Path.Combine(FileSystem.CacheDirectory, nombreArchivo);

            await File.WriteAllBytesAsync(ruta, contenido);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Exportar documento a Excel",
                File = new ShareFile(
                    ruta,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error al exportar",
                $"No se pudo generar el archivo Excel: {ex.Message}", "OK");
        }
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

        _doc.Nombre = nuevo.Trim();
        TituloLabel.Text = _doc.Nombre;
        Actualizado?.Invoke(_doc);
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
