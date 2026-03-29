using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Photoshop.Src;

namespace Photoshop.UI;

public partial class FitTypesWindow: Window
{
    public InterpolationType? InterpolType = InterpolationType.Bilinear;
    public FitTypesWindow(InterpolationType? prevInterpolType)
    {
        InitializeComponent();

        if (prevInterpolType != null){
            InterpolType = prevInterpolType;
        }

        SelectRadioButton();

        BtnOk.IsEnabled = true;
    }

    private void OnFitClick(object? sender, RoutedEventArgs e){
        if (sender is RadioButton radio && radio.Tag is string tagValue 
            && Enum.TryParse<InterpolationType>(tagValue, out var type)){
                InterpolType = type;
        }
    }

    private void SelectRadioButton()
    {
        switch (InterpolType!.Value)
        {
            case InterpolationType.Cubic:
                RadioCubic.IsChecked = true;
                break;
            case InterpolationType.Bilinear:
                RadioBilinear.IsChecked = true;
                break;
            case InterpolationType.Step:
                RadioStep.IsChecked = true;
                break;
        }
    }
    
    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        InterpolType = null;
        Close();
    }
}