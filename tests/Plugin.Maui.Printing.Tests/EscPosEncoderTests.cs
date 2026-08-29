namespace Plugin.Maui.Printing.Tests;

public sealed class EscPosEncoderTests
{
    [Fact]
    public void Initialize_emits_esc_at()
    {
        var bytes = new EscPosEncoder().Initialize().ToArray();

        Assert.Equal(new byte[] { 0x1B, (byte)'@' }, bytes);
    }

    [Fact]
    public void Columns_fit_80mm_width()
    {
        var encoder = new EscPosEncoder(ThermalPaperWidth.Mm80);
        encoder.Columns("Left", "Right");
        var text = System.Text.Encoding.ASCII.GetString(encoder.ToArray()).TrimEnd('\n');

        Assert.Equal(48, text.Length);
        Assert.StartsWith("Left", text, StringComparison.Ordinal);
        Assert.EndsWith("Right", text);
    }

    [Fact]
    public void Qr_contains_store_and_print_commands()
    {
        var bytes = new EscPosEncoder().Qr("https://pay.example.com/1").ToArray();

        Assert.Contains((byte)0x31, bytes);
        Assert.Contains((byte)0x50, bytes);
        Assert.Contains((byte)0x51, bytes);
        Assert.Contains("https://pay.example.com/1"u8.ToArray(), bytes);
    }

    [Fact]
    public void Barcode_code128_uses_function_73()
    {
        var bytes = new EscPosEncoder().Barcode("INV1042", BarcodeSymbology.Code128).ToArray();

        Assert.Contains((byte)73, bytes);
        Assert.Contains("INV1042"u8.ToArray(), bytes);
    }

    [Fact]
    public void Raster_includes_dimensions()
    {
        var bits = MonoRaster.FromGray([0, 255, 0, 255], width: 2, height: 2);
        var bytes = new EscPosEncoder().Raster(2, 2, bits).ToArray();

        Assert.Equal(EscPosEncoder.Gs, bytes[0]);
        Assert.Equal((byte)'v', bytes[1]);
        Assert.Equal(1, bytes[4]); // width in bytes
        Assert.Equal(2, bytes[6]); // height
    }

    [Fact]
    public void Cut_feeds_then_gs_v()
    {
        var bytes = new EscPosEncoder().Cut().ToArray();

        Assert.Contains(EscPosEncoder.Gs, bytes);
        Assert.Contains((byte)'V', bytes);
    }
}
