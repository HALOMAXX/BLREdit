using BLREdit.API.REST_API.MagiCow;
using BLREdit.Export;
using BLREdit.UI;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BLREdit.Game;

public sealed class BLRServer : INotifyPropertyChanged
{
    private double ping = double.NaN;
    private bool isOnline = false;
    [JsonIgnore] public bool IsDefaultServer { get { return Equals(BLREditSettings.Settings.DefaultServer); } set { IsNotDefaultServer = value; OnPropertyChanged(); } }
    [JsonIgnore] public bool IsNotDefaultServer { get { return !IsDefaultServer; } set { OnPropertyChanged(); } }
    [JsonIgnore] public double Ping { get { return ping; } private set { ping = value; PingDisplay = ""; OnPropertyChanged(); } }
    [JsonIgnore] public string PingDisplay { get { return ping.ToString() + "ms"; } private set { OnPropertyChanged(); } }
    [JsonIgnore] public bool IsOnline { get { return isOnline; } private set { isOnline = value; IsNotOnline = value; OnPropertyChanged(); } }
    [JsonIgnore] public bool IsNotOnline { get { return !isOnline; } private set { OnPropertyChanged(); } }

    private UIBool isPinging = new(false);
    [JsonIgnore] public UIBool IsPinging { get { return isPinging; } }

    [JsonIgnore] public MagiCowServerInfo Info { get; private set; }

    [JsonIgnore] public string DisplayName { get { if (Info is null) { return ServerAddress; } else { return Info.ServerName; } } }


    [JsonIgnore] private string serverName;
    public string ServerName { get { return serverName; } set { serverName = value; OnPropertyChanged(); } }
    public string ServerAddress { get; set; } = "localhost";
    public short Port { get; set; } = 7777;
    [JsonIgnore]
    public string IPAddress
    {
        get
        {
            try
            {
                var ip = Dns.GetHostEntry(ServerAddress);
                foreach (IPAddress address in ip.AddressList)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return address.ToString();
                    }
                }
            }
            catch { }
            return null;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

    private static readonly byte[] msg = Encoding.UTF8.GetBytes("BLREdit");
    private void InternalPing()
    {
        Stopwatch watch = new();
        IPEndPoint RemoteIpEndPoint = new(System.Net.IPAddress.Parse(IPAddress), Port);
        Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        int sent;
        byte[] array = new byte[16];
        var buffer = new ArraySegment<byte>(array);

        try
        {
            socket.SendTimeout = 2000;
            socket.ReceiveTimeout = 2000;
            socket.Connect(RemoteIpEndPoint);
            Task<SocketReceiveFromResult> recieve = socket.ReceiveFromAsync(buffer, SocketFlags.Partial, RemoteIpEndPoint);

            sent = socket.SendTo(msg, msg.Length, SocketFlags.None, RemoteIpEndPoint);
            watch.Start();
            recieve.Wait(2000);
            watch.Stop();

            Ping = watch.ElapsedMilliseconds;
            if (recieve.IsCompleted && !recieve.IsCanceled && !recieve.IsFaulted)
            {
                if (sent >= 1 && recieve.Result.ReceivedBytes >= 2)
                {
                    IsOnline = true;
                }
                else
                {
                    IsOnline = false;
                }
            }
            else
            {
                Ping = double.NaN;
            }
        }
        catch (ObjectDisposedException error)
        {
            LoggingSystem.Log("{ObjectDisposedException}" + error.Message + "\n" + error.StackTrace);
            IsOnline = false;
            Ping = double.NaN;
        }
        catch (ArgumentOutOfRangeException error)
        {
            LoggingSystem.Log("{ArgumentOutOfRangeException}" + error.Message + "\n" + error.StackTrace);
            IsOnline = false;
            Ping = double.NaN;
        }
        catch (AggregateException error)
        {
            LoggingSystem.Log("{AggregateException}:");
            foreach (var ex in error.InnerExceptions)
            {
                LoggingSystem.Log(ex.Message + "\n" + ex.StackTrace + "\n");
            }
            IsOnline = false;
            Ping = double.NaN;
        }
        LoggingSystem.Log($"Finished Ping for [{ServerAddress}]:{Ping}ms");
        socket.Close();
        socket.Dispose();

        LoggingSystem.Log($"Getting Server Info for [{ServerAddress}]");
        var task = MagiCowClient.GetServerInfo(ServerAddress);
        task.Wait();
        Info = task.Result;
        if (Info is null) { Info = new(); }
        OnPropertyChanged(nameof(Info));
        isPinging.SetBool(false);
        LoggingSystem.Log($"got Server Info for [{ServerAddress}]");
    }

    public void DisplayReply(PingReply reply)
    {
        if (reply == null)
        { LoggingSystem.Log("No Reply Recieved"); return; }

        if (reply.Status == IPStatus.Success)
        {
            IsOnline = true;
            Ping = reply.RoundtripTime / 1000.0D;
            LoggingSystem.Log("[" + ServerAddress + "]:" + reply.Status + " (" + PingDisplay + ")");
        }
        else
        {
            LoggingSystem.Log("[" + ServerAddress + "]:" + reply.Status);
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
        if (BLREditSettings.Settings.DefaultClient is null)
        {
            //No Default Client selected
        }
        else
        {
            BLREditSettings.Settings?.DefaultClient?.LaunchClient(new LaunchOptions() { UserName = ExportSystem.ActiveProfile.PlayerName, Server = this });
        }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}