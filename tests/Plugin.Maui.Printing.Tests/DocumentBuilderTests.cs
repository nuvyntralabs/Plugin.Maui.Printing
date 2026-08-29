namespace Plugin.Maui.Printing.Tests;

public sealed class DocumentBuilderTests
{
    [Fact]
    public void Invoice_includes_total_and_number()
    {
        var receipt = BusinessDocumentRenderer.Invoice(new InvoiceDocument
        {
            SellerName = "ACME",
            InvoiceNumber = "INV-9",
            Lines =
            [
                new DocumentLine { Description = "Filter", Quantity = 2, UnitPrice = 10 }
            ],
            Tax = 2
        });

        var text = ReceiptRenderer.ToPlainText(receipt);

        Assert.Contains("INV-9", text, StringComparison.Ordinal);
        Assert.Contains("22.00", text, StringComparison.Ordinal);
        Assert.Contains("TOTAL", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Label_includes_barcode_field()
    {
        var receipt = BusinessDocumentRenderer.Label(new LabelDocument
        {
            Title = "SKU-100",
            Barcode = "SKU-100",
            Fields = new Dictionary<string, string> { ["Bin"] = "A-12" }
        });

        var text = ReceiptRenderer.ToPlainText(receipt);
        Assert.Contains("SKU-100", text, StringComparison.Ordinal);
        Assert.Contains("A-12", text, StringComparison.Ordinal);
        Assert.Contains("[Code128]", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Ticket_includes_seat_and_admit()
    {
        var receipt = BusinessDocumentRenderer.Ticket(new TicketDocument
        {
            EventName = "Grand Prix",
            TicketNumber = "T-88",
            Seat = "A12",
            QrPayload = "T-88"
        });

        var text = ReceiptRenderer.ToPlainText(receipt);
        Assert.Contains("Grand Prix", text, StringComparison.Ordinal);
        Assert.Contains("A12", text, StringComparison.Ordinal);
        Assert.Contains("Admit one", text, StringComparison.Ordinal);
    }

    [Fact]
    public void DeliveryChallan_lists_qty()
    {
        var receipt = BusinessDocumentRenderer.DeliveryChallan(new DeliveryChallanDocument
        {
            ChallanNumber = "DC-1",
            Consignor = "Warehouse",
            Consignee = "Dealer",
            VehicleNumber = "MH12AB1234",
            Lines = [new DocumentLine { Description = "Tyres", Quantity = 4, Unit = "pcs" }]
        });

        var text = ReceiptRenderer.ToPlainText(receipt);
        Assert.Contains("DC-1", text, StringComparison.Ordinal);
        Assert.Contains("4 pcs", text, StringComparison.Ordinal);
        Assert.Contains("MH12AB1234", text, StringComparison.Ordinal);
    }

    [Fact]
    public void InspectionReport_lists_results()
    {
        var receipt = BusinessDocumentRenderer.InspectionReport(new InspectionReportDocument
        {
            ReportNumber = "IR-3",
            VehicleIdentifier = "VIN-99",
            Inspector = "Alex",
            Items = [new InspectionItem { Name = "Brakes", Result = "Pass", Notes = "OK" }],
            Summary = "Roadworthy"
        });

        var text = ReceiptRenderer.ToPlainText(receipt);
        Assert.Contains("VIN-99", text, StringComparison.Ordinal);
        Assert.Contains("Pass", text, StringComparison.Ordinal);
        Assert.Contains("Roadworthy", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Receipt_fluent_api_keeps_order()
    {
        var receipt = new ReceiptDocument()
            .Header("Shop")
            .Separator()
            .Columns("A", "1")
            .Qr("x")
            .Cut();

        Assert.Equal(5, receipt.Lines.Count);
        Assert.Equal(ReceiptLineKind.Text, receipt.Lines[0].Kind);
        Assert.Equal(ReceiptLineKind.QrCode, receipt.Lines[3].Kind);
        Assert.Equal(ReceiptLineKind.Cut, receipt.Lines[4].Kind);
    }
}
