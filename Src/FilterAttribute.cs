using System;

namespace Photoshop.Src;

[AttributeUsage(AttributeTargets.Class)]
public class FilterAttribute(string nameRu, string nameEn, string imagePath) : Attribute
{
    public string NameRu { get; } = nameRu;
    public string NameEn { get; } = nameEn;
    public string? ImagePath { get; } = imagePath;
}