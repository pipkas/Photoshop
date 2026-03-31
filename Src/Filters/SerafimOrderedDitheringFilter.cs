using System;

namespace Photoshop.Src.Filters;

[Filter("Упорядоченный дизеринг", "Serafim's ordered dithering filter", null)]
[FilterParameter("Красный", Min = 2, Max = 128, Type = ParameterType.Double)]
[FilterParameter("Зелёный", Min = 2, Max = 128, Type = ParameterType.Double)]
[FilterParameter("Синий", Min = 2, Max = 128, Type = ParameterType.Double)]
public class SerafimOrderedDitheringFilter : IFilter
{
    public double[] Parameters { get; private set; } = Array.Empty<double>();

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;
        int rLev = (int)Math.Round(Math.Clamp(parameters[0], 2.0, 128.0));
        int gLev = (int)Math.Round(Math.Clamp(parameters[1], 2.0, 128.0));
        int bLev = (int)Math.Round(Math.Clamp(parameters[2], 2.0, 128.0));
        int rSize = GetSize(rLev);
        int gSize = GetSize(gLev);
        int bSize = GetSize(bLev);
        int[,] rMat = BuildBayer(rSize);
        int[,] gMat = BuildBayer(gSize);
        int[,] bMat = BuildBayer(bSize);
        double rArea = rSize * rSize;
        double gArea = gSize * gSize;
        double bArea = bSize * bSize;
        int w = originalPicture.Width;
        int h = originalPicture.Height;
        Picture result = new Picture(w, h);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                ReadOnlySpan<byte> px = originalPicture.GetPixel(x, y);
                int rId = rMat[y % rSize, x % rSize];
                int gId = gMat[y % gSize, x % gSize];
                int bId = bMat[y % bSize, x % bSize];
                byte nr = Dither(px[0], rLev, rId, rArea);
                byte ng = Dither(px[1], gLev, gId, gArea);
                byte nb = Dither(px[2], bLev, bId, bArea);

                result.SetPixel(x, y, nr, ng, nb);
            }
        }

        return result;
    }

    private static int GetSize(int levels)
    {
        double need = 256.0 / levels;
        int s = 2;

        while (s * s < need)
        {
            s *= 2;
        }

        return s;
    }

    private static int[,] BuildBayer(int size)
    {
        if (size == 2)
        {
            return new int[,]
            {
                { 0, 2 },
                { 3, 1 }
            };
        }

        int half = size / 2;
        int[,] small = BuildBayer(half);
        int[,] mat = new int[size, size];

        for (int y = 0; y < half; y++)
        {
            for (int x = 0; x < half; x++)
            {
                int v = small[y, x] * 4;

                mat[y, x] = v;
                mat[y, x + half] = v + 2;
                mat[y + half, x] = v + 3;
                mat[y + half, x + half] = v + 1;
            }
        }

        return mat;
    }

    private static byte Dither(byte val, int levels, int id, double area)
    {
        double step = 255.0 / (levels - 1);

        double t = (id + 0.5) / area;
        double shift = (t - 0.5) * step;

        double v = Math.Clamp(val + shift, 0.0, 255.0);

        int q = (int)Math.Round(v / step);
        q = Math.Clamp(q, 0, levels - 1);

        int res = (int)Math.Round(q * step);
        return (byte)Math.Clamp(res, 0, 255);
    }
}