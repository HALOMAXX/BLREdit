using BLREdit.API.REST_API.MagiCow;
using BLREdit.API.REST_API.Server;
using BLREdit.Export;
using BLREdit.UI;
using BLREdit.UI.Windows;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BLREdit.Game;

public sealed class BLRServer : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    [JsonIgnore] public bool IsDefaultServer { get { return Equals(BLREditSettings.Settings.DefaultServer); } set { IsNotDefaultServer = value; OnPropertyChanged(); } }
    [JsonIgnore] public bool IsNotDefaultServer { get { return !IsDefaultServer; } set { OnPropertyChanged(); } }
    [JsonIgnore] public string PingDisplay { get { if (IsOnline) { return "Online"; } else { return "Offline"; } } }
    [JsonIgnore] public bool IsOnline { get { return ((ServerInfo?.IsOnline ?? false) || (MagiInfo?.IsOnline ?? false)); } }

    private readonly UIBool isPinging = new(false);
    [JsonIgnore] public UIBool IsPinging { get { return isPinging; } }

    [JsonIgnore] public MagiCowServerInfo MagiInfo { get; private set; } = new();
    [JsonIgnore] public ServerUtilsInfo ServerInfo { get; private set; } = new();

    [JsonIgnore] public string DisplayName { get { if (!(ServerInfo?.IsOnline ?? false)) { return ServerAddress; } else { if (!string.IsNullOrEmpty(ServerInfo.ServerName)) { return ServerInfo.ServerName; } else { return ServerAddress; } } } }


    [JsonIgnore] private string serverName;
    public string ServerName { get { return serverName; } set { serverName = value; OnPropertyChanged(); } }
    public string ServerAddress { get; set; } = "localhost";
    [JsonIgnore] private ushort port = 7777;
    public ushort Port { get { return port; } set { port = value; OnPropertyChanged(); } }
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

    public void RefreshInfo()
    {
        OnPropertyChanged(nameof(MagiInfo));

        OnPropertyChanged(nameof(ServerInfo));
        OnPropertyChanged(nameof(ServerInfo.TeamList));

        OnPropertyChanged(nameof(IsOnline));
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(PingDisplay));
    }

    public override bool Equals(object obj)
    {
        if (obj is BLRServer server)
        { return server.ServerAddress == ServerAddress && server.Port == Port; }
        else
        { return false; }
    }

    public void PingServer()
    {
        if (IPAddress is null) { return; }
        isPinging.SetBool(true);
        Thread pingThread = new(new ThreadStart(InternalPing))
        {
            Name = ServerAddress + " Ping",
            Priority = ThreadPriority.Highest
        };
        pingThread.Start();
    }

    private void InternalPing()
    {
        var server = ServerUtilsClient.GetServerInfo(this);
        server.Wait();
        ServerInfo = server.Result;
        if (ServerInfo is null) { ServerInfo = new(); } else { ServerInfo.IsOnline = true; LoggingSystem.Log($"[Server]({ServerAddress}): got Server Info!"); }



        var magi = MagiCowClient.GetServerInfo(ServerAddress);
        magi.Wait();
        MagiInfo = magi.Result;
        if (MagiInfo is null) { MagiInfo = new(); } else { MagiInfo.IsOnline = true; LoggingSystem.Log($"[Server]({ServerAddress}): got Magi Info!"); }

        RefreshInfo();
        isPinging.SetBool(false);
    }

    private void EditServer()
    {
        var window = new BLRServerWindow(this);
        window.ShowDialog();
        OnPropertyChanged(nameof(DisplayName));
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

    private ICommand launchServerCommand;
    [JsonIgnore]
    public ICommand LaunchServerCommand
    {
        get
        {
            launchServerCommand ??= new RelayCommand(
                    param => this.LaunchClient()
                );
            return launchServerCommand;
        }
    }

    public void LaunchClient()
    {
        if (BLREditSettings.Settings.DefaultClient is not null)
        { BLREditSettings.Settings?.DefaultClient?.LaunchClient(new LaunchOptions() { UserName = ExportSystem.ActiveProfile.PlayerName, Server = this }); }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}