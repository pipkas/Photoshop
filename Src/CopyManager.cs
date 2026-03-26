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

                if (bitmap.ColorType == SKColorType.Bgra8888)
                {
                    for (int i = 0; i < pixelBytesPerRow; i += 4)
                    {
                        dst[i]     = src[i + 2]; // B
                        dst[i + 1] = src[i + 1]; // G
                        dst[i + 2] = src[i];     // R
                        dst[i + 3] = src[i + 3]; // A
                    }
                    
                }
                else
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

                if (bitmap.ColorType == SKColorType.Bgra8888)
                {
                    for (int i = 0; i < pixelBytesPerRow; i += 4)
                    {
                        dst[i]     = src[i + 2]; // B
                        dst[i + 1] = src[i + 1]; // G
                        dst[i + 2] = src[i];     // R
                        dst[i + 3] = src[i + 3]; // A
                    }
                }
                else
                    src.CopyTo(dst);
            }
        }
    }
}