using System;
using System.IO;
using SkiaSharp;

namespace Photoshop.Src;

public class Picture: IReadOnlyPicture
{
    public int Width {get; set;}
    public int Height {get; set;}

    //RGBA color
    private byte[] pixelsBuffer;

    public const int ColorBytesCount = 4;

    public ReadOnlySpan<byte> PixelsBuffer => pixelsBuffer;

    int IReadOnlyPicture.Width => Width;

    int IReadOnlyPicture.Height => Height;

    public Picture(int width = 550, int height = 400)
    {
        if (width == 0 || height == 0)
            throw new InvalidOperationException($"Picture has impossible size: {width}x{height}");

        Width = width;
        Height = height;
        pixelsBuffer = new byte[width * height * ColorBytesCount];
        Clean();
    }
    
    public void SetPixel(int x, int y, byte r, byte g, byte b, byte a = 255)
    {
        CheckBounds(x, y);

        int pos = ColorBytesCount * (Width * y + x);

        pixelsBuffer[pos] = r;
        pixelsBuffer[pos + 1] = g;
        pixelsBuffer[pos + 2] = b;
        pixelsBuffer[pos + 3] = a;
    }

    public ReadOnlySpan<byte> GetPixel(int x, int y)
    {
        CheckBounds(x, y);
        var pos = ColorBytesCount * (Width * y + x);
        return pixelsBuffer.AsSpan(pos, ColorBytesCount);
    }

    public void Resize(int newWidth, int newHeight)
    {
        if (newWidth == 0 || newHeight == 0)
            throw new InvalidOperationException($"Picture has impossible size: {newWidth}x{newHeight}");
        
        var newBuffer = new byte[newWidth * newHeight * ColorBytesCount];
        Array.Fill(newBuffer, (byte)255);
        CopyBuffer(pixelsBuffer, newBuffer, newWidth, newHeight);

        pixelsBuffer = newBuffer;
        Width = newWidth;
        Height = newHeight;
    }
    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public void Clean()  => Array.Fill(pixelsBuffer, (byte)255);

    public static Picture LoadFromStream(Stream stream)
    {
        using var bitmap = SKBitmap.Decode(stream) 
            ?? throw new Exception("Impossible to load the file");

        var info = new SKImageInfo(
            bitmap.Width,
            bitmap.Height,
            SKColorType.Rgba8888,
            SKAlphaType.Opaque);

        var picture = new Picture(bitmap.Width, bitmap.Height);

        using var converted = new SKBitmap(info);

        bitmap.CopyTo(converted);

        CopyManager.BitmapToBuffer(
            picture.pixelsBuffer,
            converted,
            bitmap.Width,
            bitmap.Height);
        
        return picture;
    }

    private void CheckBounds(int x, int y)
    {
        if (!IsInBounds(x, y))
            throw new IndexOutOfRangeException($"point ({x}, {y}) is out of bounds [{Width}, {Height}]");
    }
    
    private void CopyBuffer(byte[] oldBuffer, byte[] newBuffer, int newWidth, int newHeight)
    {
        if (oldBuffer == null || newBuffer == null || Width >= newWidth || Height >= newHeight) return;
        
        for (int y = 0; y < Height; y++)
        {   
            Buffer.BlockCopy(
                oldBuffer,
                y * Width * ColorBytesCount,
                newBuffer,
                y * newWidth * ColorBytesCount,
                Width * ColorBytesCount);
        }
    }

}