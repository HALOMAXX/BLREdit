using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;
using BLREdit.Game.Proxy;

using Gameloop.Vdf.Linq;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace BLREdit.API.REST_API;

public sealed class RESTAPIClient
{
    public const string CACHE = "Cache\\";
    public const string DOWNLOAD = "downloads\\";

    private string CacheFile { get; }

    readonly Dictionary<string, object> RequestCache = [];
    readonly Dictionary<string, object> OldRequestCache;

    readonly string baseAddress;
    readonly RepositoryProvider APIProvider;

    public RESTAPIClient(RepositoryProvider APIProvider, string baseAddress)
    {
        this.baseAddress = baseAddress;
        this.APIProvider = APIProvider;
        CacheFile = $"{CACHE}{IOResources.DataToBase64(IOResources.Zip($"{APIProvider}\\{baseAddress}"))}.json";
        OldRequestCache = IOResources.DeserializeFile<Dictionary<string, object>>(CacheFile) ?? [];
        DataStorage.DataSaving += SaveCache;
    }

    private void SafeCacheAddOrUpdate(string api, object data, bool old = false)
    {
        var cache = old ? OldRequestCache : RequestCache;
        try 
        {
            if (cache.ContainsKey(api))
            {
                cache[api] = data;
            }
            else
            {
                cache.Add(api, data);
            }
        }
        catch (Exception error)
        {
            LoggingSystem.Log($"[Cache]({(old ? "Old" : "New")}): failed to add or update [{api}]\n[Cache]ErrorMessage: {error.Message}\n[Cache]Stacktrace:{error.StackTrace}");
        }
    }

    public async Task<(bool, T?)> TryGetAPI<T>(string api)
    {
        if (RequestCache.TryGetValue(api, out var newCache))
        {
            LoggingSystem.Log($"[Cache]({typeof(T).Name}): {api}");
            return (true, (T)newCache);
        }

        using var response = await GetAsync(api).ConfigureAwait(false);
        if (response is not null && response.IsSuccessStatusCode)
        {
            switch (response.Content.Headers.ContentType.MediaType)
            {
                case "application/json":
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var value = IOResources.Deserialize<T>(content);
                    if (value is not null)
                    {
                        SafeCacheAddOrUpdate(api, value);
                        return (true, value); 
                    }
                    break;
                default:
                    LoggingSystem.Log($"Wrong HeaderType: {response.Content.Headers.ContentType.MediaType}");
                    break;
            }
        }

        if (OldRequestCache.TryGetValue(api, out var oldCache))
        {
            LoggingSystem.Log($"[OldCache]({typeof(T).Name}): {api}");
            return (true, (T)oldCache);
        }
        return (false, default);
    }

    public async Task<(bool, (byte[], DateTime))> TryGetBytes(string api)
    {
        if (RequestCache.TryGetValue(api, out var newCache))
        {
            LoggingSystem.Log($"[Cache](byte[]): {api}");
            return (true, ((byte[], DateTime))newCache);
        }

        using var response = await GetAsync(api).ConfigureAwait(false);
        if (response is not null && response.IsSuccessStatusCode)
        {
            switch (response.Content.Headers.ContentType.MediaType)
            {
                case "application/octet-stream":
                    var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    var date = response.Content.Headers.LastModified?.DateTime ?? DateTime.MinValue;
                    if (bytes is not null) 
                    { 
                        SafeCacheAddOrUpdate(api, (bytes, date));
                        return (true, (bytes, date));
                    }
                    break;
                default:
                    LoggingSystem.Log($"Wrong HeaderType: {response.Content.Headers.ContentType.MediaType}");
                    break;
            }
        }

        if (OldRequestCache.TryGetValue(api, out var oldCache))
        {
            LoggingSystem.Log($"[OldCache](byte[]): {api}");
            return (true, ((byte[], DateTime))oldCache);
        }
        return (false, (Array.Empty<byte>(), DateTime.MinValue));
    }

    private void SaveCache(object? sender, EventArgs args)
    {
        foreach (var cache in RequestCache)
        {
            SafeCacheAddOrUpdate(cache.Key, cache.Value, true);
        }
        IOResources.SerializeFile(CacheFile, OldRequestCache);
    }

    private async Task<HttpResponseMessage?> GetAsync(string api)
    {
        try
        {
            var response = await WebResources.HttpClient.GetAsync($"{baseAddress}{api}").ConfigureAwait(false);
            string fail = "";
            if (!response.IsSuccessStatusCode) { fail = $"\n {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}"; }
            LoggingSystem.Log($"[{APIProvider}]({response.StatusCode}): GET {api}{fail}");
            return response;
        }
        catch (Exception error)
        {
            LoggingSystem.Log($"[{APIProvider}]({error.GetType().Name}): GET {api}\n{error}");
            return null;
        }
    }

    public async Task<T?> GetLatestRelease<T>(string owner, string repository)
    {
        var releases = await GetReleases<T>(owner, repository, 1, 1).ConfigureAwait(false);
        if (releases is null || releases.Length <= 0) return default;
        return releases[0];
    }
    public async Task<T[]?> GetReleases<T>(string owner, string repository, int per_page = 10, int page = 1)
    {
        if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repository)) { LoggingSystem.FatalLog("owner or repository were null!"); return default; }
        string api;
        if (APIProvider == RepositoryProvider.GitHub)
        { api = $"repos/{owner}/{repository}/releases?page={page}&per_page={per_page}"; }
        else
        { api = $"projects/{owner.Replace("/", "%2F")}%2F{repository.Replace("/", "%2F")}/releases?page={page}&per_page={per_page}"; }

        var result = await TryGetAPI<T[]>(api).ConfigureAwait(false);

        if (result.Item1)
        {
            if (result.Item2 is GitlabRelease[] glRel)
            {
                foreach (var rel in glRel)
                {
                    rel.Owner = owner;
                    rel.Repository = repository;
                }
            }
            if (result.Item2 is GitHubRelease[] ghRel)
            {
                foreach (var rel in ghRel)
                {
                    rel.Owner = owner;
                    rel.Repository = repository;
                }
            }
            return result.Item2;
        }
        return default;
    }

    public async Task<T?> GetFile<T>(string owner, string repository, string branch, string file)
    {
        if(string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repository) || string.IsNullOrEmpty(file)) { LoggingSystem.FatalLog("owner, repository or file were null!"); return default; }
        string api;
        if (APIProvider == RepositoryProvider.GitHub)
        { api = $"repos/{owner}/{repository}/contents/{file}?ref={branch}"; }
        else
        { api = $"projects/{owner.Replace("/", "%2F")}%2F{repository.Replace("/", "%2F")}/repository/files/{file.Replace("/", "%2F").Replace(".", "%2E")}?ref={branch}"; }

        var result = await TryGetAPI<T>(api).ConfigureAwait(false);
        if (result.Item1)
        {
            return result.Item2;
        }
        return default;
    }
}
