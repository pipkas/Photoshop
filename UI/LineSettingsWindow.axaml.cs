using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Photoshop.UI;

public partial class LineSettingsWindow : Window
{
    private const int MinThickness = 1;
    private const int MaxThickness = 100;
    public int? ResultThickness { get; private set; }

    public LineSettingsWindow(int thickness = 1)
    {
        InitializeComponent();
        ThicknessInput.Text = thickness.ToString();
        BtnOk.IsEnabled = true;
        ThicknessInput.Focus();
        ThicknessInput.SelectAll();
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = ThicknessInput.Text;
        ErrorText.IsVisible = false;
        BtnOk.IsEnabled = false;

        if (string.IsNullOrWhiteSpace(text))
            return;

        if (!ValidInput(text))
        {
            ErrorText.IsVisible = true;
            return;
        }

        BtnOk.IsEnabled = true;
    }

    private bool ValidInput(string input)
    {
        if (!int.TryParse(input, out var value))
        {
            ErrorText.Text = "Введите целое число";
            return false;
        }

        if (value < MinThickness || value > MaxThickness)
        {
            ErrorText.Text = $"Значение должно быть от {MinThickness} до {MaxThickness}";
            return false;
        }

        return true;
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        if (int.TryParse(ThicknessInput.Text, out var value))
        {
            ResultThickness = value;
            Close();
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        ResultThickness = null;
        Close();
    }
}