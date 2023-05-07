using BLREdit.API.REST_API.MagiCow;
using BLREdit.API.REST_API.Server;
using BLREdit.Game;
using BLREdit.UI;
using BLREdit.UI.Windows;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BLREdit.Model.BLR;

public sealed class BLRServerModel : INotifyPropertyChanged
{
    public static RangeObservableCollection<BLRServerModel> Servers { get; } = IOResources.DeserializeFile<RangeObservableCollection<BLRServerModel>>($"Servers.json") ?? new();

    #region Event
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Event

    #region Overrides
    public override bool Equals(object obj)
    {
        if(obj is BLRServerModel model)
        {
            return model.GetHashCode() == GetHashCode();
        }
        return false;
    }
    public override string ToString()
    {
        return $"{ServerAddress}:{ServerPort}({InfoPort})";
    }
    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }
    #endregion Overrides

    private string _serverAddress = "localhost";
    private string? _serverIP;
    private ushort _serverPort = 7777;
    private ushort _infoPort = 7778;

    [JsonIgnore] public string ServerIP { get { _serverIP ??= QueryServerIPAddress(); return _serverIP; } private set { _serverIP = value; OnPropertyChanged(); } }
    public string ServerAddress { get { return _serverAddress; } set { _serverAddress = value; OnPropertyChanged(); } }
    public ushort ServerPort { get { return _serverPort; } set { _serverPort = value; OnPropertyChanged(); } }
    public ushort InfoPort { get { return _infoPort; } set { _infoPort = value; OnPropertyChanged(); } }

    [JsonIgnore] public UIBool IsPinging { get; } = new(false);
    [JsonIgnore] public BLRServerStatus ServerStatus { get; }

    public BLRServerModel()
    { 
        ServerStatus = new BLRServerStatus(this);
    }

    public string QueryServerIPAddress()
    {
        try
        {
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
        }
        catch (Exception error)
        {
            LoggingSystem.Log($"Failed to get IPAddress for {ServerAddress}\n{error}");
        }
        return "";
    }

    public void PingServer()
    {
        if (IsPinging.Is) return;
        IsPinging.Set(true);
        try
        {
            ServerIP = QueryServerIPAddress();
            string address = $"{ServerIP}:{InfoPort}";
            var utils = ServerUtilsClient.GetServerInfo(address);
            var magi = MagiCowClient.GetServerInfo(ServerAddress);
            Task.WaitAll(magi, utils);
            var magiInfo = magi.Result;
            var utilsInfo = utils.Result;
            ServerStatus.ApplyInfo(utilsInfo);
            ServerStatus.ApplyInfo(magiInfo);
        }
        catch { throw; }
        finally { IsPinging.Set(false); }
    }

    private ICommand? _removeServer;
    private ICommand? _modifyServer;
    private ICommand? _connectToServer;
    private ICommand? _pingServer;

    [JsonIgnore] public ICommand RemoveServerCommand { get { _removeServer ??= new RelayCommand(param => { Servers.Remove(this); }); return _removeServer; } }
    [JsonIgnore] public ICommand ModifyServerCommand { get { _modifyServer ??= new RelayCommand(param => { var window = new BLRServerWindow(this); window.ShowDialog(); }); return _modifyServer; } }
    [JsonIgnore] public ICommand ConnectToServerCommand { get { _connectToServer ??= new RelayCommand(param => { BLRClientModel.StartClientWithArgs(BLREditSettings.Settings.DefaultClient, $"{ServerIP}:{ServerPort}?Name={BLREditSettings.Settings.PlayerName}"); }); return _connectToServer; } }
    [JsonIgnore] public ICommand PingServerCommand { get { _pingServer ??= new RelayCommand(param => { Task.Run(PingServer); }); return _pingServer; } }
}