using System;
using System.Drawing;

namespace Photoshop.Src;

public static class FitToScreenManager
{
    public static Picture FitToScreen(InterpolationType interpolType, Picture image, Size newImageSize)
    {
        var result = new Picture(newImageSize.Width, newImageSize.Height);

        float scale = Math.Min(
            (float)newImageSize.Width / image.Width,
            (float)newImageSize.Height / image.Height
        );

        int drawWidth = (int)(image.Width * scale);
        int drawHeight = (int)(image.Height * scale);

        int offsetX = (newImageSize.Width - drawWidth) / 2;
        int offsetY = (newImageSize.Height - drawHeight) / 2;

        for (int y = 0; y < newImageSize.Height; y++)
        {
            for (int x = 0; x < newImageSize.Width; x++)
            {
                // вне изображения — просто фон
                if (x < offsetX || x >= offsetX + drawWidth ||
                    y < offsetY || y >= offsetY + drawHeight)
                {
                    continue;
                }

                float srcX = (x - offsetX) / scale;
                float srcY = (y - offsetY) / scale;

                byte r, g, b, a;

                switch (interpolType)
                {
                    case InterpolationType.Step:
                        GetNearest(image, srcX, srcY, out r, out g, out b, out a);
                        break;

                    case InterpolationType.Bilinear:
                        GetBilinear(image, srcX, srcY, out r, out g, out b, out a);
                        break;

                    case InterpolationType.Cubic:
                        GetBicubic(image, srcX, srcY, out r, out g, out b, out a);
                        break;

                    default:
                        continue;
                }

                result.SetPixel(x, y, r, g, b, a);
            }
        }

        return result;
    }

    private static void GetNearest(Picture img, float x, float y, 
    out byte r, out byte g, out byte b, out byte a)
    {
        int ix = (int)Math.Round(x);
        int iy = (int)Math.Round(y);

        ix = Math.Clamp(ix, 0, img.Width - 1);
        iy = Math.Clamp(iy, 0, img.Height - 1);

        var p = img.GetPixel(ix, iy);
        r = p[0]; g = p[1]; b = p[2]; a = p[3];
    }

    private static void GetBilinear(Picture img, float x, float y,
    out byte r, out byte g, out byte b, out byte a)
    {
        int x0 = (int)Math.Floor(x);
        int y0 = (int)Math.Floor(y);
        int x1 = Math.Min(x0 + 1, img.Width - 1);
        int y1 = Math.Min(y0 + 1, img.Height - 1);

        float dx = x - x0;
        float dy = y - y0;

        var p00 = img.GetPixel(x0, y0);
        var p10 = img.GetPixel(x1, y0);
        var p01 = img.GetPixel(x0, y1);
        var p11 = img.GetPixel(x1, y1);

        r = (byte)(
            p00[0] * (1 - dx) * (1 - dy) +
            p10[0] * dx * (1 - dy) +
            p01[0] * (1 - dx) * dy +
            p11[0] * dx * dy);

        g = (byte)(
            p00[1] * (1 - dx) * (1 - dy) +
            p10[1] * dx * (1 - dy) +
            p01[1] * (1 - dx) * dy +
            p11[1] * dx * dy);

        b = (byte)(
            p00[2] * (1 - dx) * (1 - dy) +
            p10[2] * dx * (1 - dy) +
            p01[2] * (1 - dx) * dy +
            p11[2] * dx * dy);

        a = (byte)(
            p00[3] * (1 - dx) * (1 - dy) +
            p10[3] * dx * (1 - dy) +
            p01[3] * (1 - dx) * dy +
            p11[3] * dx * dy);
    }

    private static void GetBicubic(Picture img, float x, float y,
    out byte r, out byte g, out byte b, out byte a)
    {
        int ix = (int)Math.Floor(x);
        int iy = (int)Math.Floor(y);

        float tx = x - ix;
        float ty = y - iy;

        float[] arrR = new float[4];
        float[] arrG = new float[4];
        float[] arrB = new float[4];
        float[] arrA = new float[4];

        for (int m = -1; m <= 2; m++)
        {
            float[] colR = new float[4];
            float[] colG = new float[4];
            float[] colB = new float[4];
            float[] colA = new float[4];

            for (int n = -1; n <= 2; n++)
            {
                int px = Math.Clamp(ix + n, 0, img.Width - 1);
                int py = Math.Clamp(iy + m, 0, img.Height - 1);

                var p = img.GetPixel(px, py);

                colR[n + 1] = p[0];
                colG[n + 1] = p[1];
                colB[n + 1] = p[2];
                colA[n + 1] = p[3];
            }

            arrR[m + 1] = Cubic(colR[0], colR[1], colR[2], colR[3], tx);
            arrG[m + 1] = Cubic(colG[0], colG[1], colG[2], colG[3], tx);
            arrB[m + 1] = Cubic(colB[0], colB[1], colB[2], colB[3], tx);
            arrA[m + 1] = Cubic(colA[0], colA[1], colA[2], colA[3], tx);
        }

        r = (byte)Math.Clamp(Cubic(arrR[0], arrR[1], arrR[2], arrR[3], ty), 0, 255);
        g = (byte)Math.Clamp(Cubic(arrG[0], arrG[1], arrG[2], arrG[3], ty), 0, 255);
        b = (byte)Math.Clamp(Cubic(arrB[0], arrB[1], arrB[2], arrB[3], ty), 0, 255);
        a = (byte)Math.Clamp(Cubic(arrA[0], arrA[1], arrA[2], arrA[3], ty), 0, 255);
    }

    private static float Cubic(float v0, float v1, float v2, float v3, float t)
    {
        float a0 = -0.5f*v0 + 1.5f*v1 - 1.5f*v2 + 0.5f*v3;
        float a1 = v0 - 2.5f*v1 + 2f*v2 - 0.5f*v3;
        float a2 = -0.5f*v0 + 0.5f*v2;
        float a3 = v1;

        return ((a0 * t + a1) * t + a2) * t + a3;
    }
}