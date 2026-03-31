using System;

namespace Photoshop.Src.Filters;

[Filter("Сглаживание (Гаусс)", "Gaussian blur filter", "Assets/blur.png")]
[FilterParameter("Размер ядра", Min = 3, Max = 11, Type = ParameterType.OddInteger)]
public class GaussianBlurFilter : IFilter
{
    public double[] Parameters { get; private set; } = Array.Empty<double>();

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;

        int kernelSize = (int)Math.Round(parameters[0]);

        if (kernelSize < 3)
        {
            kernelSize = 3;
        }

        if (kernelSize > 11)
        {
            kernelSize = 11;
        }

        if (kernelSize % 2 == 0)
        {
            kernelSize++;
            if (kernelSize > 11)
            {
                kernelSize = 11;
            }
        }

        double sigma = kernelSize / 3.0;
        double[,] kernel = CreateGaussianKernel(kernelSize, sigma);

        int width = originalPicture.Width;
        int height = originalPicture.Height;
        int offset = kernelSize / 2;

        Picture result = new Picture(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double sumR = 0;
                double sumG = 0;
                double sumB = 0;
                double weightSum = 0;

                for (int ky = -offset; ky <= offset; ky++)
                {
                    int py = y + ky;
                    if (py < 0 || py >= height)
                    {
                        continue;
                    }

                    for (int kx = -offset; kx <= offset; kx++)
                    {
                        int px = x + kx;
                        if (px < 0 || px >= width)
                        {
                            continue;
                        }

                        ReadOnlySpan<byte> pixel = originalPicture.GetPixel(px, py);
                        double weight = kernel[ky + offset, kx + offset];

                        sumR += pixel[0] * weight;
                        sumG += pixel[1] * weight;
                        sumB += pixel[2] * weight;
                        weightSum += weight;
                    }
                }

                if (weightSum > 0)
                {
                    sumR /= weightSum;
                    sumG /= weightSum;
                    sumB /= weightSum;
                }

                result.SetPixel(
                    x,
                    y,
                    ClampToByte((int)Math.Round(sumR)),
                    ClampToByte((int)Math.Round(sumG)),
                    ClampToByte((int)Math.Round(sumB))
                );
            }
        }

        return result;
    }

    private static double[,] CreateGaussianKernel(int size, double sigma)
    {
        double[,] kernel = new double[size, size];
        int radius = size / 2;
        double sigmaSq = sigma * sigma;
        double sum = 0;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                double value = Math.Exp(-(x * x + y * y) / (2 * sigmaSq));
                kernel[y + radius, x + radius] = value;
                sum += value;
            }
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                kernel[y, x] /= sum;
            }
        }

        return kernel;
    }

    private static byte ClampToByte(int value)
    {
        return (byte)Math.Max(0, Math.Min(255, value));
    }
}