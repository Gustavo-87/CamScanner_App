using Microsoft.Extensions.Logging;
using CamScanner.Data;

namespace CamScanner;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if ANDROID || IOS
        builder.Services.AddSingleton<IOcrService, OcrService>();
#endif
#if DEBUG
        builder.Logging.AddDebug();

#endif



builder.Services.AddSingleton<CamScannerDatabase>();
		return builder.Build();
	}
}
