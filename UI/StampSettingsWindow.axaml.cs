using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition.Transport;
using Photoshop.Src;

namespace Photoshop.UI;

public partial class StampSettingsWindow: Window, INotifyPropertyChanged
{
    // private int _radius = 30;
    // private int _rotation = 0;
    // public const int MinVertexCount = 3;
    // public const int MaxVertexCount = 16;
    // public const int RadiusMin = 1;
    // public const int RadiusMax = 500;
    // public const int RotationMin = 0;
    // public const int RotationMax = 360;
    // public event PropertyChangedEventHandler? PropertyChanged;
    // public Stamp? ResultedStamp { get; private set; }
    // public StampType SelectedShape { get; private set; } = StampType.Star;
    // public int VertexCount { get; private set; } = 5;
    // public int Radius
    // {
    //     get => _radius;
    //     set
    //     {
    //         if (_radius != value)
    //         {
    //             ErrorText.IsVisible = false;
    //             BtnOk.IsEnabled = false;
    //             var validated = ValidValue(RadiusMin, RadiusMax, value.ToString());
    //             if (validated.HasValue)
    //             {
    //                 _radius = validated.Value;
    //                 OnPropertyChanged();
    //                 BtnOk.IsEnabled = true;
    //             }
    //             else
    //                 ErrorText.IsVisible = true;
    //         }
    //     }
    // }

    // public int Rotation
    // {
    //     get => _rotation;
    //     set
    //     {
    //         if (_rotation != value)
    //         {
    //             ErrorText.IsVisible = false;
    //             BtnOk.IsEnabled = false;
    //             var validated = ValidValue(RotationMin, RotationMax, value.ToString());
    //             if (validated.HasValue)
    //             {
    //                 _rotation = validated.Value;
    //                 OnPropertyChanged();
    //                 BtnOk.IsEnabled = true;
    //             }
    //             else
    //                 ErrorText.IsVisible = true;
    //         }
    //     }
    // }

    // public StampSettingsWindow(Stamp stamp)
    // {
    //     InitializeComponent();

    //     SelectedShape = stamp.StampType;
    //     Rotation = stamp.RotationDegrees;
    //     Radius = stamp.RadiusSize;
    //     VertexCount = stamp.VertexCount;

    //     FillFields();

    //     BtnOk.IsEnabled = true;
    //     DataContext = this;
    // }

    // public StampSettingsWindow()
    // {
    //     InitializeComponent();
    //     FillFields();
    //     BtnOk.IsEnabled = true;
    //     DataContext = this;
    // }

    // private void FillFields()
    // {
    //     VertexCountInput.Text = VertexCount.ToString();
    //     RadiusInput.Text = Radius.ToString();
    //     RotationInput.Text = Rotation.ToString();
    //     SetShape();
    // }

    // protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
    //     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    
    // private void OnShapeClick(object? sender, RoutedEventArgs e)
    // {
    //     if (sender is RadioButton button && button.Tag is string name)
    //     {
    //         SelectedShape = Enum.Parse<StampType>(name);
    //     }
    // }

    // private void OnVertexChanged(object? sender, TextChangedEventArgs e)
    // {
    //     var text = VertexCountInput.Text;
    //     ErrorText.IsVisible = false;
    //     BtnOk.IsEnabled = false;

    //     if (string.IsNullOrWhiteSpace(text))
    //         return;

    //     if (ValidValue(MinVertexCount, MaxVertexCount, text) is { } validated)
    //     {
    //         VertexCount = validated;
    //         BtnOk.IsEnabled = true;
    //     }
    //     else
    //         ErrorText.IsVisible = true;
    // }

    // private double? ValidValue(int minValue, int maxValue, string input)
    // {
    //     if (!int.TryParse(input, out var value))
    //     {
    //         ErrorText.Text = "Введите целое число";
    //         return null;
    //     }

    //     if (value < minValue || value > maxValue)
    //     {
    //         ErrorText.Text = $"Значение должно быть от {minValue} до {maxValue}";
    //         return null;
    //     }
    //     return value;
    // }

    // private void SetShape()
    // {
    //     foreach (var control in ShapeInput.Children)
    //     {
    //         if (control is RadioButton rb && rb.Tag?.ToString() == SelectedShape.ToString())
    //         {
    //             rb.IsChecked = true;
    //             break;
    //         }
    //     }
    // }

    // private void OnOkClick(object? sender, RoutedEventArgs e)
    // {
    //     ResultedStamp = new Stamp(SelectedShape, VertexCount, 
    //                             Radius, Rotation);
    //     Close();
    // }

    // private void OnCancelClick(object? sender, RoutedEventArgs e)
    // {
    //     ResultedStamp = null;
    //     Close();
    // }

}