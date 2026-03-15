using System;

namespace Photoshop.Src.Filters;

//тестовый образец
public class GaussianDiffFilter : IFilter
{
    public double[] Parameters {get; private set;}

    public Picture Modify(IReadOnlyPicture originalPicture, params double[] parameters)
    {
        Parameters = parameters;
        Console.WriteLine(Parameters[0]);
        Console.WriteLine(Parameters[1]);
        return new Picture();
    }
}