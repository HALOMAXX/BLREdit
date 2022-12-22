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
    private static async Task<HttpResponseMessage> GetAsync(string server, string api)
    {
        try
        {
            Uri uri = new($"{server}{api}");
            var response = await IOResources.HttpClient.GetAsync(uri);
            string fail = "";
            if (!response.IsSuccessStatusCode) { fail = $"\n {await response.Content.ReadAsStringAsync()}"; }
            LoggingSystem.Log($"[Server]({response.StatusCode}): GET {server}{api}{fail}");
            return response;
        }
        catch (Exception error) { LoggingSystem.Log($"[Server]({server}{api}): {error}"); }
        return null;
    }

    public static async Task<ServerUtilsInfo> GetServerInfo(BLRServer server)
    {
        ServerUtilsInfo info = null;
        string fail = "";
        try
        {
            using var response = await GetAsync($"http://{server.ServerAddress}:{server.InfoPort}", "/server_info");
            if (response is not null && response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content.StartsWith("{") || content.StartsWith("["))
                { info = IOResources.Deserialize<ServerUtilsInfo>(content); info.IsOnline = true; }
                else
                { fail = "Not a valid Json!"; }
            }
        }
        catch (Exception error) { LoggingSystem.Log($"[Server]({server.ServerAddress}): {error}\n{fail}"); }
        return info;
    }
}
