using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace BLREdit.UI;

[JsonConverter(typeof(JsonUIBoolConverter))]
public sealed class UIBool : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

    private bool isBool = false;

    public bool Is { get { return isBool; } set { Set(value); } }
    public bool IsNot { get { return !isBool; } set { Set(!value); } }

    public Visibility Visibility { get { if (isBool) { return Visibility.Visible; } else { return Visibility.Collapsed; } } }
    public Visibility VisibilityInverted { get { if (isBool) { return Visibility.Collapsed; } else { return Visibility.Visible; } } }

    public UIBool() { }
    public UIBool(bool Bool)
    { this.Set(Bool); }
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
        OnPropertyChanged(nameof(Visibility));
        OnPropertyChanged(nameof(VisibilityInverted));
    }

    public override string ToString()
    {
        return Is.ToString();
    }
}
