using System;

namespace Photoshop.Src.Filters;

//меняем каждый канал на противоположный
[Filter("Инверсия", "InvertFilter", "Assets/invert.png")]
public class InvertFilter  : IFilter
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
                
                byte newR = (byte)(255 - r);
                byte newG = (byte)(255 - g);
                byte newB = (byte)(255 - b);
                
                result.SetPixel(x, y, newR, newG, newB, a);
            }
        }
        return result;
    }
}