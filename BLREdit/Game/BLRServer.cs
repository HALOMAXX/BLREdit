using BLREdit.API.REST_API.Server;
using BLREdit.API.Utils;
using BLREdit.UI;
using BLREdit.UI.Windows;

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BLREdit.Game;

public sealed class BLRServer : INotifyPropertyChanged
{
    public static string EmptyServer { get; } = "?/? Players";

    public static AwaitableCollection<BLRServer> ServersToPing { get; } = new();

    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    #region Overrides
    public override bool Equals(object? obj)
    {
        if (obj is BLRServer server)
        {
            return ID.Equals(server.ID, StringComparison.Ordinal);
        }
        else
        { return false; }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    #endregion Overrides

    [JsonIgnore] public UIBool IsOnline { get { return new(ServerInfo?.IsOnline ?? false); } }
    [JsonIgnore] public bool IsDefaultServer { get { return Equals(DataStorage.Settings.DefaultServer); } set { IsNotDefaultServer = value; OnPropertyChanged(); } }
    [JsonIgnore] public bool IsNotDefaultServer { get { return !IsDefaultServer; } set { OnPropertyChanged(); } }
    [JsonIgnore] public UIBool IsPinging { get; } = new(false);
    [JsonIgnore] public ServerUtilsInfo ServerInfo { get; private set; } = new();
    [JsonIgnore] public UIBool IsTeammode { get { if (ServerInfo?.IsOnline ?? false) { return new(ServerInfo?.TeamList.Count >= 2); } else { return new(false); } } }
    [JsonIgnore] public string ServerDescription { get { return GetServerDescription(); } }
    [JsonIgnore] public BitmapImage MapImage { get { if ((ServerInfo?.IsOnline ?? false) && (ServerInfo.BLRMap?.SquareImage is not null)) { return new(new Uri(ServerInfo?.BLRMap?.SquareImage)); } else { return new(new Uri($"{IOResources.BaseDirectory}Assets\\textures\\t_bluescreen2.png")); } } }
    [JsonIgnore] public StringCollection PlayerList { get { if (ServerInfo?.IsOnline ?? false) { return ServerInfo.List; } else { return [EmptyServer]; } } }
    [JsonIgnore] public StringCollection Team1List { get { if (ServerInfo?.IsOnline ?? false) { return ServerInfo?.Team1List ?? [EmptyServer]; } else { return [EmptyServer]; } } }
    [JsonIgnore] public StringCollection Team2List { get { if (ServerInfo?.IsOnline ?? false) { return ServerInfo?.Team2List ?? [EmptyServer]; } else { return [EmptyServer]; } } }
    [JsonIgnore] public int PlayerCount { get { if (ServerInfo?.IsOnline ?? false) { return ServerInfo.PlayerCount; } else { return -1; } } }
    [JsonIgnore] public int BotCount { get { if (ServerInfo?.IsOnline ?? false) { return ServerInfo.BotCount; } else { return -1; } } }
    [JsonIgnore] public UIBool HasBots { get; } = new(false);
    [JsonIgnore] public int MaxPlayers { get { if (ServerInfo?.IsOnline ?? false) { return ServerInfo.MaxPlayers; } else { return -1; } } }

    [JsonIgnore] private string id = string.Empty;
    public string ID { get { return id; } set { id = value; OnPropertyChanged(); } }
    [JsonIgnore] private string serverAddress = "localhost";
    public string ServerAddress { get { return serverAddress; } set { serverAddress = value; OnPropertyChanged(); } }
    [JsonIgnore] private ushort port = 7777;
    public ushort Port { get { return port; } set { port = value; OnPropertyChanged(); } }
    [JsonIgnore] private ushort infoPort = 7777;
    public ushort InfoPort { get { return infoPort; } set { infoPort = value; OnPropertyChanged(); } }
    [JsonIgnore] private bool hidden = false;
    public bool Hidden { get { return hidden; } set { hidden = value; OnPropertyChanged(); } }
    [JsonIgnore] private string region = string.Empty;
    public string Region { get { return region; } set { region = value; OnPropertyChanged(); } }
    [JsonIgnore] private bool validatesLoadout = true;
    public bool ValidatesLoadout { get { return validatesLoadout; } set { validatesLoadout = value; OnPropertyChanged(); } }


    static BLRServer()
    {
        LoggingSystem.Log("Starting Ping Workers!");
        for (int i = Environment.ProcessorCount; i > 0; i--)
        {
            var thread = new Thread(PingWorker)
            { Name = $"Ping Thread[{i}]", IsBackground = true };
            App.AppThreads.Add(thread);
            thread.Start();
        }
        LoggingSystem.Log("Finished Starting Ping Workers!");
    }

    static void PingWorker()
    {
        while (App.IsRunning)
        {
            ServersToPing.WaitForFill();
            var server = ServersToPing.Take();
            if (server is null || string.IsNullOrEmpty(server.ServerAddress) || string.IsNullOrEmpty(server.IPAddress)) continue;
            if (DataStorage.Settings.PingHiddenServers.IsNot) { if (server.Hidden) { LoggingSystem.Log($"Skipping Hidden Server:{server}"); continue; } }
            server.IsPinging.Set(true);
            server.InternalPing();
            server.IsPinging.Set(false);
        }
    }

    [JsonIgnore]
    public string? IPAddress
    {
        get
        {
            try
            {
                IPAddress ip;
                if (System.Net.IPAddress.TryParse(ServerAddress, out ip))
                {
                    return ip; // Use directly
                }
                else
                {
#if NET6_0_OR_GREATER
                    return Dns.GetHostEntry(ServerAddress, System.Net.Sockets.AddressFamily.InterNetwork).ToString();
#else
                    ip = Dns.GetHostEntry(ServerAddress);
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
                }
#endif
            }
            catch (Exception error)
            {
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
            desc = $"{ServerInfo.ServerName}\n{ServerInfo.GetTimeDisplay()}\nMVP: {ServerInfo.GetScoreDisplay()}\n{ServerInfo.GameModeFullName}/{ServerInfo.Playlist}\n{ServerInfo?.BLRMap?.DisplayName ?? ServerInfo?.Map}";
        }
        else
        {
            desc = $"{ServerAddress}\n{ServerInfo?.BLRMap?.DisplayName ?? ServerInfo?.Map}";
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
        if (BotCount > 0)
        {
            HasBots.Set(true);
        }
        else
        {
            HasBots.Set(false);
        }

        MainWindow.Instance?.Dispatcher.Invoke(MainWindow.Instance.RefreshServerList);
    }

    public void PingServer()
    {
        ServersToPing.Add(this);
    }

    private void InternalPing()
    {
        var server = ServerUtilsClient.GetServerInfo(this);

        Task.WaitAll(server);

        var serverInfo = server.Result;

        if (serverInfo is null) { ServerInfo = new(); } else { serverInfo.IsOnline = true; ServerInfo = serverInfo; }

        RefreshInfo();
    }

    private void EditServer()
    {
        var window = new BLRServerWindow(this);
        window.ShowDialog();
        RefreshInfo();
    }

    private ICommand? editServerCommand;
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

    private ICommand? refreshPingCommand;
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

    private ICommand? connectToServerCommand;
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

    private ICommand? removeServer;
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
        DataStorage.ServerList.Remove(this);
    }

    public void LaunchClient()
    {
        if (DataStorage.Settings.DefaultClient is not null)
        { DataStorage.Settings?.DefaultClient?.LaunchClient(new LaunchOptions() { UserName = DataStorage.Settings.PlayerName, Server = this }); }
    }
}
