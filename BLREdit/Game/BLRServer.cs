using BLREdit.Export;

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

    [JsonIgnore] private string serverName;
    public string ServerName { get { return serverName; } set { serverName = value; OnPropertyChanged(); } }
    public string ServerAddress { get; set; } = "localhost";
    public short Port { get; set; } = 7777;
    private string ipAddress;
    [JsonIgnore]
    public string IPAddress
    {
        get
        {
            if (ipAddress is null)
            {
                try
                {
                    var ip = Dns.GetHostEntry(ServerAddress);
                    foreach (IPAddress address in ip.AddressList)
                    {
                        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            ipAddress = address.ToString();
                        }
                    }
                }catch { }
            }
            return ipAddress;
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
        LoggingSystem.Log("Finished Ping for [" + ServerAddress + "]:" + Ping + "ms");
        socket.Close();
        socket.Dispose();
    }

    private void PingCompletedCallback(object sender, PingCompletedEventArgs e)
    {
        // If the operation was canceled, display a message to the user.
        if (e.Cancelled)
        {
            // Let the main thread resume.
            // UserToken is the AutoResetEvent object that the main thread
            // is waiting for.
            ((AutoResetEvent)e.UserState).Set();
        }

        // If an error occurred, display the exception to the user.
        if (e.Error != null)
        {
            LoggingSystem.Log(e.Error.Message);
            // Let the main thread resume.
            ((AutoResetEvent)e.UserState).Set();
        }

        PingReply reply = e.Reply;

        DisplayReply(reply);

        // Let the main thread resume.
        ((AutoResetEvent)e.UserState).Set();
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