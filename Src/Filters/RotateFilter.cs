using System;

namespace Photoshop.Src.Filters;

[Filter("Поворот", "Rotate filter", "Assets/rotate.png")]
[FilterParameter("Угол поворота", Min = -180, Max = 180, Type = ParameterType.Double)]
public class RotateFilter : IFilter
{
    public double[] Parameters {get; private set;} = Array.Empty<double>();

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;

        double angleDeg = parameters.Length > 0 ? parameters[0] : 0.0;
        double angleRad = angleDeg * Math.PI / 180.0;
        double cosA = Math.Cos(angleRad);
        double sinA = Math.Sin(angleRad);

        int oldWidth = originalPicture.Width;
        int oldHeight = originalPicture.Height;
        double oldCenterX = (oldWidth - 1) / 2.0;  //центр исходной картинки
        double oldCenterY = (oldHeight - 1) / 2.0;

        var corners = new (double x, double y)[]
        {
            RotatePoint(-oldCenterX, -oldCenterY, cosA, sinA),
            RotatePoint(oldWidth - 1 - oldCenterX, -oldCenterY, cosA, sinA),
            RotatePoint(-oldCenterX, oldHeight - 1 - oldCenterY, cosA, sinA),
            RotatePoint(oldWidth - 1 - oldCenterX, oldHeight - 1 - oldCenterY, cosA, sinA)
        };

        double minX = corners[0].x;
        double maxX = corners[0].x;
        double minY = corners[0].y;
        double maxY = corners[0].y;

        foreach (var corner in corners)
        {
            if (corner.x < minX) minX = corner.x;
            if (corner.x > maxX) maxX = corner.x;
            if (corner.y < minY) minY = corner.y;
            if (corner.y > maxY) maxY = corner.y;
        }

        int newWidth = (int)Math.Ceiling(maxX - minX + 1);
        int newHeight = (int)Math.Ceiling(maxY - minY + 1);

        var result = new Picture(newWidth, newHeight);

        double newCenterX = (newWidth - 1) / 2.0;
        double newCenterY = (newHeight - 1) / 2.0;

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                double dx = x - newCenterX;  //вектор от центра до точки x, т.е. где находится точка относительно центра
                double dy = y - newCenterY;

                double oldX = oldCenterX + dx * cosA - dy * sinA;
                double oldY = oldCenterY + dx * sinA + dy * cosA;

                //метод ближайшего соседа
                int nearestX = (int)Math.Round(oldX);
                int nearestY = (int)Math.Round(oldY);
                if (nearestX >= 0 && nearestX < oldWidth && nearestY >= 0 && nearestY < oldHeight)
                {
                    var pixel = originalPicture.GetPixel(nearestX, nearestY);
                    result.SetPixel(x, y, pixel[0], pixel[1], pixel[2], pixel[3]);
                }
                else
                {
                    result.SetPixel(x, y, 255, 255, 255, 255);
                }
            }
        }          
        return result;
    }

    private static (double x, double y) RotatePoint(double x, double y, double cosA, double sinA)
    {
        return (
            x * cosA - y * sinA,
            x * sinA + y * cosA
        );
    }
}