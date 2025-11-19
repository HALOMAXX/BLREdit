using BLREdit.Import;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace BLREdit.API.Export
{
    public sealed class BLRPlaylist : INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Events

        #region PrivatFields
        [JsonIgnore] private string _Name = "Playlist Name";
        [JsonIgnore] private string _ClientVersion = "v302";
        [JsonIgnore] private ObservableCollection<BLRPlaylistEntry> _Entries = [];
        #endregion PrivateFields

        public string Name { get { return _Name; } set { _Name = value; OnPropertyChanged(); } }
        public string ClientVersion { get { return _ClientVersion; } set { _ClientVersion = value; OnPropertyChanged(); } }
        public ObservableCollection<BLRPlaylistEntry> Entries { get { return _Entries; } set { _Entries = value; OnPropertyChanged(); } }
        [JsonIgnore] public BitmapImage MapImage { 
            get {
                if (_Entries != null && _Entries.Count > 0 && !string.IsNullOrEmpty(_Entries[0].Map) && BLRMap.FindPlaylistName(_Entries[0].Map) is BLRMap map){
                    return new(new Uri(map.SquareImage));}
                else {
                    return new(new Uri($"{IOResources.BaseDirectory}Assets\\textures\\t_bluescreen2.png")); }
            }
        }
    }
}
