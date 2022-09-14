using BLREdit.Game.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BLREdit.API.REST_API;

public class RESTAPIClient
{
    RepositoryProvider API_Provider;
    string BaseAddress = "";

    public RESTAPIClient(RepositoryProvider prov, string baseAddress)
    { 
        BaseAddress = baseAddress;
        API_Provider = prov;
    }

    private async Task<HttpResponseMessage> GetAsync(string api)
    {
        var response = await IOResources.HttpClient.GetAsync($"{BaseAddress}{api}");
        LoggingSystem.Log($"[{API_Provider}]({response.StatusCode}): GET {api}");
        return response;
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

        var response = await GetAsync(api);
        if (response.IsSuccessStatusCode)
        { 
            var content = await response.Content.ReadAsStringAsync();
            return IOResources.Deserialize<T[]>(content);
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

        var response = await GetAsync(api);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return IOResources.Deserialize<T>(content);
        }
        return default;
    }
}
