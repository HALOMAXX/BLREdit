using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.API.REST_API;

public sealed class HttpGetClient
{
    public static async Task<HttpResponseMessage?> GetAsync(string server, string api, string? serverName = null)
    {
        Uri uri = new($"{server}{api}");
        Uri logUri = new($"{(serverName is not null ? serverName : server)}{api}");
        try
        {
            var response = await WebResources.HttpClient.GetAsync(uri);
            LoggingSystem.Log($"[Http]({response.StatusCode}): GET {logUri}");
            return response;
        }
        catch (HttpRequestException requestError) {
            if (requestError.InnerException.HResult == -2146233079)
            {
                LoggingSystem.Log($"[Http]({logUri}): Unable to connect to server!");
            }
            else
            {
                LoggingSystem.Log($"[Http]({logUri}): {requestError}");
            }
            
        }
        catch (TaskCanceledException) { LoggingSystem.Log($"[Http]({logUri}): GET Request canceled / timedout !"); }
        catch (Exception error) { LoggingSystem.Log($"[Http]({logUri}): {error}"); }
        return null;
    }
}
