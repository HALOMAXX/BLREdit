using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLREdit.API.REST_API.GitHub;

public static class GitHubClient
{
    public static HttpClient Client { get; } = new HttpClient() { BaseAddress = new("https://api.github.com/"), Timeout = new(0, 0, 10) };

    static GitHubClient()
    {
        Client.DefaultRequestHeaders.Add("User-Agent", $"BLREdit-{App.CurrentVersion}");
        Client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
    }

    private static async Task<HttpResponseMessage> GetAsync(string api)
    {
        var response = await Client.GetAsync(api);
        LoggingSystem.LogInfo($"[GitHub]: GET {api} returned:{response.StatusCode}");
        return response;
    }

    public static async Task<GitHubRelease> GetLatestRelease(string repo, string owner)
    {
        var response = await GetAsync($"repos/{owner}/{repo}/releases/latest");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return IOResources.Deserialize<GitHubRelease>(content);
        }
        return null;
    }

    public static async Task<GitHubRelease[]> GetReleases(string repo, string owner, int per_page = 1, int page = 1)
    {
        var response = await GetAsync($"repos/{owner}/{repo}/releases?page={page}&per_page={per_page}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return IOResources.Deserialize<GitHubRelease[]>(content);
        }
        return null;
    }

    public static async Task<T> GetObjectFromFile<T>(string repo, string owner, string branch, string file)
    {
        var response = await GetAsync($"repos/{owner}/{repo}/contents/{file}?ref={branch}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return IOResources.Deserialize<T>(content);
        }
        return default;
    }
}