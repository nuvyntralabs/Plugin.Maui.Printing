using Plugin.Maui.Printing;

namespace Plugin.Maui.Printing.Sample;

public partial class MainPage : ContentPage
{
    readonly IPrinter _printer;
    readonly List<string> _log = [];

    public MainPage(IPrinter printer)
    {
        InitializeComponent();
        _printer = printer;
        TargetPicker.ItemsSource = Enum.GetValues<PrinterKind>().Select(value => value.ToString()).ToList();
        TargetPicker.SelectedIndex = 0;
        _printer.PrintCompleted += (_, args) => MainThread.BeginInvokeOnMainThread(() =>
        {
            ResultLabel.Text = $"{args.Result.Status} · {args.Result.ResolvedTarget} · {args.Result.PrinterName}";
            Log($"{args.Result.Status} {args.Result.ContentKind} {args.Result.Message}");
        });
        RefreshAvailability();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshAvailability();
    }

    async void OnDiscoverClicked(object? sender, EventArgs e)
    {
        try
        {
            var printers = await _printer.DiscoverPrintersAsync();
            ResultLabel.Text = printers.Count == 0 ? "No printers" : string.Join(", ", printers.Select(item => item.Name));
            Log($"discover {printers.Count}");
        }
        catch (Exception ex)
        {
            ResultLabel.Text = ex.Message;
        }
    }

    async void OnPermissionsClicked(object? sender, EventArgs e)
    {
        try
        {
            var status = await _printer.RequestPermissionsAsync();
            ResultLabel.Text = status.ToString();
            Log($"permission {status}");
            RefreshAvailability();
        }
        catch (Exception ex)
        {
            ResultLabel.Text = ex.Message;
        }
    }

    async void OnPrintTextClicked(object? sender, EventArgs e) =>
        await PrintAsync(PrintDocument.FromText("Plugin.Maui.Printing\nSystem and thermal text demo.", "Sample text"));

    async void OnPrintPdfClicked(object? sender, EventArgs e) =>
        await PrintAsync(PrintDocument.Pdf(await SampleDocuments.WritePdfAsync(), "Sample PDF"));

    async void OnPrintImageClicked(object? sender, EventArgs e) =>
        await PrintAsync(PrintDocument.Image(await SampleDocuments.WritePngAsync(), "Sample image"));

    async void OnPrintReceiptClicked(object? sender, EventArgs e) =>
        await PrintAsync(new ReceiptDocument { JobName = "Receipt" }
            .Header("ACME STORE")
            .Align(ReceiptAlign.Center).Text(DateTime.Now.ToString("g"))
            .Separator()
            .Align(ReceiptAlign.Left)
            .Columns("Espresso", "3.50")
            .Columns("Croissant", "2.80")
            .Separator()
            .BoldColumns("TOTAL", "6.30")
            .Feed()
            .Qr("https://pay.example.com/r/1042")
            .Align(ReceiptAlign.Center).Text("Thank you")
            .Cut());

    async void OnPrintInvoiceClicked(object? sender, EventArgs e) =>
        await PrintAsync(PrintDocument.Invoice(new InvoiceDocument
        {
            SellerName = "ACME Auto Care",
            SellerAddress = "12 Service Road",
            InvoiceNumber = "INV-1042",
            BuyerName = "Jordan Lee",
            Lines =
            [
                new DocumentLine { Description = "Oil filter", Quantity = 1, UnitPrice = 14.50m },
                new DocumentLine { Description = "Labour", Quantity = 1, UnitPrice = 40.00m }
            ],
            Tax = 5.45m,
            QrPayload = "INV-1042"
        }));

    async void OnPrintLabelClicked(object? sender, EventArgs e) =>
        await PrintAsync(PrintDocument.Label(new LabelDocument
        {
            Title = "SHIP TO",
            Barcode = "PKG44021",
            Fields = new Dictionary<string, string>
            {
                ["SKU"] = "OIL-204",
                ["Bin"] = "A-12"
            }
        }));

    async void OnPrintTicketClicked(object? sender, EventArgs e) =>
        await PrintAsync(PrintDocument.Ticket(new TicketDocument
        {
            EventName = "Pit Lane Pass",
            TicketNumber = "T-88421",
            StartsAt = DateTimeOffset.Now.AddDays(1),
            Seat = "Gate B / Bay 3",
            HolderName = "Jordan Lee",
            QrPayload = "T-88421"
        }));

    async void OnPrintChallanClicked(object? sender, EventArgs e) =>
        await PrintAsync(PrintDocument.DeliveryChallan(new DeliveryChallanDocument
        {
            ChallanNumber = "DC-7781",
            Consignor = "ACME Warehouse",
            Consignee = "City Dealer",
            VehicleNumber = "MH12AB1234",
            Lines =
            [
                new DocumentLine { Description = "Tyres", Quantity = 4, Unit = "pcs" },
                new DocumentLine { Description = "Oil drums", Quantity = 2, Unit = "pcs" }
            ]
        }));

    async void OnPrintInspectionClicked(object? sender, EventArgs e) =>
        await PrintAsync(PrintDocument.InspectionReport(new InspectionReportDocument
        {
            ReportNumber = "IR-3301",
            VehicleIdentifier = "VIN-99ABC",
            Inspector = "Alex Rivera",
            Items =
            [
                new InspectionItem { Name = "Brakes", Result = "Pass" },
                new InspectionItem { Name = "Lights", Result = "Pass" },
                new InspectionItem { Name = "Tyres", Result = "Watch", Notes = "Inner shoulder wear" }
            ],
            Summary = "Roadworthy with tyre watch"
        }));

    async Task PrintAsync(PrintDocument document)
    {
        try
        {
            var result = await _printer.PrintAsync(document, SelectedOptions());
            ResultLabel.Text = $"{result.Status} · {result.ResolvedTarget}";
        }
        catch (Exception ex)
        {
            ResultLabel.Text = ex.Message;
        }
    }

    async Task PrintAsync(ReceiptDocument receipt) =>
        await PrintAsync(PrintDocument.FromReceipt(receipt));

    PrintOptions SelectedOptions() =>
        new()
        {
            Target = Enum.TryParse<PrinterKind>(TargetPicker.SelectedItem?.ToString(), out var target)
                ? target
                : PrinterKind.Auto,
            BluetoothAddress = string.IsNullOrWhiteSpace(AddressEntry.Text) ? null : AddressEntry.Text.Trim()
        };

    void RefreshAvailability()
    {
        var availability = _printer.Availability;
        AvailabilityLabel.Text =
            $"Supported={_printer.IsSupported}  System={availability.CanUseSystemPrinter}  Bluetooth={availability.CanUseBluetooth}  On={availability.BluetoothEnabled}{Environment.NewLine}" +
            $"{availability.Platform}";
    }

    void Log(string line)
    {
        _log.Insert(0, $"{DateTime.Now:HH:mm:ss} {line}");
        if (_log.Count > 12)
            _log.RemoveAt(_log.Count - 1);
        LogLabel.Text = string.Join(Environment.NewLine, _log);
    }
}
