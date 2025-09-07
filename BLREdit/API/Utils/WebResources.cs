using System;
using System.Net.Http;
using System.Net;
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
        ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
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

    private static BlockingCollection<DownloadRequest> DownloadRequests { get; } = [];
    public static bool DownloadFile(string url, string filename)
    {
        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(filename)) { LoggingSystem.Log($"Failed to download ({filename}) from:\n<{url}>"); return false; }
        LoggingSystem.Log($"Downloading ({filename}) from ({url})");
        DownloadRequest req = new(url, filename);
        DownloadRequests.Add(req);
        WaitHandle.WaitAll([req.locked]);
        req.Dispose();
        if (req.Error is not null)
        {
            LoggingSystem.MessageLog($"Failed to download ({filename}) from:\n<{url}>\n{req.Error.Message}", "Error");
            return false;
        }
        LoggingSystem.Log($"Finished downloading ({filename}) from:\n<{url}>");
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
                request.Error = error;
                LoggingSystem.Log($"[WebClient]: Failed to download ({request.filename}) from:\n<{request.url}>\nReason:{error}");
            }
            finally
            { request.locked.Set(); }
        }
    }
}

sealed class DownloadRequest(string url, string filename) : IDisposable
{
    public string url = url, filename = filename;
    public Exception? Error { get; set; }
    public AutoResetEvent locked = new(false);

    public void Dispose()
    {
        locked.Dispose();
    }
}
