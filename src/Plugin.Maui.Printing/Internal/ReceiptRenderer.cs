namespace Plugin.Maui.Printing;

static class ReceiptRenderer
{
    public static byte[] Encode(ReceiptDocument receipt, ThermalPaperWidth width, bool cutPaper, bool openCashDrawer)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        var encoder = new EscPosEncoder(width).Initialize();

        foreach (var line in receipt.Lines)
            Write(encoder, line);

        if (openCashDrawer && receipt.Lines.All(line => line.Kind != ReceiptLineKind.CashDrawer))
            encoder.PulseDrawer();

        if (cutPaper && receipt.Lines.All(line => line.Kind != ReceiptLineKind.Cut))
            encoder.Cut();

        return encoder.ToArray();
    }

    public static string ToPlainText(ReceiptDocument receipt, ThermalPaperWidth width = ThermalPaperWidth.Mm80)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        var columns = width == ThermalPaperWidth.Mm58 ? 32 : 48;
        var builder = new StringBuilder();

        foreach (var line in receipt.Lines)
        {
            switch (line.Kind)
            {
                case ReceiptLineKind.Text:
                    builder.AppendLine(AlignText(line.Text ?? string.Empty, line.Align, columns));
                    break;
                case ReceiptLineKind.Columns:
                    builder.AppendLine(ColumnText(line.Text ?? string.Empty, line.RightText ?? string.Empty, columns));
                    break;
                case ReceiptLineKind.Separator:
                    builder.AppendLine(new string(line.SeparatorChar, columns));
                    break;
                case ReceiptLineKind.Feed:
                    builder.Append(new string('\n', Math.Max(1, line.FeedLines)));
                    break;
                case ReceiptLineKind.QrCode:
                    builder.AppendLine($"[QR] {line.Text}");
                    break;
                case ReceiptLineKind.Barcode:
                    builder.AppendLine($"[{line.BarcodeSymbology}] {line.Text}");
                    break;
                case ReceiptLineKind.Image:
                    builder.AppendLine("[image]");
                    break;
                case ReceiptLineKind.Cut:
                    builder.AppendLine("--- cut ---");
                    break;
            }
        }

        return builder.ToString();
    }

    static void Write(EscPosEncoder encoder, ReceiptLine line)
    {
        switch (line.Kind)
        {
            case ReceiptLineKind.Text:
                encoder.Align(line.Align).ApplyStyle(line.Style).Line(line.Text ?? string.Empty).ResetStyle();
                break;
            case ReceiptLineKind.Columns:
                encoder.Align(ReceiptAlign.Left).ApplyStyle(line.Style)
                    .Columns(line.Text ?? string.Empty, line.RightText ?? string.Empty)
                    .ResetStyle();
                break;
            case ReceiptLineKind.Separator:
                encoder.Align(ReceiptAlign.Left).Separator(line.SeparatorChar);
                break;
            case ReceiptLineKind.Feed:
                encoder.Feed(line.FeedLines);
                break;
            case ReceiptLineKind.QrCode:
                encoder.Align(ReceiptAlign.Center).Qr(line.Text ?? string.Empty);
                break;
            case ReceiptLineKind.Barcode:
                encoder.Align(ReceiptAlign.Center).Barcode(line.Text ?? string.Empty, line.BarcodeSymbology);
                break;
            case ReceiptLineKind.Image when line.ImageWidth > 0 && line.ImageHeight > 0 && line.ImageBytes is { Length: > 0 }:
                encoder.Align(ReceiptAlign.Center).Raster(line.ImageWidth, line.ImageHeight, line.ImageBytes);
                break;
            case ReceiptLineKind.Image:
                encoder.Align(ReceiptAlign.Center).Line("[image]");
                break;
            case ReceiptLineKind.Cut:
                encoder.Cut(line.PartialCut);
                break;
            case ReceiptLineKind.CashDrawer:
                encoder.PulseDrawer();
                break;
        }
    }

    static string AlignText(string text, ReceiptAlign align, int columns)
    {
        if (text.Length >= columns || align == ReceiptAlign.Left)
            return text;

        if (align == ReceiptAlign.Right)
            return text.PadLeft(columns);

        var pad = (columns - text.Length) / 2;
        return new string(' ', pad) + text;
    }

    static string ColumnText(string left, string right, int columns)
    {
        if (left.Length + right.Length >= columns)
            return left + Environment.NewLine + right;

        return left + new string(' ', columns - left.Length - right.Length) + right;
    }
}
