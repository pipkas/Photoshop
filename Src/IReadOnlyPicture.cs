using System;

namespace Photoshop.Src;

public interface IReadOnlyPicture
{
    int Width { get; }
    int Height { get; }
    ReadOnlySpan<byte> PixelsBuffer { get; }
    ReadOnlySpan<byte> GetPixel(int x, int y);
    bool IsInBounds(int x, int y);
}