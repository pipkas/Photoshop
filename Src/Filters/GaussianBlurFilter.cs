using System;

namespace Photoshop.Src.Filters;

[Filter("Сглаживание (Гаусс)", "GaussianBlurFilter", "Assets/blur.png")]
[FilterParameter("Размер ядра", Min = 3, Max = 5, Type = ParameterType.Integer)]
public class GaussianBlurFilter : IFilter
{
    public double[] Parameters { get; private set; }
    private static readonly double[,] Kernel3x3 = new double[,]
    {
        { 1, 2, 1 },
        { 2, 4, 2 },
        { 1, 2, 1 }
    };

    private static readonly double[,] Kernel5x5 = new double[,]
    {
        { 1,  4,  6,  4, 1 },
        { 4, 16, 24, 16, 4 },
        { 6, 24, 36, 24, 6 },
        { 4, 16, 24, 16, 4 },
        { 1,  4,  6,  4, 1 }
    };
    

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;
        int kernelSize = (int)parameters[0];
        
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

        double[,] kernel;
        double divisor;
        
        switch (kernelSize)
        {
            case 3:
                kernel = (double[,])Kernel3x3.Clone();
                divisor = 16.0; 
                break;
            case 5:
                kernel = (double[,])Kernel5x5.Clone();
                divisor = 256.0; 
                break;
            default:
                kernel = (double[,])Kernel3x3.Clone();
                divisor = 16.0;
                break;
        }

        int size = kernelSize;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                kernel[i, j] /= divisor;
            }
        }

        int width = originalPicture.Width;
        int height = originalPicture.Height;
        int offset = kernelSize / 2;

        Picture result = new Picture(width, height);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double sumR = 0, sumG = 0, sumB = 0;

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
                    }
                }

                result.SetPixel(x, y, ClampToByte((int)Math.Round(sumR)), ClampToByte((int)Math.Round(sumG)), ClampToByte((int)Math.Round(sumB)));
            }
        }

        return result;
    }

    private byte ClampToByte(int value)
    {
        return (byte)Math.Max(0, Math.Min(255, value));
    }
}