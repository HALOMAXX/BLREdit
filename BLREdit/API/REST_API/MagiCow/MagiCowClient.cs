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
        catch(Exception error) { LoggingSystem.Log($"[Server]({server}{api}): {error}"); }
        return null;
    }

    public static async Task<MagiCowServerInfo> GetServerInfo(string server)
    {
        MagiCowServerInfo info = null;
        string fail = "";
        try
        {
            using var response = await GetAsync($"http://{server}", "/api/server");
            if (response is not null && response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (content.StartsWith("{") || content.StartsWith("["))
                { info = IOResources.Deserialize<MagiCowServerInfo>(content); }
                else 
                { fail = "Not a valid Json!"; }
            }
        }
        catch(Exception error) { LoggingSystem.Log($"[Server]({server}): {error}"); }
        if (info is null) { LoggingSystem.Log($"[Server]({server}): failed to get Server Info! {fail}"); }
        return info;
    }
}