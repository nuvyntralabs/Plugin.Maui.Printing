namespace Plugin.Maui.Printing;

/// <summary>
/// A printable payload: PDF, image, text, receipt, or raw ESC/POS.
/// </summary>
public sealed class PrintDocument
{
    /// <summary>
    /// Gets the job name shown in the system print UI.
    /// </summary>
    public string JobName { get; init; } = "Print";

    /// <summary>
    /// Gets the payload type.
    /// </summary>
    public PrintContentKind ContentKind { get; init; }

    /// <summary>
    /// Gets the business document kind.
    /// </summary>
    public PrintJobKind JobKind { get; init; } = PrintJobKind.Generic;

    /// <summary>
    /// Gets a local file path for PDF or image content.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Gets in-memory PDF, image, or raw ESC/POS bytes.
    /// </summary>
    public byte[]? Bytes { get; init; }

    /// <summary>
    /// Gets plain text.
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Gets an optional MIME type hint.
    /// </summary>
    public string? MimeType { get; init; }

    /// <summary>
    /// Gets a structured receipt when <see cref="ContentKind"/> is <see cref="PrintContentKind.Receipt"/>.
    /// </summary>
    public ReceiptDocument? Receipt { get; init; }

    /// <summary>
    /// Creates a PDF document from a local file.
    /// </summary>
    public static PrintDocument Pdf(string filePath, string? jobName = null) =>
        new()
        {
            ContentKind = PrintContentKind.Pdf,
            FilePath = filePath,
            JobName = jobName ?? "PDF",
            MimeType = "application/pdf"
        };

    /// <summary>
    /// Creates a PDF document from bytes.
    /// </summary>
    public static PrintDocument Pdf(byte[] bytes, string? jobName = null) =>
        new()
        {
            ContentKind = PrintContentKind.Pdf,
            Bytes = bytes,
            JobName = jobName ?? "PDF",
            MimeType = "application/pdf"
        };

    /// <summary>
    /// Creates an image document from a local file.
    /// </summary>
    public static PrintDocument Image(string filePath, string? jobName = null) =>
        new()
        {
            ContentKind = PrintContentKind.Image,
            FilePath = filePath,
            JobName = jobName ?? "Image"
        };

    /// <summary>
    /// Creates an image document from bytes.
    /// </summary>
    public static PrintDocument Image(byte[] bytes, string? jobName = null) =>
        new()
        {
            ContentKind = PrintContentKind.Image,
            Bytes = bytes,
            JobName = jobName ?? "Image"
        };

    /// <summary>
    /// Creates a plain-text document.
    /// </summary>
    public static PrintDocument FromText(string text, string? jobName = null) =>
        new()
        {
            ContentKind = PrintContentKind.Text,
            Text = text,
            JobName = jobName ?? "Text",
            MimeType = "text/plain"
        };

    /// <summary>
    /// Creates a receipt document.
    /// </summary>
    public static PrintDocument FromReceipt(ReceiptDocument receipt, string? jobName = null) =>
        new()
        {
            ContentKind = PrintContentKind.Receipt,
            JobKind = PrintJobKind.Receipt,
            Receipt = receipt,
            JobName = jobName ?? receipt.JobName
        };

    /// <summary>
    /// Creates a raw ESC/POS payload.
    /// </summary>
    public static PrintDocument RawEscPos(byte[] commands, string? jobName = null) =>
        new()
        {
            ContentKind = PrintContentKind.RawEscPos,
            Bytes = commands,
            JobName = jobName ?? "ESC/POS"
        };

    /// <summary>
    /// Creates an invoice from a structured model.
    /// </summary>
    public static PrintDocument Invoice(InvoiceDocument invoice) =>
        FromBusiness(PrintJobKind.Invoice, invoice.JobName, BusinessDocumentRenderer.Invoice(invoice));

    /// <summary>
    /// Creates a label from a structured model.
    /// </summary>
    public static PrintDocument Label(LabelDocument label) =>
        FromBusiness(PrintJobKind.Label, label.JobName, BusinessDocumentRenderer.Label(label));

    /// <summary>
    /// Creates a ticket from a structured model.
    /// </summary>
    public static PrintDocument Ticket(TicketDocument ticket) =>
        FromBusiness(PrintJobKind.Ticket, ticket.JobName, BusinessDocumentRenderer.Ticket(ticket));

    /// <summary>
    /// Creates a delivery challan from a structured model.
    /// </summary>
    public static PrintDocument DeliveryChallan(DeliveryChallanDocument challan) =>
        FromBusiness(PrintJobKind.DeliveryChallan, challan.JobName, BusinessDocumentRenderer.DeliveryChallan(challan));

    /// <summary>
    /// Creates a vehicle inspection report from a structured model.
    /// </summary>
    public static PrintDocument InspectionReport(InspectionReportDocument report) =>
        FromBusiness(PrintJobKind.InspectionReport, report.JobName, BusinessDocumentRenderer.InspectionReport(report));

    static PrintDocument FromBusiness(PrintJobKind jobKind, string jobName, ReceiptDocument receipt) =>
        new()
        {
            ContentKind = PrintContentKind.Receipt,
            JobKind = jobKind,
            Receipt = receipt,
            JobName = jobName
        };
}
