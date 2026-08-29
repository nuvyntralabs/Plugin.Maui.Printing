namespace Plugin.Maui.Printing.Tests;

public sealed class PrinterTests
{
    [Fact]
    public async Task PrintAsync_pdf_uses_system_printer()
    {
        var (printer, platform) = Harness.Create();
        var path = Harness.WriteTempFile();

        var result = await printer.PrintAsync(PrintDocument.Pdf(path, "Invoice"));

        Assert.True(result.Completed);
        Assert.Equal("system", platform.LastMode);
        Assert.Equal(PrintContentKind.Pdf, result.ContentKind);
        Assert.Equal(PrinterKind.System, result.ResolvedTarget);
        Assert.Equal("Invoice", platform.LastJob?.JobName);
        Assert.Equal(path, platform.LastJob?.FilePath);
    }

    [Fact]
    public async Task PrintAsync_receipt_uses_thermal_and_encodes_escpos()
    {
        var (printer, platform) = Harness.Create();
        var receipt = new ReceiptDocument { JobName = "Shop" }
            .Header("ACME")
            .Columns("Oil filter", "14.50")
            .Cut();

        var result = await printer.PrintReceiptAsync(receipt);

        Assert.True(result.Completed);
        Assert.Equal("bluetooth", platform.LastMode);
        Assert.Equal(PrintContentKind.Receipt, result.ContentKind);
        Assert.Equal(PrinterKind.Thermal, result.ResolvedTarget);
        Assert.NotNull(platform.LastPayload);
        Assert.Contains(EscPosEncoder.Esc, platform.LastPayload);
        Assert.Contains(EscPosEncoder.Gs, platform.LastPayload);
    }

    [Fact]
    public async Task PrintAsync_invoice_is_thermal_by_default()
    {
        var (printer, platform) = Harness.Create();
        var document = PrintDocument.Invoice(new InvoiceDocument
        {
            SellerName = "ACME Auto",
            InvoiceNumber = "INV-1042",
            Lines = [new DocumentLine { Description = "Labour", Quantity = 1, UnitPrice = 40 }],
            Tax = 2
        });

        await printer.PrintAsync(document);

        Assert.Equal("bluetooth", platform.LastMode);
        Assert.Equal(PrintJobKind.Invoice, platform.LastJob?.Document.JobKind);
        Assert.Contains("INV-1042"u8.ToArray(), platform.LastPayload!);
    }

    [Fact]
    public async Task PrintAsync_text_can_override_to_system()
    {
        var (printer, platform) = Harness.Create();

        await printer.PrintTextAsync("hello", new PrintOptions { Target = PrinterKind.System });

        Assert.Equal("system", platform.LastMode);
        Assert.Equal("hello", platform.LastJob?.Text);
    }

    [Fact]
    public async Task PrintAsync_empty_text_throws()
    {
        var (printer, _) = Harness.Create();

        var error = await Assert.ThrowsAsync<PrinterException>(() => printer.PrintTextAsync("  "));
        Assert.Equal(PrinterError.InvalidRequest, error.Error);
    }

    [Fact]
    public async Task PrintAsync_missing_file_throws()
    {
        var (printer, _) = Harness.Create();

        var error = await Assert.ThrowsAsync<PrinterException>(
            () => printer.PrintPdfAsync(Path.Combine(Path.GetTempPath(), "missing-print.pdf")));
        Assert.Equal(PrinterError.FileNotFound, error.Error);
    }

    [Fact]
    public async Task PrintAsync_unsupported_platform_throws()
    {
        var options = new PrintingOptions();
        var platform = new FakePrintPlatform { IsSupported = false };
        var printer = Printer.Create(options, platform);

        var error = await Assert.ThrowsAsync<PrinterException>(() => printer.PrintTextAsync("hello"));
        Assert.Equal(PrinterError.NotSupported, error.Error);
    }

    [Fact]
    public async Task DiscoverPrintersAsync_returns_platform_list()
    {
        var (printer, _) = Harness.Create();

        var printers = await printer.DiscoverPrintersAsync();

        Assert.Equal(2, printers.Count);
        Assert.Contains(printers, item => item.Kind == PrinterKind.Thermal && item.SupportsEscPos);
    }

    [Fact]
    public async Task PrintCompleted_is_raised()
    {
        var (printer, _) = Harness.Create();
        PrintResult? observed = null;
        printer.PrintCompleted += (_, args) => observed = args.Result;

        await printer.PrintTextAsync("ping", new PrintOptions { Target = PrinterKind.System });

        Assert.NotNull(observed);
        Assert.True(observed!.Completed);
    }

    [Fact]
    public async Task Unavailable_printer_can_throw()
    {
        var (printer, platform) = Harness.Create(options => options.ThrowWhenPrinterUnavailable = true);
        platform.NextResult = PrintResult.Unavailable(PrintContentKind.Receipt, PrinterKind.Thermal, PrinterKind.Thermal);

        var error = await Assert.ThrowsAsync<PrinterException>(() =>
            printer.PrintReceiptAsync(new ReceiptDocument().Text("hi")));
        Assert.Equal(PrinterError.PrinterUnavailable, error.Error);
    }

    [Fact]
    public void EncodeEscPos_is_public_preview()
    {
        var (printer, _) = Harness.Create();
        var bytes = printer.EncodeEscPos(new ReceiptDocument().Header("ACME").Text("ok"));

        Assert.True(bytes.Length > 4);
        Assert.Equal(EscPosEncoder.Esc, bytes[0]);
        Assert.Equal((byte)'@', bytes[1]);
    }
}
