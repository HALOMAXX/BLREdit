using HtmlAgilityPack;
using System;
using System.Text.RegularExpressions;

namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabModuleRepository : ProxyModuleRepository
{
    private GitlabRelease? _latestReleaseInfo;
    public GitlabRelease? LatestReleaseInfo { get { return _latestReleaseInfo; } set { _latestReleaseInfo = value; OnPropertyChanged(); } }

    public override void DownloadModuleToCache()
    {
        if (IsDownloadingModuleToCache.Is) return;
        IsDownloadingModuleToCache.Set(true);
        int index = CachedModules.IndexOf(this);
        DateTime latestReleaseDate = this.GetLatestReleaseDateTime();
        string? dl = null;
        if (index >= 0 || index == -1)
        {
            if (index == -1 || CachedModules[index].ReleaseTime < latestReleaseDate)
            {
                foreach (var asset in LatestReleaseInfo.Assets.Links)
                {
                    if (asset.Name.StartsWith(Name) && asset.Name.EndsWith(".dll"))
                    {
                        dl = asset.URL;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(dl))
                {
                    LoggingSystem.Log($"No file found in Asset links gonna go down the deep end!");
                    Regex regex = new($@"(\/uploads\/\w+\/{Name}.dll)");
                    if (regex.Match(LatestReleaseInfo.Description) is Match match)
                    {
                        LoggingSystem.Log($"Found {match.Captures.Count} matches");
                        if (match.Captures.Count > 0)
                        {
                            foreach (var capture in match.Captures)
                            {
                                LoggingSystem.Log($"\t{capture}");
                                if (string.IsNullOrEmpty(dl))
                                {
                                    dl = $"https://gitlab.com/{Owner}/{Repository}{capture}";
                                }
                            }
                        }
                    }
                }
                this.ReleaseTime = latestReleaseDate;
                IOResources.DownloadFile(dl, $"downloads\\moduleCache\\{FullName}.dll");

                if (index >= 0) { CachedModules.RemoveAt(index); }
                CachedModules.Add(this);
            }
        }
        IsDownloadingModuleToCache.Set(false);
    }

    public override DateTime GetLatestReleaseDateTime()
    {
        if (LatestReleaseInfo is null)
        {
            var task = GitlabClient.GetLatestRelease(Owner, Repository);
            task.Wait();
            LatestReleaseInfo = task.Result;
        }
        return LatestReleaseInfo.ReleasedAt;
    }

    public override async void GetMetaData()
    {
        if (IsGettingMetaData.Is) return;
        IsGettingMetaData.Set(true);

        var rawHtml = await IOResources.HttpClientWeb.GetStringAsync($"https://gitlab.com/{Owner}/{Repository}");
        HtmlDocument doc = new();
        doc.LoadHtml(rawHtml);

        foreach (var docNode in doc.DocumentNode.ChildNodes)
        {
            if (docNode.Name is "html")
            {
                foreach (var htmlNode in docNode.ChildNodes)
                {
                    if (htmlNode.Name is "head")
                    {
                        foreach (var headNode in htmlNode.ChildNodes)
                        {
                            if(headNode.Name is "meta")
                            {
                                if (headNode.Attributes.Count >= 2)
                                {
                                    if (!MetaData.ContainsKey(headNode.Attributes[1].Value)) { MetaData.Add(headNode.Attributes[1].Value, headNode.Attributes[0].Value); }
                                }
                            }
                        }
                    }
                }
            }
        }

        IsGettingMetaData.Set(false);
        NotifyMetaDataPropertiesChanged();
    }
}
