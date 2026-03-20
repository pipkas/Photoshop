using System;

namespace Photoshop.Src.Filters;

//тестовый образец
[Filter("Какой-то фильтр", "SomeFilter", "Assets/filter.png")]
[FilterParameter("Ширина", Min = 1, Max = 100, Type = ParameterType.Double)]
[FilterParameter("Высота", Min = 1, Max = 100, Type = ParameterType.Integer)]
public class GaussianDiffFilter : IFilter
{
    public double[] Parameters {get; private set;}

    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters)
    {
        Parameters = parameters;
        // Console.WriteLine(Parameters[0]);
        // Console.WriteLine(Parameters[1]);
        return new Picture();
    }
}
