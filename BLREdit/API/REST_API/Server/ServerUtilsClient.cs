using BLREdit.API.REST_API.MagiCow;
using BLREdit.Game;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.API.REST_API.Server;

public sealed class ServerUtilsClient
{


    public static async Task<ServerUtilsInfo?> GetServerInfo(BLRServer server)
    {
        string serverAddress = $"http://{server.ServerAddress}:{server.InfoPort}";
        string api = "/server_info";
        ServerUtilsInfo? info = null;
        string fail = "";
        try
        {
            using var response = await HttpGetClient.GetAsync(serverAddress, api);
            if (response is not null && response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content.StartsWith("{"))
                {
                    info = IOResources.Deserialize<ServerUtilsInfo>(content);
                }
                else if (content.StartsWith("["))
                {
                    var infos = IOResources.Deserialize<ServerUtilsInfo[]>(content);
                    if (infos is not null && infos.Length > 0)
                    {
                        info = infos[0];
                    }
                }
                else
                { fail = "Invalid Json!"; }
            }
        }
        catch (Exception error) { LoggingSystem.Log($"[Http]({serverAddress}{api}): {error}\n{fail}"); }
        if (info is not null) { info.IsOnline = true; }
        return info;
    }
}
