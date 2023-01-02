using BLREdit.Import;
using BLREdit.UI.Windows;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.MagiCow;

public sealed class MagiCowServerInfo
{
    public int PlayerCount { get; set; } = 0;
    public string Map { get; set; } = "Lobby";
    public string[] PlayerList { get; set; } = System.Array.Empty<string>();
    public string ServerName { get; set; } = "";
    public string GameMode { get; set; } = "DM";

    public bool IsOnline { get; set; } = false;

    private BLRMap map;
    [JsonIgnore] public BLRMap BLRMap
    {
        get
        {
            if (map is null) { foreach (var m in MapModeSelect.Maps) { if (m.MagiCowName == Map) { map = m; break; } } }
            return map;
        }
    }

    private ObservableCollection<string> list;
    [JsonIgnore] public ObservableCollection<string> List
    {
        get
        {
            if (list is null) { list = new() { $"{PlayerCount}/16" }; foreach (var player in PlayerList) { list.Add($"{player}"); } }
            return list;
        }
    }

    public override string ToString()
    {
        return LoggingSystem.ObjectToTextWall(this);
    }
}
