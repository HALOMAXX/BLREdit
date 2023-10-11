using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace BLREdit;

public sealed class WebResources
{
    public static WebClient WebClient { get; } = new WebClient();
    public static HttpClient HttpClient { get; } = new HttpClient() { Timeout = new TimeSpan(0, 0, 10) };
    public static HttpClient HttpClientWeb { get; } = new HttpClient() { Timeout = new TimeSpan(0, 0, 10) };

    static WebResources()
    {
        LoggingSystem.Log($"[{nameof(WebResources)}]: Initializing!");
        System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        WebClient.Headers.Add(HttpRequestHeader.UserAgent, $"BLREdit-{App.CurrentVersion}");
        if (!HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd($"BLREdit-{App.CurrentVersion}")) { LoggingSystem.Log($"Failed to add {HttpRequestHeader.UserAgent} to HttpClient"); };
        HttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        if (!HttpClientWeb.DefaultRequestHeaders.UserAgent.TryParseAdd($"BLREdit-{App.CurrentVersion}")) { LoggingSystem.Log($"Failed to add {HttpRequestHeader.UserAgent} to HttpClientWeb"); };
        HttpClientWeb.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));
        Thread downloadClient = new(DownloadFiles);
        App.AppThreads.Add(downloadClient);
        downloadClient.Start();
        LoggingSystem.Log($"[{nameof(WebResources)}]: Finished Initializing!");
    }

    private static BlockingCollection<DownloadRequest> DownloadRequests { get; } = new();
    public static bool DownloadFile(string url, string filename)
    {
        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(filename)) { LoggingSystem.Log($"Failed to download file({filename}) from url({url})!"); return false; }
        DownloadRequest req = new(url, filename);
        DownloadRequests.Add(req);
        WaitHandle.WaitAll(new WaitHandle[] { req.locked });
        return true;
    }

    private static void DownloadFiles()
    {
        while (App.IsRunning)
        {
            var request = DownloadRequests.Take();
            try
            {
                WebClient.DownloadFile(request.url, request.filename);
            }
            catch (Exception error)
            {
                LoggingSystem.Log($"[WebClient]Failed to Download({request.url})\nReason:{error}");
            }
            finally
            { request.locked.Set(); }
        }
    }
}

class DownloadRequest(string url, string filename)
{
    public string url = url, filename = filename;
    public AutoResetEvent locked = new(false);
}
