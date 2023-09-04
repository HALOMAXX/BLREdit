using BLREdit.API.Export;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

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

    public ShareableProfile ConvertToShareable()
    {
        var loadouts = new ObservableCollection<ShareableLoadout>
        {
            Loadout1.ConvertToShareable(),
            Loadout2.ConvertToShareable(),
            Loadout3.ConvertToShareable()
        };

        var profile = new ShareableProfile()
        {
            Name = Name,
            Loadouts = loadouts
        };
        return profile;
    }
}