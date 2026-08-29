using Microsoft.Maui.Hosting;

namespace Plugin.Maui.Printing;

/// <summary>
/// MAUI host registration for printing.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Registers <see cref="IPrinter"/> as a singleton.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.UseMauiPrinting(options =>
    /// {
    ///     options.DefaultJobName = "Invoice";
    ///     options.DefaultThermalWidth = ThermalPaperWidth.Mm80;
    /// });
    /// </code>
    /// </example>
    public static MauiAppBuilder UseMauiPrinting(this MauiAppBuilder builder, Action<PrintingOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new PrintingOptions();
        configure?.Invoke(options);

        builder.Services.AddMauiPrinting(options);
        builder.Services.AddTransient<IMauiInitializeService, PrintingInitializer>();
        return builder;
    }
}
