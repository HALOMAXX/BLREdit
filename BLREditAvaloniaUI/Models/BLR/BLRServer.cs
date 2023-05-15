using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BLREdit.Models.BLR;

public sealed class BLRServer : ModelBase
{
    public static RangeObservableCollection<BLRServer> Servers { get; } = IOResources.DeserializeFile<RangeObservableCollection<BLRServer>>("Data\\ServerList.json") ?? new();

    #region Overrides
    public override bool Equals(object? obj)
    {
        if (obj is BLRServer model)
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

    private string? _serverIP;
    [JsonIgnore] public string ServerIP { get { _serverIP ??= QueryServerIPAddress(ServerAddress); return _serverIP; } }
    public string ServerAddress { get; set; }
    public ushort ServerPort { get; set; } = 7777;
    public ushort InfoPort { get; set; } = 7778;

    [JsonConstructor]
    public BLRServer(string serverAddress)
    { 
        ServerAddress = serverAddress;
    }

    public static string QueryServerIPAddress(string address)
    {
        var hostEntry = Dns.GetHostEntry(address, System.Net.Sockets.AddressFamily.InterNetwork);
        if (hostEntry.AddressList.Length > 0)
        { 
            return hostEntry.AddressList[0].ToString();
        }
        return address;
    }
}