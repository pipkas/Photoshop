using System;

namespace Photoshop.Src.Filters;

[Filter("Гамма-коррекция", "GammaFilter", "Assets/GammaFilter.png")]
[FilterParameter("Гамма", Min = 0.1, Max = 10, Type = ParameterType.Double)]
public class GammaFilter : IFilter
{
    public double[] Parameters { get; private set; }

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;
        double gamma = parameters[0];

        int width = originalPicture.Width;
        int height = originalPicture.Height;

        Picture result = new Picture(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ReadOnlySpan<byte> pixel = originalPicture.GetPixel(x, y);
                double r = pixel[0] / 255.0;
                double g = pixel[1] / 255.0;
                double b = pixel[2] / 255.0;
                
                r = Math.Pow(r, gamma);
                g = Math.Pow(g, gamma);
                b = Math.Pow(b, gamma);
                byte R = (byte)(Math.Clamp(r * 255.0, 0, 255));
                byte G = (byte)(Math.Clamp(g * 255.0, 0, 255));
                byte B = (byte)(Math.Clamp(b * 255.0, 0, 255));

                result.SetPixel(x, y, R, G, B);
            }
        }

        return result;
    }
}