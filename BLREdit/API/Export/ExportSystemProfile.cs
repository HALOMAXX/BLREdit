using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BLREdit.Export;

public sealed class ExportSystemProfile : MagiCowsProfile, INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    [JsonIgnore] public string Name { get { return '(' + Index.ToString() + ')' + PlayerName; } }
    public int Index { get; set; }

    public void RefreshInfo()
    {
        OnPropertyChanged(nameof(Name));
    }

    public ExportSystemProfile Duplicate()
    {
        var dup = base.Clone() as ExportSystemProfile ?? new();
        dup.Index = ExportSystem.Profiles.Count;
        ExportSystem.Profiles.Add(dup);
        return dup;
    }
}
