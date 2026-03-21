using System;

namespace Photoshop.Src.Filters;

/**
 * GaussianDiffFilter - Сглаживающий фильтр по окну 3×3 или 5×5 по выбору пользователя (по Гауссу).
 * 
 */
[Filter("Размытие по Гауссу", "GaussianDiffFilter", "Assets/filter.png")]
[FilterParameter("Размер ядра", Min = 3, Max = 5, Type = ParameterType.Integer)]
public class GaussianDiffFilter : IFilter
{
    private static readonly double[,] Kernel3 = new double[,]
    {
        { 1, 2, 1 },
        { 2, 4, 2 },
        { 1, 2, 1 }
    };
    private const double Kernel3Norm = 16.0;
    
    private static readonly double[,] Kernel5 = new double[,]
    {
        { 1, 2, 3, 2, 1 },
        { 2, 4, 5, 4, 2 },
        { 3, 5, 6, 5, 3 },
        { 2, 4, 5, 4, 2 },
        { 1, 2, 3, 2, 1 }
    };
    private const double Kernel5Norm = 100.0;
    
    public double[] Parameters { get; private set; }
    
    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;
        
        int kernelSize = 3;
        if (parameters != null && parameters.Length > 0)
        {
            kernelSize = (int)parameters[0];
            if (kernelSize != 3 && kernelSize != 5)
                kernelSize = 3;
        }
        
        Picture picture = new Picture(originalPicture.Width, originalPicture.Height);
        for (int y = 0; y < originalPicture.Height; y++)
        {
            for (int x = 0; x < originalPicture.Width; x++)
            {
                ReadOnlySpan<byte> pixel = originalPicture.GetPixel(x, y);
                picture.SetPixel(x, y, pixel[0], pixel[1], pixel[2]);
            }
        }
        ApplyGaussianBlur(originalPicture, picture, 
            kernelSize == 3 ? Kernel3 : Kernel5,
            kernelSize == 3 ? Kernel3Norm : Kernel5Norm, 
            kernelSize);
        
        return picture;
    }

    private void ApplyGaussianBlur(IReadOnlyPicture source, Picture destination, 
                                   double[,] kernel, double norm, int kernelSize)
    {
        int width = source.Width;
        int height = source.Height;
        int radius = (kernelSize - 1) / 2;
        double[,] normalizedKernel = new double[kernelSize, kernelSize];
        for (int i = 0; i < kernelSize; i++)
        {
            for (int j = 0; j < kernelSize; j++)
            {
                normalizedKernel[i, j] = kernel[i, j] / norm;
            }
        }

        for (int y = radius; y < height - radius; y++)
        {
            for (int x = radius; x < width - radius; x++)
            {
                double sumR = 0, sumG = 0, sumB = 0;
                
                for (int ky = -radius; ky <= radius; ky++)
                {
                    for (int kx = -radius; kx <= radius; kx++)
                    {
                        int px = x + kx;
                        int py = y + ky;
                        ReadOnlySpan<byte> pixel = source.GetPixel(px, py);
                        byte red = pixel[0];
                        byte green = pixel[1];
                        byte blue = pixel[2];
                        
                        int kernelX = kx + radius;
                        int kernelY = ky + radius;
                        double weight = normalizedKernel[kernelY, kernelX];
                        
                        sumR += red * weight;
                        sumG += green * weight;
                        sumB += blue * weight;
                    }
                }
                
                destination.SetPixel(x, y,
                    ClampToByte((int)Math.Round(sumR)),
                    ClampToByte((int)Math.Round(sumG)),
                    ClampToByte((int)Math.Round(sumB)));
            }
        }
    }
        
    private byte ClampToByte(int value)
    {
        if (value < 0)
        {
            return 0;
        }

        if (value > 255)
        {
            return 255;
        }
        return (byte)value;
    }
}