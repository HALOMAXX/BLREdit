using BLREdit.API.REST_API.MagiCow;
using BLREdit.API.REST_API.Server;
using BLREdit.UI;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace BLREdit.Model.BLR;

public sealed class BLRServerStatus : INotifyPropertyChanged
{
    #region Event
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Event

    private readonly BLRServerModel _model;
    private MagiCowServerInfo? _magiInfo;
    private ServerUtilsInfo? _utilsInfo;

    [JsonIgnore] public UIBool IsOnline { get; } = new(false);
    [JsonIgnore] public UIBool IsTeammode { get; } = new(false);
    [JsonIgnore] public string MapImage
    {
        get
        {
            if (_magiInfo is not null) { return _magiInfo.BLRMap.SquareImage; }
            if (_utilsInfo is not null) { return _utilsInfo.BLRMap.SquareImage; }
            return $"{IOResources.BaseDirectory}Assets\\textures\\t_bluescreen2.png";
        }
    }
    [JsonIgnore] public ObservableCollection<string> PlayerList
    {
        get
        {
            if (_magiInfo is not null) { return _magiInfo.List; }
            if (_utilsInfo is not null) { return _utilsInfo.List; }
            return new() { $"?/? Players" };
        }
    }
    [JsonIgnore] public ObservableCollection<string> Team1List 
    { 
        get
        {
            if (_magiInfo is not null) { return _magiInfo.Team1List; }
            if (_utilsInfo is not null) { return _utilsInfo.Team1List; }
            return new() { $"?/? Players" };
        }
    }
    [JsonIgnore]
    public ObservableCollection<string> Team2List
    {
        get
        {
            if (_magiInfo is not null) { return _magiInfo.Team2List; }
            if (_utilsInfo is not null) { return _utilsInfo.Team2List; }
            return new() { $"?/? Players" };
        }
    }
    [JsonIgnore] public string? ServerDescription 
    { 
        get 
        {
            if (_magiInfo is not null) { return $"{_magiInfo.ServerName}\n{_magiInfo.GetTimeDisplay()}\nMVP: {_magiInfo.GetScoreDisplay()}\n{_magiInfo.GameModeFullName}/{_magiInfo.Playlist}\n{_magiInfo.BLRMap.DisplayName}"; }
            if (_utilsInfo is not null) { return $"{_utilsInfo.ServerName}\n{_utilsInfo.GetTimeDisplay()}\nMVP: {_utilsInfo.GetScoreDisplay()}\n{_utilsInfo.GameModeFullName}/{_utilsInfo.Playlist}\n{_utilsInfo.BLRMap.DisplayName}"; }
            return _model.ServerAddress;
        } 
    }

    public BLRServerStatus(BLRServerModel model)
    { 
        _model = model;
    }

    public void NotifyAllProperties()
    {
        IsOnline.Set(_magiInfo is not null || _utilsInfo is not null);

        CheckTeammode();

        OnPropertyChanged(nameof(MapImage));
        OnPropertyChanged(nameof(PlayerList));
        OnPropertyChanged(nameof(Team1List));
        OnPropertyChanged(nameof(Team2List));
        OnPropertyChanged(nameof(ServerDescription));
    }

    private void CheckTeammode()
    {
        if (_magiInfo is not null)
        {
            IsTeammode.Set(_magiInfo.BLRMode.IsTeammode);
        }
        else if (_utilsInfo is not null)
        {
            IsTeammode.Set(_utilsInfo.BLRMode.IsTeammode);
        }
        else
        { IsTeammode.Set(false); }
    }

    public void ApplyInfo(MagiCowServerInfo? magiInfo)
    {
        if (_magiInfo == magiInfo) return;
        _magiInfo = magiInfo;
        NotifyAllProperties();
    }

    public void ApplyInfo(ServerUtilsInfo? utilsInfo)
    {
        if (_utilsInfo == utilsInfo) return;
        _utilsInfo = utilsInfo;
        NotifyAllProperties();
    }
}
