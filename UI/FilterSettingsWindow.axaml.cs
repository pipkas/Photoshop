using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Photoshop.Src;

namespace Photoshop.UI;

public partial class FilterSettingsWindow: Window, INotifyPropertyChanged
{
    private FilterParameterAttribute[] ParamAttr {get;}
    public event PropertyChangedEventHandler? PropertyChanged;

    public double[]? Params {get; private set;}

    public FilterSettingsWindow(FilterParameterAttribute[] parameters, double[]? prevParams)
    {
        InitializeComponent();

        ParamAttr = parameters;
        MakeParamFields(prevParams);

        BtnOk.IsEnabled = true;
        ClearError();
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void MakeParamFields(double[]? prevParams)
    {
        int i = 0;
        foreach (var parameter in ParamAttr)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var label = new TextBlock
            {
                Text = parameter.Name,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 150
            };
            var val = prevParams?[i++] ?? parameter.Min;

            var slider = new Slider
            {
                Minimum = parameter.Min,
                Maximum = parameter.Max,
                Width = 120,
                TickFrequency = TakeFreq(parameter.Type),
                IsSnapToTickEnabled = true,
                Value = val
            };

            var textBox = new TextBox
            {
                Width = 80,
                Text = val.ToString("0.##")
            };

            slider.PropertyChanged += (_, e) =>
            {
                if (e.Property.Name == nameof(Slider.Value))
                {
                    textBox.Text = slider.Value.ToString("0.##");
                    ValidateParameter(parameter, slider.Value);
                }
            };

            textBox.LostFocus += (_, __) =>
            {
                if (double.TryParse(textBox.Text, out var val))
                {
                    slider.Value = val;
                    ValidateParameter(parameter, val);
                }
                else
                {
                    ShowError($"Параметр {parameter.Name}: неверное число");
                }
            };

            row.Children.Add(label);
            row.Children.Add(slider);
            row.Children.Add(textBox);

            ParamsContainer.Children.Add(row);
        }
    }

    private static double TakeFreq(ParameterType type) => type switch
    {
        ParameterType.Integer => 1,
        ParameterType.Double => 0.01,
        ParameterType.OddInteger => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    private void ValidateParameter(FilterParameterAttribute attr, double value)
    {
        var error = Validator.Validate(attr, value);

        if (error != null)
        {
            ShowError(error);
            BtnOk.IsEnabled = false;
        }
        else
        {
            ClearError();
            BtnOk.IsEnabled = AllParamsValid();
        }
    }

    private bool AllParamsValid()
    {
        foreach (var child in ParamsContainer.Children)
        {
            if (child is StackPanel row)
            {
                var slider = row.Children.OfType<Slider>().FirstOrDefault();
                var attr = ParamAttr[ParamsContainer.Children.IndexOf(row)];

                if (slider != null)
                {
                    if (Validator.Validate(attr, slider.Value) != null)
                        return false;
                }
            }
        }

        return true;
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.IsVisible = true;
    }

    private void ClearError() => ErrorText.IsVisible = false;
    
    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Params = new double[ParamAttr.Length];
        int i = 0;
        foreach (var child in ParamsContainer.Children)
        {
            if (child is StackPanel row)
            {
                var slider = row.Children.OfType<Slider>().FirstOrDefault();
                if (slider != null)
                    Params[i++] = slider.Value;
            }
        }
        
        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Params = null;
        Close();
    }
}