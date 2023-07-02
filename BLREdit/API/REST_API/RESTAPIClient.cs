using BLREdit.Game.Proxy;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BLREdit.API.REST_API;

public sealed class RESTAPIClient
{
    readonly RepositoryProvider API_Provider;
    readonly string BaseAddress = "";

    readonly Dictionary<string, object> RequestCache = new();

    public RESTAPIClient(RepositoryProvider prov, string baseAddress)
    { 
        BaseAddress = baseAddress;
        API_Provider = prov;
    }

    private async Task<HttpResponseMessage> GetAsync(string api)
    {
        try
        {
            var response = await IOResources.HttpClient.GetAsync($"{BaseAddress}{api}");
            string fail = "";
            if (!response.IsSuccessStatusCode) { fail = $"\n {await response.Content.ReadAsStringAsync()}"; }
            LoggingSystem.Log($"[{API_Provider}]({response.StatusCode}): GET {api}{fail}");
            return response;
        }
        catch (Exception error)
        {
            LoggingSystem.Log($"[{API_Provider}]({error.GetType().Name}): GET {api}\n{error}");
            return null;
        }
    }

    public async Task<T> GetLatestRelease<T>(string owner, string repository)
    {
        var releases = await GetReleases<T>(owner, repository, 1, 1);
        if (releases is null || releases.Length <= 0) return default;
        return releases[0];
    }
    public async Task<T[]> GetReleases<T>(string owner, string repository, int per_page = 10, int page = 1)
    {
        string api;
        if (API_Provider == RepositoryProvider.GitHub)
        { api = $"repos/{owner}/{repository}/releases?per_page={per_page}&page={page}"; }
        else
        { api = $"projects/{owner.Replace("/", "%2F")}%2F{repository.Replace("/", "%2F")}/releases?page={page}&per_page={per_page}"; }

        if (RequestCache.TryGetValue(api, out object value))
        {
            LoggingSystem.Log($"[Cache]:({typeof(T).Name}) {api}");
            return (T[])value;
        }

        using (var response = await GetAsync(api))
        {
            if (response is not null && response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var releases = IOResources.Deserialize<T[]>(content);
                RequestCache.Add(api, releases);
                return releases;
            }
        }
        return default;
    }

    public async Task<T> GetFile<T>(string owner, string repository, string branch, string file)
    {
        string api;
        if (API_Provider == RepositoryProvider.GitHub)
        { api = $"repos/{owner}/{repository}/contents/{file}?ref={branch}"; }
        else
        { api = $"projects/{owner.Replace("/", "%2F")}%2F{repository.Replace("/", "%2F")}/repository/files/{file.Replace("/", "%2F").Replace(".", "%2E")}?ref={branch}"; }

        if (RequestCache.TryGetValue(api, out object value))
        {
            LoggingSystem.Log($"[Cache]:({typeof(T).Name}) {api}");
            return (T)value;
        }

        using (var response = await GetAsync(api))
        {
            if (response is not null && response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var fileData = IOResources.Deserialize<T>(content);
                RequestCache.Add(api, fileData);
                return fileData;
            }
        }
        return default;
    }
}
