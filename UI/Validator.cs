using System;
using System.Linq;
using System.Reflection;
using Photoshop.Src;

namespace Photoshop.UI;

public static class Validator
{
    public static string? Validate(FilterParameterAttribute attr, double value)
    {
        if (value < attr.Min || value > attr.Max)
            return $"Параметр {attr.Name} вне диапазона [{attr.Min}, {attr.Max}]";

        switch (attr.Type)
        {
            case ParameterType.Integer:
                if (Math.Abs(value - Math.Round(value)) > 1e-9)
                    return $"Параметр {attr.Name} должен быть целым";
                break;

            case ParameterType.OddInteger:
                if (value % 1 != 0 || ((int)value % 2 == 0))
                    return $"Параметр {attr.Name} должен быть нечётным целым";
                break;
        }
        
        return null;
    }
}