using BLREdit.Game.Proxy;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BLREdit.API.REST_API;

public sealed class RESTAPIClient
{
    private string CacheFile { get; }

    readonly Dictionary<string, object> RequestCache = new();
    readonly Dictionary<string, object> OldRequestCache;

    readonly string baseAddress;
    readonly RepositoryProvider APIProvider;

    public RESTAPIClient(RepositoryProvider APIProvider, string baseAddress)
    {
        this.baseAddress = baseAddress;
        this.APIProvider = APIProvider;
        CacheFile = $"Cache\\{IOResources.DataToBase64(IOResources.Zip($"{APIProvider}\\{baseAddress}"))}.json";
        OldRequestCache = IOResources.DeserializeFile<Dictionary<string, object>>(CacheFile) ?? new();
        DataStorage.DataSaving += SaveCache;
    }

    private async Task<(bool, T)> TryGetAPI<T>(string api)
    {
        if (RequestCache.TryGetValue(api, out var newCache))
        {
            LoggingSystem.Log($"[Cache]({typeof(T).Name}): {api}");
            return (true, (T)newCache);
        }

        using var response = await GetAsync(api);
        if (response is not null && response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var value = IOResources.Deserialize<T>(content);
            if (value is not null) { RequestCache.Add(api, value); return (true, value); }
        }

        if (OldRequestCache.TryGetValue(api, out var oldCache))
        {
            LoggingSystem.Log($"[OldCache]({typeof(T).Name}): {api}");
            return (true, (T)oldCache);
        }
        return (false, default);
    }

    private void SaveCache(object? sender, EventArgs args)
    {
        foreach (var cache in RequestCache)
        {
            if (OldRequestCache.ContainsKey(cache.Key))
            {
                OldRequestCache[cache.Key] = cache.Value;
            }
            else
            { 
                OldRequestCache.Add(cache.Key, cache.Value);
            }
        }
        IOResources.SerializeFile(CacheFile, OldRequestCache);
    }

    private async Task<HttpResponseMessage?> GetAsync(string api)
    {
        try
        {
            var response = await WebResources.HttpClient.GetAsync($"{baseAddress}{api}");
            string fail = "";
            if (!response.IsSuccessStatusCode) { fail = $"\n {await response.Content.ReadAsStringAsync()}"; }
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
        var releases = await GetReleases<T>(owner, repository, 1, 1);
        if (releases is null || releases.Length <= 0) return default;
        return releases[0];
    }
    public async Task<T[]?> GetReleases<T>(string owner, string repository, int per_page = 10, int page = 1)
    {
        string api;
        if (APIProvider == RepositoryProvider.GitHub)
        { api = $"repos/{owner}/{repository}/releases?page={page}&per_page={per_page}"; }
        else
        { api = $"projects/{owner.Replace("/", "%2F")}%2F{repository.Replace("/", "%2F")}/releases?page={page}&per_page={per_page}"; }

        var result = await TryGetAPI<T[]>(api);
        if (result.Item1)
        {
            return result.Item2;
        }
        return default;
    }

    public async Task<T?> GetFile<T>(string owner, string repository, string branch, string file)
    {
        string api;
        if (APIProvider == RepositoryProvider.GitHub)
        { api = $"repos/{owner}/{repository}/contents/{file}?ref={branch}"; }
        else
        { api = $"projects/{owner.Replace("/", "%2F")}%2F{repository.Replace("/", "%2F")}/repository/files/{file.Replace("/", "%2F").Replace(".", "%2E")}?ref={branch}"; }

        var result = await TryGetAPI<T>(api);
        if (result.Item1)
        {
            return result.Item2;
        }
        return default;
    }
}
