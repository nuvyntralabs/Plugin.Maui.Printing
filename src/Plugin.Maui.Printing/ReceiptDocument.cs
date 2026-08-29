namespace Plugin.Maui.Printing;

/// <summary>
/// Fluent ESC/POS receipt used for POS, labels, and other 58/80 mm jobs.
/// </summary>
public sealed class ReceiptDocument
{
    readonly List<ReceiptLine> _lines = [];
    ReceiptAlign _align = ReceiptAlign.Left;
    ReceiptTextStyle _style = ReceiptTextStyle.None;

    /// <summary>
    /// Gets or sets the print job name.
    /// </summary>
    public string JobName { get; set; } = "Receipt";

    /// <summary>
    /// Gets the instructions in order.
    /// </summary>
    public IReadOnlyList<ReceiptLine> Lines => _lines;

    /// <summary>
    /// Sets alignment for following text and column lines.
    /// </summary>
    public ReceiptDocument Align(ReceiptAlign align)
    {
        _align = align;
        return this;
    }

    /// <summary>
    /// Sets text emphasis for following text and column lines.
    /// </summary>
    public ReceiptDocument Style(ReceiptTextStyle style)
    {
        _style = style;
        return this;
    }

    /// <summary>
    /// Prints a line of text.
    /// </summary>
    public ReceiptDocument Text(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        _lines.Add(new ReceiptLine
        {
            Kind = ReceiptLineKind.Text,
            Text = text,
            Align = _align,
            Style = _style
        });
        return this;
    }

    /// <summary>
    /// Prints a centered, bold line. Restores the previous style afterwards.
    /// </summary>
    public ReceiptDocument Header(string text)
    {
        var previousAlign = _align;
        var previousStyle = _style;
        _align = ReceiptAlign.Center;
        _style = ReceiptTextStyle.Bold;
        Text(text);
        _align = previousAlign;
        _style = previousStyle;
        return this;
    }

    /// <summary>
    /// Prints a bold line using the current alignment.
    /// </summary>
    public ReceiptDocument Bold(string text)
    {
        var previous = _style;
        _style |= ReceiptTextStyle.Bold;
        Text(text);
        _style = previous;
        return this;
    }

    /// <summary>
    /// Prints left and right text on one line.
    /// </summary>
    public ReceiptDocument Columns(string left, string right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        _lines.Add(new ReceiptLine
        {
            Kind = ReceiptLineKind.Columns,
            Text = left,
            RightText = right,
            Align = ReceiptAlign.Left,
            Style = _style
        });
        return this;
    }

    /// <summary>
    /// Prints bold left and right columns.
    /// </summary>
    public ReceiptDocument BoldColumns(string left, string right)
    {
        var previous = _style;
        _style |= ReceiptTextStyle.Bold;
        Columns(left, right);
        _style = previous;
        return this;
    }

    /// <summary>
    /// Prints a full-width rule.
    /// </summary>
    public ReceiptDocument Separator(char character = '-')
    {
        _lines.Add(new ReceiptLine { Kind = ReceiptLineKind.Separator, SeparatorChar = character });
        return this;
    }

    /// <summary>
    /// Advances the paper.
    /// </summary>
    public ReceiptDocument Feed(int lines = 1)
    {
        if (lines < 1)
            throw new ArgumentOutOfRangeException(nameof(lines));

        _lines.Add(new ReceiptLine { Kind = ReceiptLineKind.Feed, FeedLines = lines });
        return this;
    }

    /// <summary>
    /// Prints a QR code (Model 2).
    /// </summary>
    public ReceiptDocument Qr(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        _lines.Add(new ReceiptLine
        {
            Kind = ReceiptLineKind.QrCode,
            Text = payload,
            Align = ReceiptAlign.Center
        });
        return this;
    }

    /// <summary>
    /// Prints a 1D barcode.
    /// </summary>
    public ReceiptDocument Barcode(string data, BarcodeSymbology symbology = BarcodeSymbology.Code128)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(data);
        _lines.Add(new ReceiptLine
        {
            Kind = ReceiptLineKind.Barcode,
            Text = data,
            BarcodeSymbology = symbology,
            Align = ReceiptAlign.Center
        });
        return this;
    }

    /// <summary>
    /// Prints a 1-bit raster already packed MSB-first, or PNG/JPEG bytes for the platform to decode.
    /// </summary>
    public ReceiptDocument Image(byte[] bytes, int width = 0, int height = 0)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        _lines.Add(new ReceiptLine
        {
            Kind = ReceiptLineKind.Image,
            ImageBytes = bytes,
            ImageWidth = width,
            ImageHeight = height,
            Align = ReceiptAlign.Center
        });
        return this;
    }

    /// <summary>
    /// Cuts the paper. Default is a partial cut.
    /// </summary>
    public ReceiptDocument Cut(bool partial = true)
    {
        _lines.Add(new ReceiptLine { Kind = ReceiptLineKind.Cut, PartialCut = partial });
        return this;
    }

    /// <summary>
    /// Sends a cash-drawer pulse (ESC p).
    /// </summary>
    public ReceiptDocument OpenCashDrawer()
    {
        _lines.Add(new ReceiptLine { Kind = ReceiptLineKind.CashDrawer });
        return this;
    }

    /// <summary>
    /// Adds a pre-built line.
    /// </summary>
    public ReceiptDocument Add(ReceiptLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        _lines.Add(line);
        return this;
    }
}
