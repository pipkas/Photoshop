using System;

namespace Photoshop.Src.Filters;

[Filter("Фильтр выделения границ Робертcа", "Robert's filter", "Assets/Rob.png")]
[FilterParameter("Порог бинаризации", Min = 0, Max = 510, Type = ParameterType.Integer)]
public class BorderSelectionFilterRoberts : IFilter
{
    public double[] Parameters { get; private set; }

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;
        int threshold = (int)parameters[0];
        
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
        
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                ReadOnlySpan<byte> p1 = originalPicture.GetPixel(x, y);
                ReadOnlySpan<byte> p2 = originalPicture.GetPixel(x + 1, y + 1);
                ReadOnlySpan<byte> p3 = originalPicture.GetPixel(x, y + 1);
                ReadOnlySpan<byte> p4 = originalPicture.GetPixel(x + 1, y);
                
                double lum1 = p1[0] * 0.299 + p1[1] * 0.587 + p1[2] * 0.114;
                double lum2 = p2[0] * 0.299 + p2[1] * 0.587 + p2[2] * 0.114;
                double lum3 = p3[0] * 0.299 + p3[1] * 0.587 + p3[2] * 0.114;
                double lum4 = p4[0] * 0.299 + p4[1] * 0.587 + p4[2] * 0.114;
                
                double gradient = Math.Abs(lum1 - lum2) + Math.Abs(lum3 - lum4);
                
                if (gradient > threshold)
                {
                    result.SetPixel(x, y, 255, 255, 255);
                }
                else
                {
                    result.SetPixel(x, y, 0, 0, 0);
                }
            }
        }
        
        return result;
    }
}