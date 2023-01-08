using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.API.REST_API;

public sealed class HttpGetClient
{
    public static async Task<HttpResponseMessage> GetAsync(string server, string api)
    {
        try
        {
            Uri uri = new($"{server}{api}");
            var response = await IOResources.HttpClient.GetAsync(uri);
            LoggingSystem.Log($"[Http]({response.StatusCode}): GET {server}{api}");
            return response;
        }
        catch (HttpRequestException requestError) {
            if (requestError.InnerException.HResult == -2146233079)
            {
                LoggingSystem.Log($"[Http]({server}{api}): Unable to connect to server!");
            }
            else
            {
                LoggingSystem.Log($"[Http]({server}{api}): {requestError}");
            }
            
        }
        catch (TaskCanceledException _) { LoggingSystem.Log($"[Http]({server}{api}): GET Request canceled / timedout !"); }
        catch (Exception error) { LoggingSystem.Log($"[Http]({server}{api}): {error}"); }
        return null;
    }
}
