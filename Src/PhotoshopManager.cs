using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

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
    public InterpolationType InterpolType {get; set;}
    public Picture OriginalImage {get; set;}
    public Picture? ModifiedImage {get; set;}
    public Picture? FitOrigImage {get; set;}
    public Picture? FitModImage {get; set;}
    public List<IFilter> Filters {get;}

    public PhotoshopManager()
    {
        OriginalImage = new Picture();
        CurImage = OriginalImage;
        CurFilter = null;
        InterpolType = InterpolationType.None;
        Filters = FilterFinder.FindFilters();
    }

    public void UseFilter(IFilter filter, double[]? parameters)
    {
        var result = filter.Modify(OriginalImage, parameters);

        ModifiedImage = result;
        CurFilter = filter;
        if (InterpolType != InterpolationType.None)
        {
            FitModImage = FitToScreenManager.FitToScreen(InterpolType, 
                ModifiedImage, new Size(CurImage.Width, CurImage.Height));

            CurImage = FitModImage;
        }
        else
            CurImage = ModifiedImage;
    }

    public void FitToScreen(InterpolationType type, Size newImageSize)
    {
        if (type == InterpolationType.None || CurImage.Width <= newImageSize.Width && CurImage.Height <= newImageSize.Height)
            return;

        InterpolType = type;
        var prevImage = CurImage == OriginalImage || CurImage == FitOrigImage 
                        ? OriginalImage 
                        : ModifiedImage;

        var fitImage = FitToScreenManager.FitToScreen(type, OriginalImage, newImageSize);
        FitOrigImage = fitImage;
        if (ModifiedImage != null)
        {
            fitImage = FitToScreenManager.FitToScreen(type, ModifiedImage, newImageSize);
            FitModImage = fitImage;
        }
        CurImage = prevImage == OriginalImage ? FitOrigImage : FitModImage;
    }

    public void MakeRealSize()
    {
        if (InterpolType != InterpolationType.None)
        {
            InterpolType = InterpolationType.None;
            CurImage = CurImage == FitModImage ? ModifiedImage : OriginalImage;
        }
    }

    public void SwitchCurImage()
    {
        if (ModifiedImage != null)
        {
            if (InterpolType == InterpolationType.None)
                CurImage = CurImage == OriginalImage ? ModifiedImage : OriginalImage;
            
            else
                CurImage = CurImage == FitModImage ? FitOrigImage : FitModImage;
        }
    }

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}