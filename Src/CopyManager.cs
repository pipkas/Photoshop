using System;
using SkiaSharp;

namespace Photoshop.Src;

public static class CopyManager
{
    public static void BufferToBitmap(ReadOnlySpan<byte> buffer, SKBitmap bitmap, int width, int height)
    {
        var ptr = bitmap.GetPixels();
        var rowBytes = bitmap.Info.RowBytes;
        var pixelBytesPerRow = width * 4;

        unsafe
        {
            var bitmapSpan = new Span<byte>((void*)ptr, rowBytes * height);

            for (int y = 0; y < height; y++)
            {
                var src = buffer.Slice(y * pixelBytesPerRow, pixelBytesPerRow);
                var dst = bitmapSpan.Slice(y * rowBytes, pixelBytesPerRow);

                src.CopyTo(dst);
            }
        }
    }

    public static void BitmapToBuffer(Span<byte> buffer, SKBitmap bitmap, int width, int height)
    {
        var ptr = bitmap.GetPixels();
        var rowBytes = bitmap.Info.RowBytes;
        var pixelBytesPerRow = width * 4;

        unsafe
        {
            var bitmapSpan = new Span<byte>((void*)ptr, rowBytes * height);

            for (int y = 0; y < height; y++)
            {
                var src = bitmapSpan.Slice(y * rowBytes, pixelBytesPerRow);
                var dst = buffer.Slice(y * pixelBytesPerRow, pixelBytesPerRow);

                src.CopyTo(dst);
            }
        }
    }
}