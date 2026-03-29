using System;
using System.IO;
using SkiaSharp;

namespace Photoshop.Src;

public static class FileManager
{
    public static void SaveToPng(Picture picture, Stream stream)
    {
        var info = new SKImageInfo(
            picture.Width,
            picture.Height,
            SKColorType.Rgba8888,
            SKAlphaType.Opaque);

        using var bitmap = new SKBitmap(info);
        CopyManager.BufferToBitmap(picture.PixelsBuffer, bitmap, 
                        picture.Width, picture.Height);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        data.SaveTo(stream);
    }

}