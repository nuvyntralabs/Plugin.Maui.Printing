namespace Plugin.Maui.Printing.Sample;

static class SampleDocuments
{
    public static async Task<string> WritePdfAsync()
    {
        var path = Path.Combine(FileSystem.CacheDirectory, "printing-sample.pdf");
        await File.WriteAllBytesAsync(path, MinimalPdf.Create("Plugin.Maui.Printing", "Invoice / receipt / label demo"));
        return path;
    }

    public static async Task<string> WritePngAsync()
    {
        var path = Path.Combine(FileSystem.CacheDirectory, "printing-sample.png");
        await File.WriteAllBytesAsync(path, Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAQAAAAEAAQMAAABrrFhUAAAABlBMVEUAAAB/AAD//3p5AAAAKklEQVR4nO3BAQ0AAADCoPdPbQ8HFAAAAAAAAAAAAAAAAAAAAAAAAAAAfA8xAAAB9xG4AAAAAElFTkSuQmCC"));
        return path;
    }
}

static class MinimalPdf
{
    public static byte[] Create(string title, string body)
    {
        var content = $"BT /F1 18 Tf 72 720 Td ({Escape(title)}) Tj 0 -28 Td /F1 12 Tf ({Escape(body)}) Tj ET";
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
            $"<< /Length {content.Length} >>\nstream\n{content}\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, new System.Text.UTF8Encoding(false), leaveOpen: true);
        writer.NewLine = "\n";
        writer.WriteLine("%PDF-1.1");
        writer.Flush();
        var offsets = new List<long> { 0 };
        for (var i = 0; i < objects.Length; i++)
        {
            offsets.Add(stream.Position);
            writer.WriteLine($"{i + 1} 0 obj");
            writer.WriteLine(objects[i]);
            writer.WriteLine("endobj");
            writer.Flush();
        }

        var startxref = stream.Position;
        writer.WriteLine($"xref");
        writer.WriteLine($"0 {objects.Length + 1}");
        writer.WriteLine("0000000000 65535 f ");
        for (var i = 1; i < offsets.Count; i++)
            writer.WriteLine($"{offsets[i]:D10} 00000 n ");
        writer.WriteLine("trailer");
        writer.WriteLine($"<< /Size {objects.Length + 1} /Root 1 0 R >>");
        writer.WriteLine("startxref");
        writer.WriteLine(startxref);
        writer.WriteLine("%%EOF");
        writer.Flush();
        return stream.ToArray();
    }

    static string Escape(string value) => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
}
