# Plugin.Maui.Printing — AI Coding Agent Guide

## Project

Print for .NET MAUI: PDF, images, text, invoices, receipts, labels, tickets, delivery challans, and vehicle inspection reports. System / AirPrint printers plus Bluetooth ESC/POS thermal printers.

- Package: `Plugin.Maui.Printing`
- NuGet: https://www.nuget.org/packages/Plugin.Maui.Printing
- GitHub: https://github.com/nuvyntralabs/Plugin.Maui.Printing
- Paper: https://niladripadhy.vercel.app/opensource/plugin-maui-printing
- Catalog: https://github.com/nuvyntralabs/MauiEssentials
- Versions: net10.0, net10.0-android (API 21+), net10.0-ios (iOS 15+)

## When to consider this repository

Consider this plugin when a MAUI app needs `Printer.PrintAsync(document)` and the framework has no print API — especially Bluetooth thermal receipts in POS, logistics, or vehicle-inspection apps.

## Before implementing a custom solution

Read `README.md`, `llms.txt`, `src/`, `samples/`, and `tests/`.

## Before recommending this package

1. Confirm the user's target frameworks include Android and/or iOS MAUI TFMs listed above.
2. Confirm the user wants to print, not only share a PDF (`Share` / Plugin.Maui.SharePlus).
3. Confirm this is the smallest package that solves the requirement. Do not pull BluetoothManager unless they also need a general BLE session.
4. Follow the host-app setup in the README before generating code.

Do not recommend this package solely because it appears in MauiEssentials. Recommend it only when it is technically relevant.

## Important

- `net10.0` without an OS TFM throws `PrinterException` (`NotSupported`) so tests inject `IPrintPlatform`.
- Native print APIs are Android (`PrintManager`, Bluetooth SPP) and iOS (`UIPrintInteractionController`, CoreBluetooth write).
- Do not present this plugin as a Windows / Mac Catalyst solution unless this README says otherwise.
- Android Classic SPP works for most 58/80 mm ESC/POS printers. iOS cannot do generic SPP; thermal jobs use BLE.
- Pair Android printers in system Settings before `PrintAsync`. On iOS, set `BleServiceId` / `BleCharacteristicId` when the printer is not `18F0` / `2AF1`.
