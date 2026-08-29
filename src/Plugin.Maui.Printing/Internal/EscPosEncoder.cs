namespace Plugin.Maui.Printing;

/// <summary>
/// ESC/POS command builder shared by tests and both platforms.
/// </summary>
sealed class EscPosEncoder
{
    public const byte Esc = 0x1B;
    public const byte Gs = 0x1D;
    public const byte Lf = 0x0A;

    readonly MemoryStream _buffer = new();
    readonly ThermalPaperWidth _width;

    public EscPosEncoder(ThermalPaperWidth width = ThermalPaperWidth.Mm80)
    {
        _width = width;
    }

    public int LineCharacterWidth => _width == ThermalPaperWidth.Mm58 ? 32 : 48;

    public EscPosEncoder Initialize()
    {
        _buffer.WriteByte(Esc);
        _buffer.WriteByte((byte)'@');
        return this;
    }

    public EscPosEncoder Align(ReceiptAlign align)
    {
        _buffer.WriteByte(Esc);
        _buffer.WriteByte((byte)'a');
        _buffer.WriteByte(align switch
        {
            ReceiptAlign.Center => (byte)1,
            ReceiptAlign.Right => (byte)2,
            _ => (byte)0
        });
        return this;
    }

    public EscPosEncoder Bold(bool on)
    {
        _buffer.WriteByte(Esc);
        _buffer.WriteByte((byte)'E');
        _buffer.WriteByte(on ? (byte)1 : (byte)0);
        return this;
    }

    public EscPosEncoder Underline(bool on)
    {
        _buffer.WriteByte(Esc);
        _buffer.WriteByte((byte)'-');
        _buffer.WriteByte(on ? (byte)1 : (byte)0);
        return this;
    }

    public EscPosEncoder DoubleSize(bool on)
    {
        _buffer.WriteByte(Gs);
        _buffer.WriteByte((byte)'!');
        _buffer.WriteByte(on ? (byte)0x11 : (byte)0x00);
        return this;
    }

    public EscPosEncoder ApplyStyle(ReceiptTextStyle style)
    {
        Bold(style.HasFlag(ReceiptTextStyle.Bold));
        Underline(style.HasFlag(ReceiptTextStyle.Underline));
        DoubleSize(style.HasFlag(ReceiptTextStyle.DoubleSize));
        return this;
    }

    public EscPosEncoder ResetStyle() => ApplyStyle(ReceiptTextStyle.None);

    public EscPosEncoder Text(string text)
    {
        var bytes = EscPosText.Encode(text);
        _buffer.Write(bytes, 0, bytes.Length);
        return this;
    }

    public EscPosEncoder Line(string text)
    {
        Text(text);
        _buffer.WriteByte(Lf);
        return this;
    }

    public EscPosEncoder Columns(string left, string right)
    {
        var width = LineCharacterWidth;
        left ??= string.Empty;
        right ??= string.Empty;

        if (left.Length + right.Length >= width)
        {
            Line(left);
            if (!string.IsNullOrEmpty(right))
                Line(right);
            return this;
        }

        var pad = width - left.Length - right.Length;
        Line(left + new string(' ', pad) + right);
        return this;
    }

    public EscPosEncoder Separator(char character = '-')
    {
        Line(new string(character, LineCharacterWidth));
        return this;
    }

    public EscPosEncoder Feed(int lines = 1)
    {
        if (lines < 1)
            return this;

        _buffer.WriteByte(Esc);
        _buffer.WriteByte((byte)'d');
        _buffer.WriteByte((byte)Math.Clamp(lines, 1, 255));
        return this;
    }

    public EscPosEncoder Cut(bool partial = true)
    {
        Feed(3);
        _buffer.WriteByte(Gs);
        _buffer.WriteByte((byte)'V');
        _buffer.WriteByte(partial ? (byte)1 : (byte)0);
        return this;
    }

    public EscPosEncoder PulseDrawer()
    {
        _buffer.WriteByte(Esc);
        _buffer.WriteByte((byte)'p');
        _buffer.WriteByte(0);
        _buffer.WriteByte(25);
        _buffer.WriteByte(250);
        return this;
    }

    public EscPosEncoder Qr(string payload, int moduleSize = 4)
    {
        var data = Encoding.UTF8.GetBytes(payload);
        var store = 3 + data.Length;

        // Model 2
        Write(Gs, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, 0x32, 0x00);
        // Module size
        Write(Gs, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, (byte)Math.Clamp(moduleSize, 1, 16));
        // Error correction L
        Write(Gs, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x30);
        // Store
        _buffer.WriteByte(Gs);
        _buffer.WriteByte(0x28);
        _buffer.WriteByte(0x6B);
        _buffer.WriteByte((byte)(store & 0xFF));
        _buffer.WriteByte((byte)((store >> 8) & 0xFF));
        _buffer.WriteByte(0x31);
        _buffer.WriteByte(0x50);
        _buffer.WriteByte(0x30);
        _buffer.Write(data, 0, data.Length);
        // Print
        Write(Gs, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30);
        _buffer.WriteByte(Lf);
        return this;
    }

    public EscPosEncoder Barcode(string data, BarcodeSymbology symbology)
    {
        var payload = Encoding.ASCII.GetBytes(data);
        _buffer.WriteByte(Gs);
        _buffer.WriteByte((byte)'h');
        _buffer.WriteByte(80);
        _buffer.WriteByte(Gs);
        _buffer.WriteByte((byte)'k');
        _buffer.WriteByte(symbology switch
        {
            BarcodeSymbology.Code39 => (byte)69,
            BarcodeSymbology.Ean13 => (byte)67,
            _ => (byte)73
        });
        _buffer.WriteByte((byte)payload.Length);
        _buffer.Write(payload, 0, payload.Length);
        _buffer.WriteByte(Lf);
        return this;
    }

    public EscPosEncoder Raster(int widthPixels, int heightPixels, byte[] bits)
    {
        ArgumentNullException.ThrowIfNull(bits);
        var widthBytes = (widthPixels + 7) / 8;
        Write(Gs, (byte)'v', (byte)'0', 0);
        _buffer.WriteByte((byte)(widthBytes & 0xFF));
        _buffer.WriteByte((byte)((widthBytes >> 8) & 0xFF));
        _buffer.WriteByte((byte)(heightPixels & 0xFF));
        _buffer.WriteByte((byte)((heightPixels >> 8) & 0xFF));
        _buffer.Write(bits, 0, bits.Length);
        _buffer.WriteByte(Lf);
        return this;
    }

    public EscPosEncoder Raw(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        _buffer.Write(bytes, 0, bytes.Length);
        return this;
    }

    public byte[] ToArray() => _buffer.ToArray();

    void Write(params byte[] bytes) => _buffer.Write(bytes, 0, bytes.Length);
}

static class EscPosText
{
    public static byte[] Encode(string text)
    {
        var buffer = new byte[text.Length];
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            buffer[i] = ch <= 0x7F ? (byte)ch : Map(ch);
        }

        return buffer;
    }

    static byte Map(char ch) => ch switch
    {
        '£' => 0x9C,
        '€' => (byte)'E',
        '₹' => (byte)'R',
        '°' => 0xF8,
        '•' => 0x07,
        '–' or '—' => (byte)'-',
        '“' or '”' or '‘' or '’' => (byte)'\'',
        _ => (byte)'?'
    };
}
