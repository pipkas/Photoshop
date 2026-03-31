using System;
using System.IO;
using SkiaSharp; // если нужно, но мы работаем с твоим Picture

namespace Photoshop.Src.Filters;

[Filter("Карандашный рисунок", "Pencil filter", "Assets/pencil.png")]
public class PencilFilter : IFilter
{
    public double[] Parameters => Array.Empty<double>();

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        int width = originalPicture.Width;
        int height = originalPicture.Height;
        var result = new Picture(width, height);

        // 1. Grayscale
        byte[,] gray = new byte[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var px = originalPicture.GetPixel(x, y);
                byte r = px[0], g = px[1], b = px[2];
                gray[x, y] = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
            }
        }

        // 2. Лёгкое размытие grayscale (убираем мелкий шум)
        byte[,] blurredGray = GaussianBlur(gray, width, height, radius: 1);

        // 3. Инвертируем + сильное размытие
        byte[,] inverted = new byte[width, height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                inverted[x, y] = (byte)(255 - blurredGray[x, y]);

        byte[,] blurredInverted = GaussianBlur(inverted, width, height, radius: 15); // 10–20 — основной параметр эффекта

        // 4. Color Dodge (основной карандашный эффект)
        byte[,] sketch = new byte[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte c = blurredGray[x, y]; 
                byte b = blurredInverted[x, y];

                byte val = (b == 255) ? (byte)255 : (byte)Math.Min(255, (c * 255) / (255 - b + 1)); // +1 чтобы избежать деления на 0
                sketch[x, y] = val;
            }
        }

        // 5. Добавляем чёткие карандашные линии (Laplacian / простой edge)
        byte[,] edges = LaplacianEdge(sketch, width, height);   // или SobelEdge

        // Комбинируем: чем темнее линия — тем сильнее она проявляется
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int finalVal = sketch[x, y] - edges[x, y];           // вычитаем линии (делают темнее)
                finalVal = Math.Clamp(finalVal, 0, 255);

                // Усиливаем контраст (опционально, но сильно улучшает вид)
                finalVal = (int)(finalVal * 1.15 - 20);             // можно подкрутить
                finalVal = Math.Clamp(finalVal, 0, 255);

                result.SetPixel(x, y, (byte)finalVal, (byte)finalVal, (byte)finalVal, 255);
            }
        }

        return result;
    }

    private byte[,] GaussianBlur(byte[,] input, int width, int height, int radius)
    {
        if (radius < 1) radius = 1;
        byte[,] temp = new byte[width, height];
        byte[,] output = new byte[width, height];

        int kernelSize = 2 * radius + 1;
        double sigma = radius / 2.0;
        double[] kernel = new double[kernelSize];
        double sum = 0;

        for (int i = -radius; i <= radius; i++)
        {
            kernel[i + radius] = Math.Exp(-(i * i) / (2 * sigma * sigma));
            sum += kernel[i + radius];
        }
        for (int i = 0; i < kernelSize; i++)
            kernel[i] /= sum;

        // Horizontal
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                double val = 0;
                for (int k = -radius; k <= radius; k++)
                {
                    int nx = Math.Clamp(x + k, 0, width - 1);
                    val += input[nx, y] * kernel[k + radius];
                }
                temp[x, y] = (byte)Math.Clamp((int)val, 0, 255);
            }

        // Vertical
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                double val = 0;
                for (int k = -radius; k <= radius; k++)
                {
                    int ny = Math.Clamp(y + k, 0, height - 1);
                    val += temp[x, ny] * kernel[k + radius];
                }
                output[x, y] = (byte)Math.Clamp((int)val, 0, 255);
            }

        return output;
    }

    private byte[,] LaplacianEdge(byte[,] input, int width, int height)
    {
        byte[,] output = new byte[width, height];
        int[,] laplacianKernel = {
            { 0,  1,  0 },
            { 1, -4,  1 },
            { 0,  1,  0 }
        };

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int sum = 0;
                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        int nx = Math.Clamp(x + kx, 0, width - 1);
                        int ny = Math.Clamp(y + ky, 0, height - 1);
                        sum += input[nx, ny] * laplacianKernel[ky + 1, kx + 1];
                    }
                }
                int edge = Math.Abs(sum);
                output[x, y] = (byte)Math.Clamp(edge / 2, 0, 255);
            }
        }
        return output;
    }

}