using System;

namespace Photoshop.Src.Filters;

[Filter("Билатеральное размытие", "BilateralFilter", "Assets/blur.png")]
[FilterParameter("Размер окна", Min = 3, Max = 11, Type = ParameterType.Integer)]
[FilterParameter("SigmaSpatial", Min = 1, Max = 10, Type = ParameterType.Double)]
[FilterParameter("SigmaRange", Min = 1, Max = 100, Type = ParameterType.Double)]
public class BilateralFilter : IFilter
{
    public double[] Parameters { get; private set; }

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;

        int kernelSize = (int)parameters[0];
        double sigmaSpatial = parameters[1];
        double sigmaRange = parameters[2];

        if (kernelSize % 2 == 0)
        {
            kernelSize++;
        }

        if (kernelSize < 3)
        {
            kernelSize = 3;
        }

        if (kernelSize > 11)
        {
            kernelSize = 11;
        }

        int width = originalPicture.Width;
        int height = originalPicture.Height;
        int radius = kernelSize / 2;

        Picture result = new Picture(width, height);
        
        double[,] lum = new double[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var p = originalPicture.GetPixel(x, y);
                lum[x, y] = p[0] * 0.299 + p[1] * 0.587 + p[2] * 0.114;
            }
        }
        
        double[,] spatial = new double[kernelSize, kernelSize];
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                double dist2 = dx * dx + dy * dy;
                spatial[dy + radius, dx + radius] =
                    Math.Exp(-dist2 / (2 * sigmaSpatial * sigmaSpatial));
            }
        }

        for (int y = radius; y < height - radius; y++)
        {
            for (int x = radius; x < width - radius; x++)
            {
                double sumR = 0, sumG = 0, sumB = 0;
                double norm = 0;

                double centerLum = lum[x, y];

                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        int px = x + dx;
                        int py = y + dy;

                        var p = originalPicture.GetPixel(px, py);

                        double diff = lum[px, py] - centerLum;

                        double range = Math.Exp(-(diff * diff) / (2 * sigmaRange * sigmaRange));

                        double weight = spatial[dy + radius, dx + radius] * range;

                        sumR += p[0] * weight;
                        sumG += p[1] * weight;
                        sumB += p[2] * weight;

                        norm += weight;
                    }
                }

                result.SetPixel(x, y,
                    Clamp((int)(sumR / norm + 0.5)),
                    Clamp((int)(sumG / norm + 0.5)),
                    Clamp((int)(sumB / norm + 0.5)));
            }
        }

        return result;
    }

    private byte Clamp(int v)
    {
        if (v < 0)
        {
            return 0;
        }

        if (v > 255)
        {
            return 255;
        }
        return (byte)v;
    }
}