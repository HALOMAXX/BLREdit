using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BLREdit.API.Export;

public sealed class BLRPlaylistEntry : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    [JsonIgnore] private string _Map = "helodeck";
    [JsonIgnore] private string _GameMode = "DM";


    public string Map { get { return _Map; } set { _Map = value; OnPropertyChanged(); } }
    public string GameMode { get { return _GameMode; } set { _GameMode = value; OnPropertyChanged(); } }
    public BLRServerProperties Properties { get; set; } = new();

}