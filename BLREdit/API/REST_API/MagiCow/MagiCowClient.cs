using BLREdit.API.REST_API.GitHub;
using BLREdit.Game.Proxy;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLREdit.API.REST_API.MagiCow;
public sealed class MagiCowClient
{
    public static async Task<MagiCowServerInfo?> GetServerInfo(string server)
    {
        string serverAddress = $"http://{server}";
        string api = "/api/server";
        MagiCowServerInfo? info = null;
        string fail = "";
        try
        {
            using var response = await HttpGetClient.GetAsync(serverAddress, api, $"http://{server}").ConfigureAwait(false);
            if (response is not null && response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (content.StartsWith("{", StringComparison.InvariantCulture) || content.StartsWith("[", StringComparison.InvariantCulture))
                { info = IOResources.Deserialize<MagiCowServerInfo>(content); }
                else 
                { fail = "Not a valid Json!"; }
            }
        }
        catch(Exception error) { LoggingSystem.Log($"[Http]({serverAddress}{api}): {error}\n{fail}"); }
        return info;
    }
}