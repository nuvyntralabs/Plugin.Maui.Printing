namespace Plugin.Maui.Printing;

/// <summary>
/// Registers printing services without MAUI lifecycle hooks.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="IPrinter"/> using the supplied options instance.
    /// </summary>
    public static IServiceCollection AddMauiPrinting(this IServiceCollection services, PrintingOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);
        services.TryAddSingleton<IPrinter>(sp =>
        {
            var resolved = sp.GetService<PrintingOptions>() ?? options;
            var printer = Printer.Create(resolved);
            Printer.SetDefault(printer);
            return printer;
        });

        return services;
    }

    /// <summary>
    /// Adds <see cref="IPrinter"/> and applies <paramref name="configure"/> to a new options instance.
    /// </summary>
    public static IServiceCollection AddMauiPrinting(this IServiceCollection services, Action<PrintingOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new PrintingOptions();
        configure?.Invoke(options);
        return services.AddMauiPrinting(options);
    }
}
