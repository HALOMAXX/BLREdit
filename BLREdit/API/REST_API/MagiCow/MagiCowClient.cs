using BLREdit.API.REST_API.GitHub;
using BLREdit.Game.Proxy;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.API.REST_API.MagiCow;
public sealed class MagiCowClient
{
    private static async Task<HttpResponseMessage> GetAsync(string server, string api)
    {
        Uri uri = null;
        try
        {
            uri = new Uri($"{server}{api}");
        }
        catch { LoggingSystem.Log($"[Server]({server}{api}): not a valid uri"); return null; }
        var response = await IOResources.HttpClient.GetAsync(uri);
        string fail = "";
        if (!response.IsSuccessStatusCode) { fail = $"\n {await response.Content.ReadAsStringAsync()}"; }
        LoggingSystem.Log($"[Server]({response.StatusCode}): GET {api}{fail}");
        return response;
    }

    public static async Task<MagiCowServerInfo> GetServerInfo(string server)
    {
        try
        {
            using (var response = await GetAsync($"http://{server}", "/api/server"))
            {
                if (response is not null && response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var releases = IOResources.Deserialize<MagiCowServerInfo>(content);
                    return releases;
                }
            }
        }
        catch { LoggingSystem.Log("failed to get server Info!"); }
        return null;
    }
}