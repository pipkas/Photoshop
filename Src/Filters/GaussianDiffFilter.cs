using System;

namespace Photoshop.Src.Filters;

//из картинки делаем две копии: первую слегка размываем (сигма 1), вторую сильнее (сигма 2) 
//затем вычитаем из второй первую и получаем - узкий светлый контур вокруг линии + темный контур на самой линии
//берем абсолютное значение и получаем светлую границу на темном фоне

[Filter("Разница по Гауссу", "GaussianDiffFilter", "Assets/filter.png")]
[FilterParameter("Sigma 1", Min = 0.5, Max = 2.0, Type = ParameterType.Double)]
[FilterParameter("Sigma 2", Min = 1.0, Max = 5.0, Type = ParameterType.Double)]
[FilterParameter("Scale (усиление)", Min = 1, Max = 10, Type = ParameterType.Double)]
public class GaussianDiffFilter : IFilter
{
    public double[] Parameters {get; private set;}

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;

        double sigma1 = parameters.Length > 0 ? parameters[0] : 1.0;
        double sigma2 = parameters.Length > 1 ? parameters[1] : 2.0;
        double scale = parameters.Length > 2 ? parameters[2] : 1.0;

        if (sigma1 > sigma2)
            (sigma1, sigma2) = (sigma2, sigma1);

        var blur1 = ApplyGaussianBlur(originalPicture, sigma1);
        var blur2 = ApplyGaussianBlur(originalPicture, sigma2);

        var result = new Picture(originalPicture.Width, originalPicture.Height);

        for (int y = 0; y < originalPicture.Height; y++)
        {
            for (int x = 0; x < originalPicture.Width; x++)
            {
                var pixel1 = blur1.GetPixel(x, y);
                var pixel2 = blur2.GetPixel(x, y);

                int diffR = Math.Abs(pixel1[0] - pixel2[0]);
                int diffG = Math.Abs(pixel1[1] - pixel2[1]);
                int diffB = Math.Abs(pixel1[2] - pixel2[2]);
                int gray = (int)((0.299 * diffR + 0.587 * diffG + 0.114 * diffB) * scale);
                gray = Math.Clamp(gray, 0, 255);

                byte a = pixel1[3]; 
                result.SetPixel(x, y, (byte)gray, (byte)gray, (byte)gray, a);
            }
        }
        return result;
    }

    private Picture ApplyGaussianBlur(IReadOnlyPicture originalPicture, double sigma)
    {
        int radius = Math.Max(1, (int)Math.Ceiling(3 * sigma));       
        double[] kernel = BuildGaussianKernel(radius, sigma);
        var temp = new Picture(originalPicture.Width, originalPicture.Height);  //временное изображение для результата горизонтального размытия
        
        //горизонтальное размытие
        for (int y = 0; y < originalPicture.Height; y++)
        {
            for (int x = 0; x < originalPicture.Width; x++)
            {
                double sumR = 0, sumG = 0, sumB = 0, sumA = 0;
                
                for (int kx = -radius; kx <= radius; kx++)
                {
                    int sampleX = Math.Clamp(x + kx, 0, originalPicture.Width - 1);
                    var pixel = originalPicture.GetPixel(sampleX, y);
                    double weight = kernel[kx + radius];
                    
                    sumR += pixel[0] * weight;
                    sumG += pixel[1] * weight;
                    sumB += pixel[2] * weight;
                    sumA += pixel[3] * weight;
                }
                
                temp.SetPixel(x, y,
                    (byte)Math.Clamp((int)Math.Round(sumR), 0, 255),
                    (byte)Math.Clamp((int)Math.Round(sumG), 0, 255),
                    (byte)Math.Clamp((int)Math.Round(sumB), 0, 255),
                    (byte)Math.Clamp((int)Math.Round(sumA), 0, 255));
            }
        }
        
        //вертикальное размытие
        var result = new Picture(originalPicture.Width, originalPicture.Height);
        
        for (int y = 0; y < originalPicture.Height; y++)
        {
            for (int x = 0; x < originalPicture.Width; x++)
            {
                double sumR = 0, sumG = 0, sumB = 0, sumA = 0;
                
                for (int ky = -radius; ky <= radius; ky++)
                {
                    int sampleY = Math.Clamp(y + ky, 0, originalPicture.Height - 1);
                    var pixel = temp.GetPixel(x, sampleY);
                    double weight = kernel[ky + radius];
                    
                    sumR += pixel[0] * weight;
                    sumG += pixel[1] * weight;
                    sumB += pixel[2] * weight;
                    sumA += pixel[3] * weight;
                }
                
                result.SetPixel(x, y,
                    (byte)Math.Clamp((int)Math.Round(sumR), 0, 255),
                    (byte)Math.Clamp((int)Math.Round(sumG), 0, 255),
                    (byte)Math.Clamp((int)Math.Round(sumB), 0, 255),
                    (byte)Math.Clamp((int)Math.Round(sumA), 0, 255));
            }
        }        
        return result;
    }

    private double[] BuildGaussianKernel(int radius, double sigma)
    {
        int size = 2 * radius + 1;
        double[] kernel = new double[size];
        double sigma2 = sigma * sigma;
        double sum = 0.0;
        
        for (int x = -radius; x <= radius; x++)
        {
            double value = Math.Exp(-(x * x) / (2 * sigma2));
            kernel[x + radius] = value;
            sum += value;
        }
                
        for (int i = 0; i < size; i++)
        {
            kernel[i] /= sum;  //нормализация
        }        
        return kernel;
    }
}