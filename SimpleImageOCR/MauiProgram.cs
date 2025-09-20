using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Plugin.Maui.OCR;
using SimpleImageOCR.Services;
using SimpleImageOCR.ViewModels;
using SimpleImageOCR.Views;

namespace SimpleImageOCR
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseOcr()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register services
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<OcrService>();
            builder.Services.AddSingleton<IShareService, ShareService>();
            builder.Services.AddSingleton<ICameraService, CameraService>();

            // Register ViewModels
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<OcrResultViewModel>();
            builder.Services.AddTransient<HistoryViewModel>();

            // Register pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<Views.OcrResultPage>();
            builder.Services.AddTransient<Views.HistoryPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
