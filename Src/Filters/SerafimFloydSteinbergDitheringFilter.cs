using System;

namespace Photoshop.Src.Filters;

[Filter("Дизеринг Флойда-Стейнберга", "SerafimFloydSteinbergDitheringFilter", null)]
[FilterParameter("Красаный", Min = 2, Max = 128, Type = ParameterType.Double)]
[FilterParameter("Зелейный", Min = 2, Max = 128, Type = ParameterType.Double)]
[FilterParameter("Синий", Min = 2, Max = 128, Type = ParameterType.Double)]
public class SerafimFloydSteinbergDitheringFilter : IFilter
{
    public double[] Parameters { get; private set; } = Array.Empty<double>();

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;
        int redlvl = (int)Math.Round(Math.Clamp(parameters[0], 2.0, 128.0));
        int greenlvl = (int)Math.Round(Math.Clamp(parameters[1], 2.0, 128.0));
        int bluelvl = (int)Math.Round(Math.Clamp(parameters[2], 2.0, 128.0));
        Console.WriteLine(redlvl);
        int w = originalPicture.Width;
        int h = originalPicture.Height;
        Picture result = new Picture(w, h);
        double[,] rBuffer = new double[h, w];
        double[,] gBuffer = new double[h, w];
        double[,] bBuffer = new double[h, w];
        
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                ReadOnlySpan<byte> pixel = originalPicture.GetPixel(x, y);
                rBuffer[y, x] = pixel[0];
                gBuffer[y, x] = pixel[1];
                bBuffer[y, x] = pixel[2];
            }
        }
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                double R1 = Math.Clamp(rBuffer[y, x], 0.0, 255.0);
                double G1 = Math.Clamp(gBuffer[y, x], 0.0, 255.0);
                double B1 = Math.Clamp(bBuffer[y, x], 0.0, 255.0);
                byte R2 = GetLvlInAvailbleValue(R1, redlvl);
                byte G2 = GetLvlInAvailbleValue(G1, greenlvl);
                byte B2 = GetLvlInAvailbleValue(B1, bluelvl);
                // Console.Write("R1 =>", R1);
                // Console.Write("R =>", R2);
                result.SetPixel(x, y, R2, G2, B2);
                double errRedCh = R1 - R2;
                double errGreenCh = G1 - G2;
                double errBlueCh = B1 - B2;
                CoutingErrorAndMul(rBuffer, x, y, w, h, errRedCh);
                CoutingErrorAndMul(gBuffer, x, y, w, h, errGreenCh);
                CoutingErrorAndMul(bBuffer, x, y, w, h, errBlueCh);
            }
        }

        return result;
    }

    private static byte GetLvlInAvailbleValue(double value, int lvl)
    {
        value = Math.Clamp(value, 0.0, 255.0);

        if (lvl <= 1)
        {
            return (byte)Math.Round(value);
        }
        double v = 255.0 / (lvl - 1);
        double GetLvlInAvailbleValued = Math.Round(value / v) * v;

        return (byte)Math.Clamp((int)Math.Round(GetLvlInAvailbleValued), 0, 255);
    }

    private static void CoutingErrorAndMul(double[,] buffer, int x, int y, int w, int h, double error)
    {
        if (x + 1 < w)
        {
            buffer[y, x + 1] += error * 7.0 / 16.0;
        }

        if (y + 1 < h)
        {
            if (x - 1 >= 0)
            {
                buffer[y + 1, x - 1] += error * 3.0 / 16.0;
            }
            buffer[y + 1, x] += error * 5.0 / 16.0;
            if (x + 1 < w)
            {
                buffer[y + 1, x + 1] += error * 1.0 / 16.0;
            }
        }
    }
}