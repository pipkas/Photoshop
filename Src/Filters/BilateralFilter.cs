using System;
using System.Threading.Tasks;

namespace Photoshop.Src.Filters;

[Filter("Двусторонний фильтр", "BilateralFilter", "Assets/BilateralFilter.png")]
[FilterParameter("Радиус окна", Min = 1, Max = 10, Type = ParameterType.Integer)]
[FilterParameter("SigmaSpace", Min = 1, Max = 20, Type = ParameterType.Double)]
[FilterParameter("SigmaColor", Min = 1, Max = 150, Type = ParameterType.Double)]
public class BilateralFilter : IFilter
{
    public double[] Parameters { get; private set; }

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;

        int radius = Math.Max(1, (int)parameters[0]);
        float sigmaSpace = (float)Math.Max(0.1, parameters[1]);
        float sigmaColor = (float)Math.Max(0.1, parameters[2]);

        int width = originalPicture.Width;
        int height = originalPicture.Height;

        Picture result = new Picture(width, height);

        byte[,] sourceR = new byte[height, width];
        byte[,] sourceG = new byte[height, width];
        byte[,] sourceB = new byte[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ReadOnlySpan<byte> pixel = originalPicture.GetPixel(x, y);
                sourceR[y, x] = pixel[0];
                sourceG[y, x] = pixel[1];
                sourceB[y, x] = pixel[2];
            }
        }

        float twoSigmaSpaceSq = 2f * sigmaSpace * sigmaSpace;
        float twoSigmaColorSq = 2f * sigmaColor * sigmaColor;

        int size = 2 * radius + 1;
        float[,] spatialKernel = new float[size, size];

        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                float distanceSq = dx * dx + dy * dy;
                spatialKernel[dy + radius, dx + radius] =
                    MathF.Exp(-distanceSq / twoSigmaSpaceSq);
            }
        }

        const int maxColorDistanceSq = 255 * 255 * 3;
        float[] colorKernel = new float[maxColorDistanceSq + 1];

        for (int i = 0; i <= maxColorDistanceSq; i++)
        {
            colorKernel[i] = MathF.Exp(-i / twoSigmaColorSq);
        }

        byte[,] resultR = new byte[height, width];
        byte[,] resultG = new byte[height, width];
        byte[,] resultB = new byte[height, width];

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                byte centerR = sourceR[y, x];
                byte centerG = sourceG[y, x];
                byte centerB = sourceB[y, x];

                float sumR = 0f;
                float sumG = 0f;
                float sumB = 0f;
                float weightSum = 0f;

                int yMin = Math.Max(0, y - radius);
                int yMax = Math.Min(height - 1, y + radius);
                int xMin = Math.Max(0, x - radius);
                int xMax = Math.Min(width - 1, x + radius);

                for (int ny = yMin; ny <= yMax; ny++)
                {
                    int kernelY = ny - y + radius;

                    for (int nx = xMin; nx <= xMax; nx++)
                    {
                        int kernelX = nx - x + radius;

                        byte neighborR = sourceR[ny, nx];
                        byte neighborG = sourceG[ny, nx];
                        byte neighborB = sourceB[ny, nx];

                        int dR = centerR - neighborR;
                        int dG = centerG - neighborG;
                        int dB = centerB - neighborB;

                        int colorDistanceSq = dR * dR + dG * dG + dB * dB;

                        float spatialWeight = spatialKernel[kernelY, kernelX];
                        float rangeWeight = colorKernel[colorDistanceSq];
                        float weight = spatialWeight * rangeWeight;

                        weightSum += weight;
                        sumR += neighborR * weight;
                        sumG += neighborG * weight;
                        sumB += neighborB * weight;
                    }
                }

                resultR[y, x] = ClampToByte(sumR / weightSum);
                resultG[y, x] = ClampToByte(sumG / weightSum);
                resultB[y, x] = ClampToByte(sumB / weightSum);
            }
        });

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                result.SetPixel(x, y, resultR[y, x], resultG[y, x], resultB[y, x]);
            }
        }

        return result;
    }

    private static byte ClampToByte(float value)
    {
        if (value < 0f)
        {
            return 0;
        }

        if (value > 255f)
        {
            return 255;
        }

        return (byte)MathF.Round(value);
    }
}