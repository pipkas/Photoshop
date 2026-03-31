using System;

namespace Photoshop.Src.Filters;

[Filter("Черно-белый фильтр", "BW filter", "Assets/bw.png")]
public class BWFilter : IFilter
{
    public double[] Parameters {get; private set;} = Array.Empty<double>();

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;
        var result = new Picture(originalPicture.Width, originalPicture.Height);
        
        for (int y = 0; y < originalPicture.Height; y++)
        {
            for (int x = 0; x < originalPicture.Width; x++)
            {
                var pixel = originalPicture.GetPixel(x, y);
                var r = pixel[0];
                var g = pixel[1];
                var b = pixel[2];
                var a = pixel[3];
                
                byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                result.SetPixel(x, y, gray, gray, gray, a);
            }
        }
        return result;
    }
}