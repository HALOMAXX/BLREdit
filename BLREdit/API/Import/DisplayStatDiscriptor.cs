using System;
using System.Windows.Media;

namespace BLREdit.Import;

public sealed class DisplayStatDiscriptor
{
    public string PropertyName { get; set; } = "";
    public string Value { get; set; } = "";
    public string Description { get; set; } = "";
    public Brush ValueColor { 
        get {
            if ((UI.MainWindow.Self?.CurrentSortingPropertyName?.Equals(PropertyName) ?? false))
            {
                return HighlightValueColor;
            }
            else
            {
                return DefaultValueColor;
            }
        }
    }
    public Brush DescriptionColor
    {
        get
        {
            if ((UI.MainWindow.Self?.CurrentSortingPropertyName?.Equals(PropertyName) ?? false))
            {
                return HighlightDescriptorColor;
            }
            else
            {
                return DefaultDescriptionColor;
            }
        }
    }
    public Brush DefaultValueColor { get; set; } = new SolidColorBrush(Color.FromArgb(255, 116, 148, 160));
    public Brush DefaultDescriptionColor { get; set; } = new SolidColorBrush(Color.FromArgb(255,255,255,255));
    public Brush HighlightValueColor { get; set; } = new SolidColorBrush(Color.FromArgb(255, 180, 100, 50));
    public Brush HighlightDescriptorColor { get; set; } = new SolidColorBrush(Color.FromArgb(255, 255, 136, 0));

    public DisplayStatDiscriptor()
    { }
}

[Flags]
public enum StatsEnum
{ 
    None = 0,
    Normal = 1,
    Inverted = 2
}
