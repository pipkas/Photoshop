using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Photoshop.UI;

public partial class FilterSettings3ParamWindow: Window, INotifyPropertyChanged
{
    private double? value1 = 0;
    private double? value2 = 0;
    private double? value3 = 0;
    public event PropertyChangedEventHandler? PropertyChanged;
    public double? Value1 {
        get => value1;
        set
        {
            if (value1 != value)
            {
                value1 = value;
                OnPropertyChanged();
            }
        }
    }

    public double? Value2 {
        get => value2;
        set
        {
            if (value2 != value)
            {
                value2 = value;
                OnPropertyChanged();
            }
        }
    }

    public double? Value3 {
        get => value3;
        set
        {
            if (value3 != value)
            {
                value3 = value;
                OnPropertyChanged();
            }
        }
    }

    public FilterSettings3ParamWindow(string filterName)
    {
        InitializeComponent();

        BtnOk.IsEnabled = true;
        DataContext = this;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Value1 = null;
        Close();
    }
}