using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace BLREdit.UI;

[JsonConverter(typeof(JsonUIBoolConverter))]
public sealed class UIBool : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

    private bool isBool;

    private Color colour = Color.FromArgb(14, 158, 158, 158);
    private Color colourInverted = Color.FromArgb(255, 255, 0, 0);

    public bool Is { get { return isBool; } set { Set(value); } }
    public bool IsNot { get { return !isBool; } set { Set(!value); } }
    public Color Colour { get { if (isBool) { return colour; } else { return colourInverted; } } }
    public Color ColourInverted { get { if (isBool) { return colourInverted; } else { return colour; } } }
    public Brush Brush { get { if (isBool) { return new SolidColorBrush(colour); } else { return new SolidColorBrush(colourInverted); } } }
    public Brush BrushInverted { get { if (isBool) { return new SolidColorBrush(colourInverted); } else { return new SolidColorBrush(colour); } } }
    public Visibility Visibility { get { if (isBool) { return Visibility.Visible; } else { return Visibility.Collapsed; } } }
    public Visibility VisibilityInverted { get { if (isBool) { return Visibility.Collapsed; } else { return Visibility.Visible; } } }

    public UIBool() { }
    public UIBool(bool Bool)
    { this.Set(Bool); }
    public UIBool(bool Bool, Color isColor, Color isNotColor)
    { this.Set(Bool); colour = isColor; colourInverted = isNotColor; }
    public UIBool(int Bool)
    { this.Set(Bool); }

    public void Set(bool target)
    {
        isBool = target;
        NotifyChange();
    }

    public void Set(int target)
    {
        if (target > 0)
            isBool = true;
        else
            isBool = false;
        NotifyChange();
    }

    private void NotifyChange()
    {
        OnPropertyChanged(nameof(Is));
        OnPropertyChanged(nameof(IsNot));
        OnPropertyChanged(nameof(Colour));
        OnPropertyChanged(nameof(ColourInverted));
        OnPropertyChanged(nameof(Brush));
        OnPropertyChanged(nameof(BrushInverted));
        OnPropertyChanged(nameof(Visibility));
        OnPropertyChanged(nameof(VisibilityInverted));
    }

    public override string ToString()
    {
        return Is.ToString();
    }
}
