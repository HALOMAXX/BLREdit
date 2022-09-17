using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.Game;

public sealed class LaunchOptions
{
    public BLRServer Server { get; set; } = new BLRServer();
    public string UserName { get; set; } = "HKNI";
}
