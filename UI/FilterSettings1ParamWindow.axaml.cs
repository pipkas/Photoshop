using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Photoshop.UI;

public partial class FilterSettings1ParamWindow: Window, INotifyPropertyChanged
{
    private double? value1 = 0;
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

    public FilterSettings1ParamWindow(string filterName)
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