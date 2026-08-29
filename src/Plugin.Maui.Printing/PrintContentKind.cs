namespace Plugin.Maui.Printing;

/// <summary>
/// Payload type carried by a <see cref="PrintDocument"/>.
/// </summary>
public enum PrintContentKind
{
    /// <summary>Portable Document Format.</summary>
    Pdf = 0,

    /// <summary>PNG, JPEG, or other bitmap bytes the platform can decode.</summary>
    Image = 1,

    /// <summary>Plain text.</summary>
    Text = 2,

    /// <summary>Structured receipt lines rendered as ESC/POS or formatted text.</summary>
    Receipt = 3,

    /// <summary>Caller-supplied ESC/POS command bytes.</summary>
    RawEscPos = 4
}
