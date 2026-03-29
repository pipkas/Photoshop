using System;

namespace Photoshop.Src.Filters;

[Filter("Упорядоченный дизеринг", "PolinaOrderedDitherFilter", null)]
[FilterParameter("Уровень квантования (R)", Min = 2, Max = 128, Type = ParameterType.Integer)]
[FilterParameter("Уровень квантования (G)", Min = 2, Max = 128, Type = ParameterType.Integer)]
[FilterParameter("Уровень квантования (B)", Min = 2, Max = 128, Type = ParameterType.Integer)]
public class PolinaOrderedDitherFilter : IFilter
{
    public double[] Parameters { get; private set; } = Array.Empty<double>();
    private const int MaxMatrixSize = 8;

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;

        int redLevels = parameters.Length > 0 ? (int)Math.Round(parameters[0]) : 2;
        int greenLevels = parameters.Length > 1 ? (int)Math.Round(parameters[1]) : 2;
        int blueLevels = parameters.Length > 2 ? (int)Math.Round(parameters[2]) : 2;

        int redMatrixSize = GetMatrixSize(redLevels);
        int greenMatrixSize = GetMatrixSize(greenLevels);
        int blueMatrixSize = GetMatrixSize(blueLevels);

        int[,] redMatrix = BuildMatrix(redMatrixSize);
        int[,] greenMatrix = BuildMatrix(greenMatrixSize);
        int[,] blueMatrix = BuildMatrix(blueMatrixSize);

        double redMatrixArea = redMatrixSize * redMatrixSize;
        double greenMatrixArea = greenMatrixSize * greenMatrixSize;
        double blueMatrixArea = blueMatrixSize * blueMatrixSize;

        int width = originalPicture.Width;
        int height = originalPicture.Height;
        var result = new Picture(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pixel = originalPicture.GetPixel(x, y);

                int redThresholdIndex = redMatrix[y % redMatrixSize, x % redMatrixSize];
                int greenThresholdIndex = greenMatrix[y % greenMatrixSize, x % greenMatrixSize];
                int blueThresholdIndex = blueMatrix[y % blueMatrixSize, x % blueMatrixSize];

                byte newR = DitherChannel(pixel[0], redLevels, redThresholdIndex, redMatrixArea);
                byte newG = DitherChannel(pixel[1], greenLevels, greenThresholdIndex, greenMatrixArea);
                byte newB = DitherChannel(pixel[2], blueLevels, blueThresholdIndex, blueMatrixArea);
                byte a = pixel[3];

                result.SetPixel(x, y, newR, newG, newB, a);
            }
        }
        return result;
    }

    private static int GetMatrixSize(int levels)
    {
        double requiredThresholds = 256.0 / levels;  //минимум два цвета
        int size = 2;
        while (size * size < requiredThresholds && size < MaxMatrixSize)
        {
            size *= 2;
        }
        return size;
    }   

    private static int[,] BuildMatrix(int size)
    {
        if (size == 1)
            return new int[,] { { 0 } };

        int half = size / 2;
        int[,] subMatrix = BuildMatrix(half);
        int[,] result = new int[size, size];

        for (int y = 0; y < half; y++)
        {
            for (int x = 0; x < half; x++)
            {
                int value = 4 * subMatrix[y, x];
                result[y, x] = value;
                result[y, x + half] = value + 2;
                result[y + half, x] = value + 3;
                result[y + half, x + half] = value + 1;
            }
        }
        return result;
    }

    private static byte DitherChannel(byte value, int levels, int thresholdIndex, double matrixArea)
    {
        double threshold = (thresholdIndex + 0.5) / matrixArea;  //нормализуем 
        double offset = threshold - 0.5;

        double step = 255.0 / (levels - 1);  //расстояние между уровнями квантования

        double shifted = value + offset * step;  //смещаем исходное значение пискеля в зависимоти от позиции пикселя в матрице
        shifted = Math.Clamp(shifted, 0.0, 255.0);

        int levelIndex = (int)Math.Round(shifted / step);  //к какому уровню ближе пиксель
        levelIndex = Math.Clamp(levelIndex, 0, levels - 1);

        int quantized = (int)Math.Round(levelIndex * step);
        quantized = Math.Clamp(quantized, 0, 255);

        return (byte)quantized;
    }
}