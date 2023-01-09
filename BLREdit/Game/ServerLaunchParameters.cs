using System;

namespace BLREdit.Game;

public sealed class ServerLaunchParameters
{
    public int ClientId = -1;
    public bool WatchDog = true;
    public string ServerName = "Custom Server";
    public string Playlist = "DM";
    public int Port = 7777;
    public int BotCount = 2;
    public int MaxPlayers = 16;
    public string[] RequiredModules = Array.Empty<string>();
}