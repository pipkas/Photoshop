using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Photoshop.Src;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Avalonia;
using Avalonia.Input;
using Avalonia.Layout;
using System.ComponentModel;
using System;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Photoshop.UI;

public partial class MainWindow : Window
{
    private DispatcherTimer resizeTimer;
    private readonly PhotoshopManager painter;
    private readonly UISettings settings;
    public WriteableBitmap Bitmap {get; set;}

    public MainWindow()
    {
        InitializeComponent();
        ViewportFrame.StrokeDashArray = [4, 4];

        settings = new UISettings();
        painter = new PhotoshopManager();
        painter.PropertyChanged += Settings_PropertyChanged;
        PropertyChanged += (_, e) =>
        {
            if (e.Property == ClientSizeProperty)
                HandleWindowResize(ClientSize);
        };

        resizeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };

        resizeTimer.Tick += (_, __) =>
        {
            resizeTimer.Stop();

            if (painter.InterpolType != InterpolationType.None)
            {
                painter.FitToScreen(
                    painter.InterpolType,
                    new System.Drawing.Size(
                        (int)ImageContainer.Width,
                        (int)ImageContainer.Height));
            }
        };
        
        InitBitmap(painter.CurImage.Width, painter.CurImage.Height);
        UpdateBitmap();
        MakeComboBox();
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

        CanvasImage.Source = Bitmap;
    }

    private void MakeComboBox()
    {
        foreach (var filter in painter.Filters)
        {
            var attr = filter.GetFilterInfo();
            var comboItem = new ComboBoxItem
            {
                Padding = new Thickness(0)
            };
            ToolTip.SetTip(comboItem, attr.NameRu);

            var button = new Button
            {
                Content = attr.NameEn,
                Tag = attr.NameEn,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            button.Click += OnFilterClick;

            comboItem.Content = button;

            FiltersComboBox.Items.Add(comboItem);
        }
    }

    private async void OnFilterClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string filterName)
        {
            var filter = FilterFinder.FindFilter(painter.Filters, filterName) 
                ?? throw new InvalidOperationException($"There isn't filter with name {filterName}!");

            var parameters = filter.GetFilterParams();
            if (parameters == null || parameters.Length == 0)
                painter.UseFilter(filter, null);

            if (parameters != null && parameters.Length != 0)
            {
                var dialog = painter.CurFilter != null && filter.GetFilterInfo().NameEn.Equals(painter.CurFilter.GetFilterInfo().NameEn) 
                    ? new FilterSettingsWindow(parameters, painter.CurFilter.Parameters)
                    : new FilterSettingsWindow(parameters, null);

                if (dialog != null)
                {
                    await dialog.ShowDialog(this);
                    if (dialog.Params != null)
                        painter.UseFilter(filter, dialog.Params);
                }
            }
        }
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
    private void OnFitClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string interpolName 
            && Enum.TryParse(interpolName, true, out InterpolationType interpolType))
        {
            painter.FitToScreen(interpolType, 
            new System.Drawing.Size((int)ImageContainer.Width, (int)ImageContainer.Height));
        }
    }

    private void OnPixelToPixelClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button)
            painter.MakeRealSize();
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(painter.CurImage))
        {
            if (CanvasImage.Width != painter.CurImage.Width || CanvasImage.Height != painter.CurImage.Height)
                InitBitmap(painter.CurImage.Width, painter.CurImage.Height);

            UpdateBitmap();
        }
    }
    
    private void HandleWindowResize(Size newWindowSize)
    {
        ImageContainer.Width = (int)newWindowSize.Width - UISettings.DeltaWindowWidth;
        ImageContainer.Height = (int)newWindowSize.Height - UISettings.DeltaWindowHeight;
        resizeTimer.Stop();
        resizeTimer.Start();
    }

    private void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        painter.SwitchCurImage();
    }
}
       