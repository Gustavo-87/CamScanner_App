#if IOS || MACCATALYST
using CoreGraphics;
using Foundation;
using ImageIO;
using UIKit;
using Vision;

namespace CamScanner;

public class OcrService : IOcrService
{
    public Task InitAsync() => Task.CompletedTask;

    public Task<string?> RecognizeTextAsync(string rutaImagen)
    {
        if (!File.Exists(rutaImagen))
            return Task.FromResult<string?>(null);

        // Vision es síncrono y pesado: se ejecuta fuera del hilo de UI
        return Task.Run<string?>(() =>
        {
            using var uiImage = UIImage.FromFile(rutaImagen);
            var cgImage = uiImage?.CGImage;
            if (uiImage == null || cgImage == null)
                return null;

            using var request = new VNRecognizeTextRequest((VNRequestCompletionHandler?)null)
            {
                RecognitionLevel = VNRequestTextRecognitionLevel.Accurate,
                UsesLanguageCorrection = true
            };

            var orientacion = ConvertirOrientacion(uiImage.Orientation);

            using var handler = new VNImageRequestHandler(cgImage, orientacion, new VNImageOptions());

            if (!handler.Perform(new VNRequest[] { request }, out NSError? error))
                throw new InvalidOperationException(error?.LocalizedDescription ?? "Error de OCR en iOS");

            var observaciones = request.GetResults<VNRecognizedTextObservation>();
            if (observaciones == null || observaciones.Length == 0)
                return null;

            var lineas = observaciones
                .Select(o => o.TopCandidates(1).FirstOrDefault()?.String)
                .Where(s => !string.IsNullOrWhiteSpace(s));

            var texto = string.Join(Environment.NewLine, lineas).Trim();
            return string.IsNullOrEmpty(texto) ? null : texto;
        });
    }

    // Las fotos de cámara traen orientación EXIF; hay que pasársela a Vision
    private static CGImagePropertyOrientation ConvertirOrientacion(UIImageOrientation o) => o switch
    {
        UIImageOrientation.Up => CGImagePropertyOrientation.Up,
        UIImageOrientation.Down => CGImagePropertyOrientation.Down,
        UIImageOrientation.Left => CGImagePropertyOrientation.Left,
        UIImageOrientation.Right => CGImagePropertyOrientation.Right,
        UIImageOrientation.UpMirrored => CGImagePropertyOrientation.UpMirrored,
        UIImageOrientation.DownMirrored => CGImagePropertyOrientation.DownMirrored,
        UIImageOrientation.LeftMirrored => CGImagePropertyOrientation.LeftMirrored,
        UIImageOrientation.RightMirrored => CGImagePropertyOrientation.RightMirrored,
        _ => CGImagePropertyOrientation.Up
    };
}
#endif