using BLREdit.Core.API.REST.ServerUtils;
using BLREdit.Core.Models.BLR.Client;
using BLREdit.Core.Utils;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Text.Json.Serialization;

namespace BLREdit.Core.Models.BLR.Server;

public sealed class BLRServer : ModelBase
{
    public static DirectoryInfo ServerListLocation { get; }
    public static RangeObservableCollection<BLRServer> Servers { get; } = new();

    static BLRServer()
    {
        ServerListLocation = new DirectoryInfo("Data\\Servers\\List");
        IOResources.DeserializeDirectoryInto(Servers, ServerListLocation);
    }

    public static void Save()
    {
        Debug.WriteLine($"Saving Clients:{Servers.Count}");
        IOResources.SerializeFile($"{ServerListLocation.FullName}\\List.json", Servers);
    }

    #region Overrides
    public override bool Equals(object? obj)
    {
        if (obj is BLRServer model)
        {
            return
                ServerAddress.Equals(model.ServerAddress, StringComparison.Ordinal) &&
                ServerPort.Equals(model.ServerPort) &&
                InfoPort.Equals(model.InfoPort);
        }
        return false;
    }
    public override string ToString()
    {
        return $"{ServerAddress}:{ServerPort}({InfoPort})";
    }
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ServerAddress);
        hash.Add(ServerPort);
        hash.Add(InfoPort);
        return hash.ToHashCode();
    }
    #endregion Overrides

    private string? _serverIP;
    [JsonIgnore] public string ServerIP { get { _serverIP ??= QueryServerIPAddress(ServerAddress); return _serverIP ?? ""; } }
    public string ServerAddress { get; set; }
    public ushort ServerPort { get; set; } = 7777;
    public ushort InfoPort { get; set; } = 7778;
    [JsonIgnore] public ServerInfo ServerInfo { get; set; } = new();

    [JsonConstructor]
    public BLRServer(string serverAddress)
    {
        ServerAddress = serverAddress;
    }

    public static string? QueryServerIPAddress(string address)
    {
        if (string.IsNullOrEmpty(address)) return null;
        var hostEntry = Dns.GetHostEntry(address, System.Net.Sockets.AddressFamily.InterNetwork);
        if (hostEntry.AddressList.Length > 0)
        {
            return hostEntry.AddressList[0].ToString();
        }
        return address;
    }

    public ServerInfo QueryServerInfo()
    {
        return new();
    }
}

public sealed class JsonBLRServerConverter : JsonGenericConverter<BLRServer>
{
    static JsonBLRServerConverter()
    {
        Default = new BLRServer("");
    }
}