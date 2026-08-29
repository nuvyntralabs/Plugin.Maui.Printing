namespace Plugin.Maui.Printing;

/// <summary>
/// System-printer paper size. Thermal jobs use <see cref="ThermalPaperWidth"/> instead.
/// </summary>
public enum PaperSize
{
    /// <summary>ISO A4.</summary>
    A4 = 0,

    /// <summary>US Letter.</summary>
    Letter = 1,

    /// <summary>ISO A5.</summary>
    A5 = 2
}
