namespace Plugin.Maui.Printing;

static class BusinessDocumentRenderer
{
    public static ReceiptDocument Invoice(InvoiceDocument invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        var receipt = new ReceiptDocument { JobName = invoice.JobName }
            .Header(invoice.SellerName);

        if (!string.IsNullOrWhiteSpace(invoice.SellerAddress))
            receipt.Align(ReceiptAlign.Center).Text(invoice.SellerAddress);

        receipt
            .Align(ReceiptAlign.Left)
            .Separator()
            .Columns("Invoice", invoice.InvoiceNumber)
            .Columns("Date", invoice.IssuedAt.ToLocalTime().ToString("g"));

        if (!string.IsNullOrWhiteSpace(invoice.BuyerName))
            receipt.Columns("Bill to", invoice.BuyerName);

        receipt.Separator();

        foreach (var line in invoice.Lines)
            receipt.Columns(FormatItem(line), FormatMoney(line.ResolvedAmount));

        receipt
            .Separator()
            .Columns("Subtotal", FormatMoney(invoice.ResolvedSubtotal))
            .Columns("Tax", FormatMoney(invoice.Tax))
            .BoldColumns("TOTAL", FormatMoney(invoice.ResolvedTotal));

        if (!string.IsNullOrWhiteSpace(invoice.Notes))
            receipt.Feed().Text(invoice.Notes);

        if (!string.IsNullOrWhiteSpace(invoice.QrPayload))
            receipt.Feed().Qr(invoice.QrPayload);

        return receipt.Feed().Align(ReceiptAlign.Center).Text("Thank you");
    }

    public static ReceiptDocument Label(LabelDocument label)
    {
        ArgumentNullException.ThrowIfNull(label);
        var receipt = new ReceiptDocument { JobName = label.JobName }
            .Header(label.Title)
            .Separator();

        foreach (var field in label.Fields)
            receipt.Columns(field.Key, field.Value);

        if (!string.IsNullOrWhiteSpace(label.Barcode))
            receipt.Feed().Barcode(label.Barcode, label.BarcodeSymbology);

        if (!string.IsNullOrWhiteSpace(label.QrPayload))
            receipt.Feed().Qr(label.QrPayload);

        return receipt;
    }

    public static ReceiptDocument Ticket(TicketDocument ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        var receipt = new ReceiptDocument { JobName = ticket.JobName }
            .Header(ticket.EventName)
            .Separator();

        if (!string.IsNullOrWhiteSpace(ticket.TicketNumber))
            receipt.Columns("Ticket", ticket.TicketNumber);
        if (ticket.StartsAt is { } start)
            receipt.Columns("When", start.ToLocalTime().ToString("g"));
        if (!string.IsNullOrWhiteSpace(ticket.Seat))
            receipt.Columns("Seat", ticket.Seat);
        if (!string.IsNullOrWhiteSpace(ticket.HolderName))
            receipt.Columns("Name", ticket.HolderName);

        foreach (var field in ticket.Fields)
            receipt.Columns(field.Key, field.Value);

        if (!string.IsNullOrWhiteSpace(ticket.QrPayload))
            receipt.Feed().Qr(ticket.QrPayload);
        else if (!string.IsNullOrWhiteSpace(ticket.TicketNumber))
            receipt.Feed().Barcode(ticket.TicketNumber);

        return receipt.Feed().Align(ReceiptAlign.Center).Text("Admit one");
    }

    public static ReceiptDocument DeliveryChallan(DeliveryChallanDocument challan)
    {
        ArgumentNullException.ThrowIfNull(challan);
        var receipt = new ReceiptDocument { JobName = challan.JobName }
            .Header("DELIVERY CHALLAN")
            .Separator()
            .Columns("Challan", challan.ChallanNumber)
            .Columns("Date", challan.IssuedAt.ToLocalTime().ToString("g"))
            .Columns("From", challan.Consignor)
            .Columns("To", challan.Consignee);

        if (!string.IsNullOrWhiteSpace(challan.VehicleNumber))
            receipt.Columns("Vehicle", challan.VehicleNumber);

        receipt.Separator();

        foreach (var line in challan.Lines)
        {
            var qty = line.Unit is { Length: > 0 }
                ? $"{line.Quantity:0.##} {line.Unit}"
                : line.Quantity.ToString("0.##");
            receipt.Columns(line.Description, qty);
        }

        if (!string.IsNullOrWhiteSpace(challan.Notes))
            receipt.Feed().Text(challan.Notes);

        return receipt.Feed().Text("Received in good condition");
    }

    public static ReceiptDocument InspectionReport(InspectionReportDocument report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var receipt = new ReceiptDocument { JobName = report.JobName }
            .Header("VEHICLE INSPECTION")
            .Separator()
            .Columns("Report", report.ReportNumber)
            .Columns("Vehicle", report.VehicleIdentifier)
            .Columns("Date", report.InspectedAt.ToLocalTime().ToString("g"));

        if (!string.IsNullOrWhiteSpace(report.Inspector))
            receipt.Columns("Inspector", report.Inspector);

        receipt.Separator();

        foreach (var item in report.Items)
        {
            receipt.Columns(item.Name, item.Result);
            if (!string.IsNullOrWhiteSpace(item.Notes))
                receipt.Text("  " + item.Notes);
        }

        if (!string.IsNullOrWhiteSpace(report.Summary))
            receipt.Separator().Bold("Summary").Text(report.Summary);

        return receipt.Feed().Align(ReceiptAlign.Center).Text("End of report");
    }

    static string FormatItem(DocumentLine line)
    {
        if (line.Quantity == 1 && line.UnitPrice is null)
            return line.Description;

        var qty = line.Quantity.ToString("0.##");
        if (line.UnitPrice is { } price)
            return $"{line.Description} ({qty} x {FormatMoney(price)})";

        return $"{line.Description} x {qty}";
    }

    static string FormatMoney(decimal value) => value.ToString("0.00");
}
