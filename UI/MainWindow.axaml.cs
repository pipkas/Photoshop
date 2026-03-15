using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Photoshop.Src;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Avalonia;
using Avalonia.Input;
using System.ComponentModel;
using System;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace Photoshop.UI;

public partial class MainWindow : Window
{
    private readonly PhotoshopManager painter;
    private readonly UISettings settings;
    public WriteableBitmap Bitmap {get; set;}
    
    public MainWindow()
    {
        InitializeComponent();

        settings = new UISettings();
        painter = new PhotoshopManager();
        painter.PropertyChanged += Settings_PropertyChanged;

        InitBitmap(painter.CurImage.Width, painter.CurImage.Height);
        //DataContext = painter.Settings;
    }

    private void InitBitmap(int width, int height)
    {
        CanvasImage.Width = width;
        CanvasImage.Height = height;

        Bitmap = new WriteableBitmap(
            new PixelSize((int)CanvasImage.Width , (int)CanvasImage.Height),
            new Vector(96, 96),
            PixelFormat.Rgba8888,
            AlphaFormat.Opaque);

        UpdateBitmap();
        CanvasImage.Source = Bitmap;
    }

    private async void OnHelpClick(object? sender, RoutedEventArgs e)
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandard(
            "О программе",      
            settings.HelpClickText,
            ButtonEnum.Ok 
        );
        await messageBox.ShowWindowDialogAsync(this);
    }

    private async void OnFileClick(object? sender, RoutedEventArgs e)
    {
        var options = new FilePickerOpenOptions
        {
            Title = "Открыть изображение",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Images")
                {
                    Patterns =
                    [
                        "*.png",
                        "*.jpg",
                        "*.jpeg",
                        "*.bmp",
                        "*.gif"
                    ]
                }
            ]
        };

        var files = await StorageProvider.OpenFilePickerAsync(options);

        if (files.Count == 0)
            return;

        await using var stream = await files[0].OpenReadAsync();

        var picture = Picture.LoadFromStream(stream);

        painter.OriginalImage = picture;
        InitBitmap(painter.OriginalImage.Width, painter.OriginalImage.Height);
        painter.CurImage = picture;
        painter.CurFilter = null;

        //нет необходимости подстраивать размер окна к размеру изображения
        // Width = CanvasImage.Width + UISettings.DeltaWindowCanvasWidth;
        // Height = CanvasImage.Height + UISettings.DeltaWindowCanvasHeight;
        // CenterWindow();
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        await SaveFileAsync(this);
    }

    private async Task SaveFileAsync(Window parent)
    {
        var options = new FilePickerSaveOptions
        {
            Title = "Сохранить изображение",
            SuggestedFileName = "image.png",
            FileTypeChoices =
            [
                new FilePickerFileType("PNG Image")
                {
                    Patterns = ["*.png"]
                }
            ]
        };

        var file = await parent.StorageProvider.SaveFilePickerAsync(options);

        if (file != null)
        {
            await using var stream = await file.OpenWriteAsync();
            FileManager.SaveToPng(painter.CurImage, stream);
        }
    }

    private void OnFilter0ParamsClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string filterName)
        {
            painter.UseFilter(filterName);
        }  
    }

    private async void OnFilter1ParamsClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string filterName)
        {
            var dialog = new FilterSettings1ParamWindow(filterName);
            if (dialog != null)
            {
                await dialog.ShowDialog(this);
                if (dialog.Value1.HasValue)
                    painter.UseFilter(filterName, dialog.Value1.Value);
            }
        }  
    }

    private async void OnFilter2ParamsClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string filterName)
        {
            var dialog = new FilterSettings2ParamWindow(filterName);
            if (dialog != null)
            {
                await dialog.ShowDialog(this);
                if (dialog.Value1.HasValue && dialog.Value2.HasValue)
                    painter.UseFilter(filterName, dialog.Value1.Value, 
                                                dialog.Value2.Value);
            }
        }  
    }

    private async void OnFilter3ParamsClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string filterName)
        {
            var dialog = new FilterSettings3ParamWindow(filterName);
            if (dialog != null)
            {
                await dialog.ShowDialog(this);
                if (dialog.Value1.HasValue && dialog.Value2.HasValue && dialog.Value3.HasValue)
                    painter.UseFilter(filterName, dialog.Value1.Value, 
                                                dialog.Value2.Value, 
                                                dialog.Value3.Value);
            }
        }  
    }

    private unsafe void UpdateBitmap()
    {
        if (Bitmap == null) return;

        var picture = painter.CurImage;

        using var fb = Bitmap.Lock();

        var dst = new Span<byte>(
            (void*)fb.Address,
            picture.Width * picture.Height * Picture.ColorBytesCount);

        picture.PixelsBuffer.CopyTo(dst);

        CanvasImage?.InvalidateVisual();
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(painter.CurImage))
        {
            UpdateBitmap();
        }
    }
    
    // private void HandleWindowResize(Size newWindowSize)
    // {
    //     var newCanvasWidth = (int)newWindowSize.Width - UISettings.DeltaWindowCanvasWidth;
    //     var newCanvasHeight = (int)newWindowSize.Height - UISettings.DeltaWindowCanvasHeight;
    //     if (newCanvasWidth <= CanvasImage.Width 
    //         || newCanvasHeight <= CanvasImage.Height)
    //     {
    //         return;
    //     }
    //     painter.Image.Resize(newCanvasWidth, newCanvasHeight);

    //     InitBitmap(newCanvasWidth, newCanvasHeight);
    // }

    // private void CenterWindow()
    // {
    //     var x = (Screens.Primary.Bounds.Width - (int)Width) / 2;
    //     var y = (Screens.Primary.Bounds.Height - (int)Height) / 2;
    
    //     Position = new PixelPoint(x, y);
    // }

    private void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (painter.ModifiedImage == null)
            return;
        
        painter.CurImage = painter.CurImage == painter.OriginalImage 
            ? painter.ModifiedImage
            : painter.OriginalImage;
    }
}
       