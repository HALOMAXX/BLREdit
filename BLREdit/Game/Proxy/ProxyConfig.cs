using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.Game.Proxy;

public sealed class ProxyConfig
{
    public ProxyConfigContainer Proxy { get; set; } = new();
}

public sealed class ProxyConfigContainer
{
    public ServerConfig Server { get; set; } = new();
    public ModulesConfig Modules { get; set; } = new();
}

public sealed class ModulesConfig
{
    public List<string> Server = [];
    public List<string> Client = [];
}

public sealed class ServerConfig
{
    public string Host { get; set; } = "0.0.0.0";
    public string Port { get; set; } = "+1";
}
