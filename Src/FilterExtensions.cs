using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Photoshop.Src;

public static class FilterExtensions
{
    public static FilterParameterAttribute[] GetFilterParams(this IFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return [.. filter.GetType().GetCustomAttributes<FilterParameterAttribute>(inherit: true)];
    }
    
    public static FilterAttribute GetFilterInfo(this IFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return filter.GetType().GetCustomAttribute<FilterAttribute>(inherit: true)!;
    }

    public static bool Equals2(this IFilter filter1, IFilter filter2)
    => filter1.GetFilterInfo().NameEn.Equals(filter2.GetFilterInfo().NameEn);
    
}