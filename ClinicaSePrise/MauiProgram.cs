using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace ClinicaSePrise
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.ConfigureLifecycleEvents(events => events.AddWindows(w => w.OnWindowCreated(window => {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                if (appWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter p) p.Maximize();
            })));
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            QuitarBordeNativoEntry();

            return builder.Build();
        }

        // En Windows el Entry es un TextBox de WinUI con su propio borde y línea inferior.
        // A los Entry con ClassId="SinBorde" (los que ya van dentro de un Border redondeado) se los sacamos.
        static void QuitarBordeNativoEntry()
        {
#if WINDOWS
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("SinBorde", (handler, vista) =>
            {
                if (vista is not Entry { ClassId: "SinBorde" })
                    return;

                var textBox = handler.PlatformView;
                var cero = new Microsoft.UI.Xaml.Thickness(0);
                var transparente = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

                // WinUI cambia borde y fondo según el estado (normal, hover, foco): se pisan todos
                textBox.Resources["TextControlBorderThemeThickness"] = cero;
                textBox.Resources["TextControlBorderThemeThicknessFocused"] = cero;
                textBox.Resources["TextControlBackground"] = transparente;
                textBox.Resources["TextControlBackgroundPointerOver"] = transparente;
                textBox.Resources["TextControlBackgroundFocused"] = transparente;
                textBox.BorderThickness = cero;
            });
#endif
        }
    }
}
