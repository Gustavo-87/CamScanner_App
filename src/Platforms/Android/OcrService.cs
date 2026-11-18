#if ANDROID
using Android.Gms.Tasks;
using Android.Graphics;
using AndroidX.ExifInterface.Media;
using Xamarin.Google.MLKit.Vision.Common;
using Xamarin.Google.MLKit.Vision.Text;
using Xamarin.Google.MLKit.Vision.Text.Latin;

namespace CamScanner;

public class OcrService : IOcrService
{
    private ITextRecognizer? _recognizer;

    public System.Threading.Tasks.Task InitAsync()
    {
        _recognizer ??= TextRecognition.GetClient(TextRecognizerOptions.DefaultOptions);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public async System.Threading.Tasks.Task<string?> RecognizeTextAsync(string rutaImagen)
    {
        if (!File.Exists(rutaImagen))
            return null;

        await InitAsync();

        Bitmap? bitmap = null;
        try
        {
            bitmap = DecodificarReducido(rutaImagen, 2048);
            if (bitmap == null)
                return null;

            int rotacion = ObtenerRotacion(rutaImagen);
            using var image = InputImage.FromBitmap(bitmap, rotacion);

            var listener = new OcrListener();
            _recognizer!.Process(image)
                .AddOnSuccessListener(listener)
                .AddOnFailureListener(listener);

            return await listener.Task;
        }
        finally
        {
            bitmap?.Recycle();
            bitmap?.Dispose();
        }
    }

    private static Bitmap? DecodificarReducido(string ruta, int maxLado)
    {
        var bounds = new BitmapFactory.Options { InJustDecodeBounds = true };
        BitmapFactory.DecodeFile(ruta, bounds);

        int sample = 1;
        while (Math.Max(bounds.OutWidth, bounds.OutHeight) / sample > maxLado)
            sample *= 2;

        var opts = new BitmapFactory.Options { InSampleSize = sample };
        return BitmapFactory.DecodeFile(ruta, opts);
    }

    private static int ObtenerRotacion(string ruta)
    {
        try
        {
            using var exif = new ExifInterface(ruta);
            return exif.GetAttributeInt(ExifInterface.TagOrientation, ExifInterface.OrientationNormal) switch
            {
                ExifInterface.OrientationRotate90 => 90,
                ExifInterface.OrientationRotate180 => 180,
                ExifInterface.OrientationRotate270 => 270,
                _ => 0
            };
        }
        catch { return 0; }
    }

    // Un listener por llamada: sin estado compartido
    private sealed class OcrListener : Java.Lang.Object, IOnSuccessListener, IOnFailureListener
    {
        private readonly TaskCompletionSource<string?> _tcs = new();
        public System.Threading.Tasks.Task<string?> Task => _tcs.Task;

        public void OnSuccess(Java.Lang.Object? result)
        {
            try
            {
                var text = result as Xamarin.Google.MLKit.Vision.Text.Text
                           ?? (result == null ? null
                               : Android.Runtime.Extensions.JavaCast<Xamarin.Google.MLKit.Vision.Text.Text>(result));
                _tcs.TrySetResult(text?.GetText()?.Trim());
            }
            catch (Exception ex)
            {
                _tcs.TrySetException(ex);
            }
        }

        public void OnFailure(Java.Lang.Exception e)
            => _tcs.TrySetException(new InvalidOperationException(e.Message, e));
    }
}
#endif