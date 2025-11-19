using BLREdit.Import;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

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
    [JsonIgnore] private BLRServerProperties _Properties = new();

    public string Map { get { return _Map; } set { _Map = value; OnPropertyChanged(); } }
    public string GameMode { get { return _GameMode; } set { _GameMode = value; OnPropertyChanged(); } }
    public BLRServerProperties Properties { get { return _Properties; } set { _Properties = value; OnPropertyChanged(); } }

    [JsonIgnore]
    public BitmapImage MapImage
    {
        get
        {
            if (!string.IsNullOrEmpty(Map) && BLRMap.FindPlaylistName(Map) is BLRMap map)
            {
                return new(new Uri(map.SquareImage));
            }
            else
            {
                return new(new Uri($"{IOResources.BaseDirectory}Assets\\textures\\t_bluescreen2.png"));
            }
        }
    }

    [JsonIgnore]
    public BLRMap? MapInfo
    {
        get
        {
            return BLRMap.FindPlaylistName(Map);
        }
    }

}