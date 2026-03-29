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
using Avalonia.Media;

namespace Photoshop.UI;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer resizeTimer;
    private InterpolationType? interpolationType;
    private bool _isPanning;
    private Point _startPoint;
    private Vector _startOffset;
    private readonly PhotoshopManager painter;
    private readonly UISettings settings;
    private ImageViewer? imageViewer;
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
        MakeToolbarButtons();
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
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = Brushes.Transparent
            };

            button.Click += OnFilterClick;

            comboItem.Content = button;

            FiltersComboBox.Items.Add(comboItem);
        }
    }

    private void MakeToolbarButtons()
    {
        foreach (var filter in painter.Filters)
        {
            var attr = filter.GetFilterInfo();
            var button = new Button
            {
                Tag = attr.NameEn,
                Width = 32,
                Height = 32,
                Background = Brushes.Transparent
            };
            ToolTip.SetTip(button, attr.NameRu);

            button.Click += OnFilterClick;

            if (attr.ImagePath != null)
            {
                var image = new Image
                {
                    Width = 24,
                    Height = 24,
                    Stretch = Stretch.Uniform,
                    Source = new Bitmap(attr.ImagePath)
                };

                button.Content = image;
                LabelPanel.Children.Add(button);
            }
            else
            {
                //все дизеринги добавить в отдельную панельку
            }
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

        imageViewer = new ImageViewer(files[0].Path.LocalPath);
        if (imageViewer.ImagesCount <= 1)
        {
            PrevImageButton.IsVisible = false;
            NextImageButton.IsVisible = false;
        }
        else
        {
            PrevImageButton.IsVisible = true;
            NextImageButton.IsVisible = true;
        }

        painter.CreateNewOrigImage(picture);
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

    private void OnPrevImageClick(object? sender, RoutedEventArgs e){
        if (imageViewer == null) return;
        var picture = imageViewer.Previous();
        ImageScrollViewer.Offset = new Vector(0, 0);
        if (picture != null)
            painter.CreateNewOrigImage(picture);
    }

    private void OnNextImageClick(object? sender, RoutedEventArgs e){
        if (imageViewer == null) return;
        var picture = imageViewer.Next();
        ImageScrollViewer.Offset = new Vector(0, 0);
        if (picture != null)
            painter.CreateNewOrigImage(picture);
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

    private async void OnFitClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button)
        {
            var dialog = new FitTypesWindow(interpolationType);
            await dialog.ShowDialog(this);
            if (dialog.InterpolType != null){
                painter.FitToScreen(dialog.InterpolType.Value, 
            new System.Drawing.Size((int)ImageContainer.Width, (int)ImageContainer.Height));
                interpolationType = dialog.InterpolType;
            }
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


    private void OnImagePointerMoved(object sender, PointerEventArgs e)
    {
        if (!_isPanning) return;

        var currentPoint = e.GetPosition(ImageContainer);

        var delta = currentPoint - _startPoint;

        ImageScrollViewer.Offset = new Vector(
            _startOffset.X - delta.X,
            _startOffset.Y - delta.Y);
    }

    private void OnImagePointerReleased(object sender, PointerReleasedEventArgs e)
    {
        _isPanning = false;
        e.Pointer.Capture(null);
    }

    private void OnPointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
    {
        _isPanning = false;
    }

    private void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isPanning = true;
        _startPoint = e.GetPosition(ImageContainer);
        _startOffset = ImageScrollViewer.Offset;

        e.Pointer.Capture(CanvasImage);
    }

    private void OnImageDoubleTapped(object sender, TappedEventArgs e)
    {
        painter.SwitchCurImage();
    }
}