using System;
using System.ComponentModel;
using System.Linq;

namespace Photoshop.Src;

public class PhotoshopManager: INotifyPropertyChanged
{
    private Picture curImage;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Picture CurImage
    {
        get => curImage;
        set
        {
            if (curImage == value) return;
            curImage = value;
            OnPropertyChanged(nameof(CurImage));
        }
    }

    public IFilter? CurFilter {get; set;}
    public Picture OriginalImage {get; set;}
    public Picture ModifiedImage {get; set;}

    public PhotoshopManager()
    {
        OriginalImage = new Picture();
        CurImage = OriginalImage;
        CurFilter = null;
    }

    public void UseFilter(string filterName, params double[] parameters)
    {
        var filterType = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .FirstOrDefault(t =>
                                        typeof(IFilter).IsAssignableFrom(t) &&
                                        !t.IsInterface && !t.IsAbstract && t.Name == filterName);

        // if (filterType == null)
        //     throw new InvalidOperationException($"Filter '{filterName}' not found");

        if (filterType == null)
        {
            Console.WriteLine($"Filter '{filterName}' not found");
            return;
        }
            

        var filter = (IFilter)Activator.CreateInstance(filterType)!;

        var result = filter.Modify(OriginalImage, parameters);

        ModifiedImage = result;
        CurImage = result;
        CurFilter = filter;
    }

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}