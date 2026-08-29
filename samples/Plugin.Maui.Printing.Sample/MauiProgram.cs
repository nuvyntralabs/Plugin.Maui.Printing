using Microsoft.Extensions.Logging;
using Plugin.Maui.Printing;

namespace Plugin.Maui.Printing.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.Services.AddSingleton<MainPage>();

        builder
            .UseMauiApp<App>()
            .UseMauiPrinting(options =>
            {
                options.DefaultJobName = "Printing";
                options.DefaultThermalWidth = ThermalPaperWidth.Mm80;
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
