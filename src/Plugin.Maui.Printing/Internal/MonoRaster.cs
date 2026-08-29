namespace Plugin.Maui.Printing;

static class MonoRaster
{
    public static byte[] FromGray(byte[] gray, int width, int height, byte threshold = 160)
    {
        ArgumentNullException.ThrowIfNull(gray);
        var rowBytes = (width + 7) / 8;
        var bits = new byte[rowBytes * height];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                if (index >= gray.Length)
                    continue;

                if (gray[index] < threshold)
                    bits[y * rowBytes + (x / 8)] |= (byte)(0x80 >> (x % 8));
            }
        }

        return bits;
    }
}
