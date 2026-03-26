using System;
using System.Collections.Generic;
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
    public List<IFilter> Filters {get;}

    public PhotoshopManager()
    {
        OriginalImage = new Picture();
        CurImage = OriginalImage;
        CurFilter = null;
        Filters = FilterFinder.FindFilters();
    }

    public void UseFilter(IFilter filter, double[]? parameters)
    {
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