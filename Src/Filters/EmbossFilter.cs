using System;

namespace Photoshop.Src.Filters;

[Filter("Тиснение", "EmbossFilter", "Assets/emboss.png")]
[FilterParameter("0 - серый, 1 - цветной", Min = 0, Max = 1, Type = ParameterType.Integer)]
public class EmbossFilter  : IFilter
{
    public double[] Parameters {get; private set;} = Array.Empty<double>();
    private static readonly int[,] kernel =
    {
        {  0,  1,  0 },
        { -1,  0,  1 },
        {  0, -1,  0 }
    };

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;
        var result = new Picture(originalPicture.Width, originalPicture.Height);

        int mode = parameters.Length > 0 ? (int)Math.Round(parameters[0]) : 0;

        for (int y = 0; y < originalPicture.Height; y++)
        {
            for (int x = 0; x < originalPicture.Width; x++)
            {
                byte a = originalPicture.GetPixel(x, y)[3];

                if (mode == 0)
                {
                    int sum = 0;
                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            int neighborX = x + kx;
                            int neighborY = y + ky;
                            int srcX = Math.Clamp(neighborX, 0, originalPicture.Width - 1);
                            int srcY = Math.Clamp(neighborY, 0, originalPicture.Height - 1);    
                            var pixel = originalPicture.GetPixel(srcX, srcY);
                            int gray = (int)(0.299 * pixel[0] + 0.587 * pixel[1] + 0.114 * pixel[2]);
                            sum += gray * kernel[ky + 1, kx + 1];
                        }
                    }
                    byte value = (byte)Math.Clamp(sum + 128, 0, 255);
                    result.SetPixel(x, y, value, value, value, a);
                }
                else
                {
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
                    byte newR = (byte)Math.Clamp(sumR + 128, 0, 255);
                    byte newG = (byte)Math.Clamp(sumG + 128, 0, 255);
                    byte newB = (byte)Math.Clamp(sumB + 128, 0, 255);   
                    result.SetPixel(x, y, newR, newG, newB, a);
                }                       
            }
        }
        return result;
    }
}