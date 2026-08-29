using Microsoft.Maui.Hosting;

namespace Plugin.Maui.Printing;

sealed class PrintingInitializer : IMauiInitializeService
{
    public void Initialize(IServiceProvider services)
    {
        var printer = services.GetService<IPrinter>() ?? Printer.Current;
        Printer.SetDefault(printer);
        printer.Start();
    }
}
