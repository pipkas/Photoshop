using System;
using System.IO;
using System.Linq;

namespace Photoshop.Src;

public class ImageViewer
{
    private string[] files;
    private int currentIndex = 0;
    public string DirPath { get; private set; }
    
    public int ImagesCount => files.Length;

    public ImageViewer(string fullPath)
    {
        DirPath = Path.GetDirectoryName(fullPath) ?? throw new ArgumentException("Invalid path", nameof(fullPath));
        files = GetImagePathes();
        currentIndex = Array.IndexOf(files, Path.GetFileName(Path.GetFileName(fullPath)));
    }

    public string FullPath(string fileName) => Path.Combine(DirPath, fileName);

    private string[] GetImagePathes()
    {
        var allFiles = Directory.GetFiles(DirPath);
        var imageFiles = Array.FindAll(allFiles, file =>
            file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
            file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
            file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
            file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFileName).ToArray();
        return imageFiles;
    }

    public Picture? Next()
    {
        if (files.Length <= 1) return null;
        currentIndex = (currentIndex + 1) % files.Length;
        return Picture.LoadFromStream(File.OpenRead(FullPath(files[currentIndex])));
    }

    public Picture? Previous()
    {
        if (files.Length <= 1) return null;
        currentIndex = (currentIndex - 1 + files.Length) % files.Length;
        return Picture.LoadFromStream(File.OpenRead(FullPath(files[currentIndex])));
    }
}