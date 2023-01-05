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
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

    private bool isBool = false;

    public bool Is { get { return isBool; } set { SetBool(value); } }
    public bool IsNot { get { return !isBool; } set { SetBool(!value); } }

    public Visibility Visibility { get { if (isBool) { return Visibility.Visible; } else { return Visibility.Collapsed; } } }
    public Visibility VisibilityInverted { get { if (isBool) { return Visibility.Collapsed; } else { return Visibility.Visible; } } }

    public UIBool() { }
    public UIBool(bool Bool)
    { this.SetBool(Bool); }

    public void SetBool(bool target)
    {
        isBool = target;
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
