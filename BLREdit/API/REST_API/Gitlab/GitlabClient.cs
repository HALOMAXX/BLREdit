using BLREdit.Game.Proxy;

using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BLREdit.API.REST_API.Gitlab;

public static class GitlabClient
{
    public static readonly RESTAPIClient Client = new(RepositoryProvider.Gitlab, "https://gitlab.com/api/v4/");

    public static async Task<GitlabRelease?> GetLatestRelease(string owner, string repo)
    {
        return await Client.GetLatestRelease<GitlabRelease>(owner, repo);
    }

    public static async Task<GitlabRelease[]?> GetReleases(string owner, string repo, int amount = 10, int page = 1)
    {
        return await Client.GetReleases<GitlabRelease>(owner, repo, amount, page);
    }

    public static async Task<GitlabFile?> GetFile(string owner, string repo, string branch, string file)
    {
        return await Client.GetFile<GitlabFile>(owner, repo, branch, file);
    }

    public static (bool, string) DownloadFileFromRelease(GitlabRelease? release, string destFile, string file, string fileExt = ".dll")
    {
        if (release is null || release.Assets is null) return (false, string.Empty);
        string downloadLink = string.Empty;
        foreach (var asset in release.Assets.Links)
        {
            if (asset.Name is not null && asset.Name.StartsWith(file) && asset.Name.EndsWith(fileExt))
            {
                downloadLink = asset.URL ?? string.Empty;
                break;
            }
        }

        if (string.IsNullOrEmpty(downloadLink))
        {
            LoggingSystem.Log($"No file found in Asset links gonna go down the deep end!");
            Regex regex = new($@"(\/uploads\/\w+\/{file}{fileExt})");
            if (regex.Match(release.Description) is Match match)
            {
                LoggingSystem.Log($"Found {match.Captures.Count} matches");
                if (match.Captures.Count > 0)
                {
                    foreach (var capture in match.Captures)
                    {
                        LoggingSystem.Log($"\t{capture}");
                        if (string.IsNullOrEmpty(downloadLink))
                        {
                            downloadLink = $"https://gitlab.com/{release.Owner}/{release.Repository}{capture}";
                        }
                    }
                }
            }
        }

        Directory.CreateDirectory(RESTAPIClient.DOWNLOAD);
        string downloadTarget = $"{RESTAPIClient.DOWNLOAD}{destFile}";
        if (File.Exists(downloadTarget)) File.Delete(downloadTarget);

        return (WebResources.DownloadFile(downloadLink, downloadTarget), downloadTarget);
    }
}
