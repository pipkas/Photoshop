using System;

namespace Photoshop.Src.Filters;

[Filter("Увеличение резкости", "Sharpen filter", "Assets/Sharpen.png")]
public class SharpenFilter : IFilter
{
    private static readonly double[,] Kernel = new double[,]
    {
        { 0, -1, 0 },
        { -1, 5, -1 },
        { 0, -1, 0 }
    };
    
    public double[] Parameters { get; private set; }

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;
        
        int width = originalPicture.Width;
        int height = originalPicture.Height;
        
        Picture result = new Picture(width, height);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ReadOnlySpan<byte> pixel = originalPicture.GetPixel(x, y);
                result.SetPixel(x, y, pixel[0], pixel[1], pixel[2]);
            }
        }
        
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                double sumR = 0, sumG = 0, sumB = 0;
                
                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        ReadOnlySpan<byte> pixel = originalPicture.GetPixel(x + kx, y + ky);
                        double weight = Kernel[ky + 1, kx + 1];
                        
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