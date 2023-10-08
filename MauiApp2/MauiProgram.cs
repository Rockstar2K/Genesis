using Microsoft.Extensions.Logging;
using MauiApp2;
using SkiaSharp.Views.Maui.Controls.Hosting;


namespace MauiApp2
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

#if WINDOWS
            builder.Services.AddSingleton<IAudioPlayer, WindowsAudioPlayer>();
#endif

            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    
                    fonts.AddFont("Montserrat-Black.ttf", "Montserrat-Black");
                    fonts.AddFont("Montserrat-BlackItalic.ttf", "Montserrat-BlackItalic");
                    fonts.AddFont("Montserrat-Bold.ttf", "Montserrat-Bold");
                    fonts.AddFont("Montserrat-BoldItalic.ttf", "Montserrat-BoldItalic");
                    fonts.AddFont("Montserrat-ExtraBold.ttf", "Montserrat-ExtraBold");
                    fonts.AddFont("Montserrat-ExtraBoldItalic.ttf", "Montserrat-ExtraBoldItalic");
                    fonts.AddFont("Montserrat-ExtraLight.ttf", "Montserrat-ExtraLight");
                    fonts.AddFont("Montserrat-ExtraLightItalic.ttf", "Montserrat-ExtraLightItalic");
                    fonts.AddFont("Montserrat-Italic.ttf", "Montserrat-Italic");
                    fonts.AddFont("Montserrat-Light.ttf", "Montserrat-Light");
                    fonts.AddFont("Montserrat-LightItalic.ttf", "Montserrat-LightItalic");
                    fonts.AddFont("Montserrat-Medium.ttf", "Montserrat-Medium");
                    fonts.AddFont("Montserrat-MediumItalic.ttf", "Montserrat-MediumItalic");
                    fonts.AddFont("Montserrat-Regular.ttf", "Montserrat-Regular");
                    fonts.AddFont("Montserrat-SemiBold.ttf", "Montserrat-SemiBold");
                    fonts.AddFont("Montserrat-Thin.ttf", "Montserrat-Thin");
                    fonts.AddFont("Montserrat-ThinItalic.ttf", "Montserrat-ThinItalic");
                    fonts.AddFont("Montserrat-Regular.ttf", "Montserrat-Regular");
                    
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
