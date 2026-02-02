using System;

namespace BLREdit.Game;

public sealed class ServerLaunchParameters
{
    public int ClientId { get; set; } = -1;
    public bool WatchDog { get; set; } = true;
    public string ServerName { get; set; } = "Custom Server";
    public string Playlist { get; set; } = "DM";
    public string Mode { get; set; } = "";
    public string Map { get; set; } = "";
    public int Port { get; set; } = 7777;
    public int BotCount { get; set; } = 2;
    public int MaxPlayers { get; set; } = 16;
    public int TimeLimit { get; set; } = 100;
    public int SCP { get; set; }
    public string[] RequiredModules { get; set; } = [];
}