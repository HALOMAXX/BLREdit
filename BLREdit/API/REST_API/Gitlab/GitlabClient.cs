using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.API.REST_API.Gitlab;

public static class GitlabClient
{
    public static HttpClient Client { get; } = new HttpClient() { BaseAddress = new("https://gitlab.com/api/v4/"), Timeout = new(0, 0, 10) };

    static GitlabClient()
    {
        Client.DefaultRequestHeaders.Add("User-Agent", $"BLREdit-{App.CurrentVersion}");
        Client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    private static async Task<HttpResponseMessage> GetAsync(string api)
    {
        var response = await Client.GetAsync(api);
        LoggingSystem.LogInfo($"[Gitlab]({response.StatusCode}): GET {api}");
        return response;
    }

    public static async Task<GitlabRelease> GetLatestRelease(string repo, string owner)
    {
        var releases = await GetReleases(owner, repo, 1, 1);
        if (releases is null) return null;
        return releases[0];
    }

    public static async Task<GitlabRelease[]> GetReleases(string repo, string owner, int per_page = 1, int page = 1)
    {
        var response = await GetAsync($"projects/{owner.Replace("/", "%2F")}%2F{repo.Replace("/", "%2F")}/releases?page={page}&per_page={per_page}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return IOResources.Deserialize<GitlabRelease[]>(content);
        }
        return null;
    }
}