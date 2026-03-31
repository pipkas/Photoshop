using System;

namespace Photoshop.Src.Filters;

//цель: уменьшить число цветов, но сделать так, чтобы мы видели картинку "плавной"
[Filter("Floyd-Steinberg дизеринг", "Sonya's Floyd-Steinberg dithering filter", null)]
[FilterParameter("R", Min = 2, Max = 128, Type = ParameterType.Integer)]
[FilterParameter("G", Min = 2, Max = 128, Type = ParameterType.Integer)]
[FilterParameter("B", Min = 2, Max = 128, Type = ParameterType.Integer)]
public class SonyaFSDitheringFilter : IFilter
{
    public double[] Parameters {get; private set;} = Array.Empty<double>();

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;
        int redLevels = parameters.Length > 0 ? (int)Math.Round(parameters[0]) : 2;
        int greenLevels = parameters.Length > 1 ? (int)Math.Round(parameters[1]) : 2;
        int blueLevels = parameters.Length > 2 ? (int)Math.Round(parameters[2]) : 2;

        int width = originalPicture.Width;
        int height = originalPicture.Height;
        var result = new Picture(width, height);

        double[,] red = new double[height, width];
        double[,] green = new double[height, width];
        double[,] blue = new double[height, width];
        byte[,] a = new byte[height, width];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pixel = originalPicture.GetPixel(x, y);
                red[y, x] = pixel[0];
                green[y, x] = pixel[1];
                blue[y, x] = pixel[2];
                a[y, x] = pixel[3]; 
            }
        }
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double origR = red[y, x];
                double origG = green[y, x];
                double origB = blue[y, x];  

                byte newR = Quantize(origR, redLevels);
                byte newG = Quantize(origG, greenLevels);
                byte newB = Quantize(origB, blueLevels);
                result.SetPixel(x, y, newR, newG, newB, a[y, x]);

                double errR = origR - newR;
                double errG = origG - newG;     
                double errB = origB - newB;

                AddError(red, width, height, x + 1, y, errR * 7.0 / 16.0);
                AddError(red, width, height, x - 1, y + 1, errR * 3.0 / 16.0);
                AddError(red, width, height, x, y + 1, errR * 5.0 / 16.0);
                AddError(red, width, height, x + 1, y + 1,  errR * 1.0 / 16.0);

                AddError(green, width, height, x + 1, y, errG * 7.0 / 16.0);
                AddError(green, width, height, x - 1, y + 1, errG * 3.0 / 16.0);
                AddError(green, width, height, x, y + 1, errG * 5.0 / 16.0);
                AddError(green, width, height, x + 1, y + 1,  errG * 1.0 / 16.0);

                AddError(blue, width, height, x + 1, y, errB * 7.0 / 16.0);
                AddError(blue, width, height, x - 1, y + 1, errB * 3.0 / 16.0);
                AddError(blue, width, height, x, y + 1, errB * 5.0 / 16.0);
                AddError(blue, width, height, x + 1, y + 1,  errB * 1.0 / 16.0);
            }
        }
        return result;
    }

    private static byte Quantize(double value, int levels)
    {
        double step = 255.0 / (levels - 1);
        double quantized = Math.Round(value / step) * step;
        int result = (int)Math.Round(quantized);

        return (byte)Math.Clamp(result, 0, 255);
    }

    //распределяет ошибку квантования от текущего пикселя к соседним пикселям
    private static void AddError(double[,] channel, int width, int height, int x, int y, double errorPart)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        channel[y, x] = Math.Clamp(channel[y, x] + errorPart, 0.0, 255.0);
    }
}