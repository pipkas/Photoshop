using System;

namespace Photoshop.Src;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilterParameterAttribute(string name) : Attribute
{
    public string Name { get; } = name;
    public double Min { get; set; } = double.MinValue;
    public double Max { get; set; } = double.MaxValue;
    public ParameterType Type { get; set; } = ParameterType.Integer;
}