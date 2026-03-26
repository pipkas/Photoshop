using System;
using System.Collections.Generic;

namespace Photoshop.Src.Filters;

[Filter("Акварелизация", "WatercolorFilter", "Assets/filter.png")]
[FilterParameter("Размер ядра (нечет)", Min = 3, Max = 7, Type = ParameterType.OddInteger)]
public class WatercolorFilter : IFilter
{
    public double[] Parameters {get; private set;} = Array.Empty<double>();

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;

        int medianWindowSize = 5;
        if (Parameters.Length > 0) medianWindowSize = (int)Math.Round(Parameters[0]);

        var medianResult = ApplyMedianFilter(originalPicture, medianWindowSize);

        var sharpenedResult = ApplySharpenFilter(medianResult);
        
        return sharpenedResult;
    }

    private Picture ApplyMedianFilter(IReadOnlyPicture originalPicture, int windowSize)
    {
        var result = new Picture(originalPicture.Width, originalPicture.Height);        

        List<byte> reds = new List<byte>(windowSize * windowSize);
        List<byte> greens = new List<byte>(windowSize * windowSize);
        List<byte> blues = new List<byte>(windowSize * windowSize);
        int radius = windowSize / 2;
        
        for (int y = 0; y < originalPicture.Height; y++)
        {
            for (int x = 0; x < originalPicture.Width; x++)
            {
                reds.Clear();
                greens.Clear();
                blues.Clear();                
                for (int ky = -radius; ky <= radius; ky++)
                {
                    for (int kx = -radius; kx <= radius; kx++)
                    {
                        int sampleX = Math.Clamp(x + kx, 0, originalPicture.Width - 1);
                        int sampleY = Math.Clamp(y + ky, 0, originalPicture.Height - 1);
                        var pixel = originalPicture.GetPixel(sampleX, sampleY);

                        reds.Add(pixel[0]);
                        greens.Add(pixel[1]);
                        blues.Add(pixel[2]);
                    }
                }

                reds.Sort();    
                greens.Sort();
                blues.Sort();    
                int medianIndex = reds.Count / 2;
                byte medianR = reds[medianIndex];
                byte medianG = greens[medianIndex];
                byte medianB = blues[medianIndex];
                byte a = originalPicture.GetPixel(x, y)[3];
                result.SetPixel(x, y, medianR, medianG, medianB, a);
            }
        }
        return result;  
    }
    private Picture ApplySharpenFilter(IReadOnlyPicture originalPicture)
    {
        var result = new Picture(originalPicture.Width, originalPicture.Height);   

        int[,] kernel =
        {
            {  0, -1,  0 },
            { -1,  5, -1 },
            {  0, -1,  0 }
        };

        for (int y = 0; y < originalPicture.Height; y++)
        {
            for (int x = 0; x < originalPicture.Width; x++)
            {
                byte a = originalPicture.GetPixel(x, y)[3];
                int sumR = 0;
                int sumG = 0;
                int sumB = 0;
                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        int srcX = Math.Clamp(x + kx, 0, originalPicture.Width - 1);
                        int srcY = Math.Clamp(y + ky, 0, originalPicture.Height - 1);
                        var pixel = originalPicture.GetPixel(srcX, srcY); 
                        int k = kernel[ky + 1, kx + 1];      
                        sumR += pixel[0] * k;
                        sumG += pixel[1] * k;
                        sumB += pixel[2] * k;
                    }
                }
                byte newR = (byte)Math.Clamp(sumR, 0, 255);
                byte newG = (byte)Math.Clamp(sumG, 0, 255);
                byte newB = (byte)Math.Clamp(sumB, 0, 255);   
                result.SetPixel(x, y, newR, newG, newB, a);
            }
        }
        return result ;
    }
}