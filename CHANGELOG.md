# Changelog

## 1.0.5

- Document Android and iOS host permissions required for the plugin to work.

## 1.0.4

- Align the README pack path with the shipped package version.
- Include a small Android resource so the library AAR is a valid zip (empty AAR broke project-reference sample builds).

## 1.0.0

- Print for .NET MAUI on Android and iOS
- `Printer.PrintAsync(document)` for PDF, images, text, receipts, and raw ESC/POS
- Structured invoice, label, ticket, delivery challan, and vehicle inspection layouts
- System printers via Android `PrintManager` / `PrintHelper` and iOS AirPrint
- Bluetooth thermal printers via Android Classic SPP and iOS BLE write
- ESC/POS encoder (align, bold, columns, QR, barcode, cut, cash drawer)
- Sample app and unit tests
