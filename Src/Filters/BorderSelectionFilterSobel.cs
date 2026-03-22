using System;

namespace Photoshop.Src.Filters;

[Filter("Фильтр выделения границ", "BorderSelectionFilter", "Assets/BorderSelectionFilter.png")]
[FilterParameter("Порог бинаризации", Min = 0, Max = 255, Type = ParameterType.Integer)]
public class BorderSelectionFilterSobel : IFilter
{
    private static readonly double[,] KernelVert = new double[,]
    {
        { -1, 0, 1 },
        { -2, 0, 2 },
        { -1, 0, 1 }
    };
    
    private static readonly double[,] KernelHoriz = new double[,]
    {
        { -1, -2, -1},
        { 0, 0, 0 },
        { 1, 2, 1 }
    };
    
    public double[] Parameters { get; private set; }

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;
        int threshold = (int)parameters[0];
        
        Picture picture = new Picture(originalPicture.Width, originalPicture.Height);
        
       
        double[,] luminanceMap = new double[originalPicture.Width, originalPicture.Height];
        for (int y = 0; y < originalPicture.Height; y++)
        {
            for (int x = 0; x < originalPicture.Width; x++)
            {
                ReadOnlySpan<byte> pixel = originalPicture.GetPixel(x, y);
                picture.SetPixel(x, y, pixel[0], pixel[1], pixel[2]);
                luminanceMap[x, y] = pixel[0] * 0.299 + pixel[1] * 0.587 + pixel[2] * 0.114;
            }
        }
        
        ApplySobelFast(luminanceMap, picture, 3, threshold);
        return picture;
    }

    private void ApplySobelFast(double[,] luminance, Picture picture, int kernelSize, int threshold)
    {
        int width = luminance.GetLength(0);
        int height = luminance.GetLength(1);
        int border = (kernelSize - 1) / 2;

        var horiz = KernelHoriz;
        var vert = KernelVert;
        
        for (int y = border; y < height - border; y++)
        {
            for (int x = border; x < width - border; x++)
            {
                double gx = 0, gy = 0;
                
                gx += horiz[0, 0] * luminance[x - 1, y - 1];
                gx += horiz[0, 1] * luminance[x, y - 1];
                gx += horiz[0, 2] * luminance[x + 1, y - 1];
                gx += horiz[1, 0] * luminance[x - 1, y];
                gx += horiz[1, 1] * luminance[x, y];
                gx += horiz[1, 2] * luminance[x + 1, y];
                gx += horiz[2, 0] * luminance[x - 1, y + 1];
                gx += horiz[2, 1] * luminance[x, y + 1];
                gx += horiz[2, 2] * luminance[x + 1, y + 1];
                gy += vert[0, 0] * luminance[x - 1, y - 1];
                gy += vert[0, 1] * luminance[x, y - 1];
                gy += vert[0, 2] * luminance[x + 1, y - 1];
                gy += vert[1, 0] * luminance[x - 1, y];
                gy += vert[1, 1] * luminance[x, y];
                gy += vert[1, 2] * luminance[x + 1, y];
                gy += vert[2, 0] * luminance[x - 1, y + 1];
                gy += vert[2, 1] * luminance[x, y + 1];
                gy += vert[2, 2] * luminance[x + 1, y + 1];
                
                double gradient = Math.Sqrt(gx * gx + gy * gy);
                
                if (gradient > threshold)
                {
                    picture.SetPixel(x, y, 255, 255, 255);
                }
                else
                {
                    picture.SetPixel(x, y, 0, 0, 0);
                }
            }
        }
    }
}