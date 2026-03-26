using System;
using System.Collections.Generic;
using System.Linq;

namespace Photoshop.Src;

public static class FilterFinder
{
    public static List<IFilter> FindFilters()
    {
        return [.. AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(IFilter).IsAssignableFrom(t))
                    .Select(f => (IFilter)Activator.CreateInstance(f)!)];
    }

    public static IFilter? FindFilter(List<IFilter> filters, string nameEn)
        => filters.FirstOrDefault(t => t.GetFilterInfo().NameEn == nameEn);
}