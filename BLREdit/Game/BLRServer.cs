using BLREdit.API.REST_API.MagiCow;
using BLREdit.API.REST_API.Server;
using BLREdit.Export;
using BLREdit.UI;
using BLREdit.UI.Windows;

using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BLREdit.Game;

public sealed class BLRServer : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    #region Overrides
    public override bool Equals(object obj)
    {
        if (obj is BLRServer server)
        { return 
                ServerAddress.Equals(server.ServerAddress) && 
                Port.Equals(server.Port) &&
                InfoPort.Equals(server.InfoPort);
        }
        else
        { return false; }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    #endregion Overrides

    [JsonIgnore] public UIBool IsOnline { get { if (ServerInfo?.IsOnline ?? false) { return new(ServerInfo.IsOnline); } else { return new(MagiInfo.IsOnline); } } }
    [JsonIgnore] public bool IsDefaultServer { get { return Equals(BLREditSettings.Settings.DefaultServer); } set { IsNotDefaultServer = value; OnPropertyChanged(); } }
    [JsonIgnore] public bool IsNotDefaultServer { get { return !IsDefaultServer; } set { OnPropertyChanged(); } }
    [JsonIgnore] public UIBool IsPinging { get; } = new(false);
    [JsonIgnore] public MagiCowServerInfo MagiInfo { get; private set; } = new();
    [JsonIgnore] public ServerUtilsInfo ServerInfo { get; private set; } = new();
    [JsonIgnore] public UIBool IsTeammode { get { if (ServerInfo?.IsOnline ?? false) { return new(ServerInfo?.TeamList.Count >= 2); } else if (MagiInfo?.IsOnline ?? false) { return new(MagiInfo?.TeamList?.Count >= 2); } else { return new(false); } } }
    [JsonIgnore] public string ServerDescription { get { return GetServerDescription(); } }
    [JsonIgnore] public BitmapImage MapImage { get { if (ServerInfo?.IsOnline ?? false) { return new(new Uri(ServerInfo?.BLRMap?.SquareImage)); } else if (MagiInfo?.IsOnline ?? false) { return new(new Uri(MagiInfo.BLRMap.SquareImage)); } else { return new(new Uri($"{IOResources.BaseDirectory}Assets\\textures\\t_bluescreen2.png")); } } }
    [JsonIgnore] public StringCollection PlayerList { get { if (ServerInfo?.IsOnline ?? false) { return ServerInfo.List; } else if (MagiInfo?.IsOnline ?? false) { return MagiInfo.List; } else { return new() { $"?/? Players" }; } } }
    [JsonIgnore] public StringCollection Team1List { get { if (ServerInfo?.IsOnline ?? false) { return ServerInfo.Team1List; } else if (MagiInfo?.IsOnline ?? false) { return MagiInfo.Team1List; } else { return new() { $"?/? Players" }; } } }
    [JsonIgnore] public StringCollection Team2List { get { if (ServerInfo?.IsOnline ?? false) { return ServerInfo.Team2List; } else if (MagiInfo?.IsOnline ?? false) { return MagiInfo.Team2List; } else { return new() { $"?/? Players" }; } } }

    public string ServerAddress { get; set; } = "localhost";
    [JsonIgnore] private ushort port = 7777;
    public ushort Port { get { return port; } set { port = value; OnPropertyChanged(); } }
    [JsonIgnore] private ushort infoPort = 7778;
    public ushort InfoPort { get { return infoPort; } set { infoPort = value; OnPropertyChanged(); } }
    [JsonIgnore] private bool hidden = false;
    public bool Hidden { get { return hidden; } set { hidden = value; OnPropertyChanged(); } }
    private static BlockingCollection<BLRServer> ServersToPing { get; } = new();

    static BLRServer()
    {
        for (int i = Environment.ProcessorCount; i > 0; i--)
        {
            var thread = new Thread(PingWorker)
            { Name = $"Ping Thread[{i}]", IsBackground = true };
            App.AppThreads.Add(thread);
            thread.Start();
        }
    }

    static void PingWorker()
    {
        while (App.IsRunning)
        {
            var server = ServersToPing.Take();
            if (server is null || string.IsNullOrEmpty(server.ServerAddress) || string.IsNullOrEmpty(server.IPAddress)) continue;
            if (BLREditSettings.Settings.PingHiddenServers.IsNot) { if (server.Hidden) { LoggingSystem.Log($"Skipping Hidden Server:{server}"); continue; } }
            server.IsPinging.Set(true);
            server.InternalPing();
            server.IsPinging.Set(false);
        }
    }

    [JsonIgnore]
    public string IPAddress
    {
        get
        {
            try
            {
#if NET6_0_OR_GREATER
                return Dns.GetHostEntry(ServerAddress, System.Net.Sockets.AddressFamily.InterNetwork).ToString();
#else
                var ip = Dns.GetHostEntry(ServerAddress);
                if (ip.AddressList.Length <= 0)
                {
                    return ServerAddress;
                }
                foreach (IPAddress address in ip.AddressList)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return address.ToString();
                    }
                }
#endif
            }
            catch (Exception error)
            {
                MagiInfo = new();
                ServerInfo = new();
                RefreshInfo();
                LoggingSystem.Log($"Failed to get IPAddress for {ServerAddress}\n{error}");
            }
            return null;
        }
    }

    private string GetServerDescription()
    {
        string desc;
        if (ServerInfo?.IsOnline ?? false)
        {
            desc = $"{ServerInfo.ServerName}\n{ServerInfo.GetTimeDisplay()}\nMVP: {ServerInfo.GetScoreDisplay()}\n{ServerInfo.GameModeFullName}/{ServerInfo.Playlist}\n{ServerInfo.BLRMap.DisplayName}";
        }
        else if (MagiInfo?.IsOnline ?? false)
        {
            desc = $"{MagiInfo.ServerName}\n{MagiInfo.GetTimeDisplay()}\nMVP: {MagiInfo.GetScoreDisplay()}\n{MagiInfo.GameModeFullName}/{MagiInfo.Playlist}\n{MagiInfo.BLRMap.DisplayName}";
        }
        else
        {
            desc = $"{ServerAddress}\n{ServerInfo?.BLRMap?.DisplayName}";
        }
        return desc;
    }

    public void RefreshInfo()
    {
        OnPropertyChanged(nameof(IsOnline));
        OnPropertyChanged(nameof(IsTeammode));
        OnPropertyChanged(nameof(ServerDescription));
        OnPropertyChanged(nameof(MapImage));
        OnPropertyChanged(nameof(PlayerList));
        OnPropertyChanged(nameof(Team1List));
        OnPropertyChanged(nameof(Team2List));
        MainWindow.Instance.Dispatcher.Invoke(MainWindow.Instance.RefreshServerList);
    }

    public void PingServer()
    {
        ServersToPing.Add(this);
    }

    private void InternalPing()
    {
        var server = ServerUtilsClient.GetServerInfo(this);
        var magi = MagiCowClient.GetServerInfo(ServerAddress);

        Task.WaitAll(server, magi);

        var serverInfo = server.Result;
        var magiInfo = magi.Result;

        if (serverInfo is null) { ServerInfo = new(); } else { serverInfo.IsOnline = true; ServerInfo = serverInfo; }
        if (magiInfo is null) { MagiInfo = new(); } else { magiInfo.IsOnline = true; MagiInfo = magiInfo; }

        RefreshInfo();
    }

    private void EditServer()
    {
        var window = new BLRServerWindow(this);
        window.ShowDialog();
        RefreshInfo();
    }

    private ICommand editServerCommand;
    [JsonIgnore]
    public ICommand EditServerCommand
    {
        get
        {
            editServerCommand ??= new RelayCommand(
                    param => this.EditServer()
                );
            return editServerCommand;
        }
    }

    private ICommand refreshPingCommand;
    [JsonIgnore]
    public ICommand RefreshPingCommand
    {
        get
        {
            refreshPingCommand ??= new RelayCommand(
                    param => this.PingServer()
                );
            return refreshPingCommand;
        }
    }

    private ICommand connectToServerCommand;
    [JsonIgnore]
    public ICommand ConnectToServerCommand
    {
        get
        {
            connectToServerCommand ??= new RelayCommand(
                    param => this.LaunchClient()
                );
            return connectToServerCommand;
        }
    }

    private ICommand removeServer;
    [JsonIgnore]
    public ICommand RemoveServerCommand
    {
        get
        {
            removeServer ??= new RelayCommand(
                    param => this.RemoveServer()
                );
            return removeServer;
        }
    }

    public void RemoveServer()
    { 
        MainWindow.View.ServerList.Remove(this);
    }

    public void LaunchClient()
    {
        if (BLREditSettings.Settings.DefaultClient is not null)
        { BLREditSettings.Settings?.DefaultClient?.LaunchClient(new LaunchOptions() { UserName = BLREditSettings.Settings.PlayerName, Server = this }); }
    }
}