using System.Windows.Media;

namespace BLREdit;

public struct DisplayStatDiscriptor
{
    public string PropertyName { get; set; } = "";
    public string Value { get; set; } = "";
    public string Description { get; set; } = "";
    public Brush ValueColor { 
        get {
            if (UI.MainWindow.Self.CurrentSortingPropertyName?.Equals(PropertyName) ?? false)
            {
                return HighlightColor;
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
            if (UI.MainWindow.Self.CurrentSortingPropertyName?.Equals(PropertyName) ?? false)
            {
                return HighlightColor;
            }
            else
            {
                return DefaultDescriptionColor;
            }
        }
    }
    public Brush DefaultValueColor { get; set; } = new SolidColorBrush(Color.FromArgb(255, 116, 148, 160));
    public Brush DefaultDescriptionColor { get; set; } = new SolidColorBrush(Color.FromArgb(255,255,255,255));
    public Brush HighlightColor { get; set; } = new SolidColorBrush(Color.FromArgb(255, 255, 105, 0));

    public DisplayStatDiscriptor()
    { }
}
