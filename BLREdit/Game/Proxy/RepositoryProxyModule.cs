using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;

using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace BLREdit.Game.Proxy;

public sealed class RepositoryProxyModule
{
    public RepositoryProvider RepositoryProvider { get; set; } = RepositoryProvider.Gitlab;
    [JsonIgnore] public string InstallName { get { return $"{ModuleName}-{Owner}-{Repository}".Replace('/', '-'); } }
    public string Owner { get; set; } = "blrevive";
    public string Repository { get; set; } = "modules/loadout-manager";
    public string ModuleName { get; set; } = "LoadoutManager";
    public bool Client { get; set; } = true;
    public bool Server { get; set; } = true;
    public bool Required { get; set; } = false;
    public string ProxyVersion { get; set; } = "v1.0.0-beta.2";

    private GitHubRelease? hubRelease = null;
    [JsonIgnore]
    public GitHubRelease? GitHubRelease
    {
        get
        {
            if (RepositoryProvider == RepositoryProvider.GitHub)
            {
                if (hubRelease is null) { Task.Run(GetLatestReleaseInfo).Wait(); }
                return hubRelease;
            }
            else
            {
                return null;
            }
        }
    }
    private GitlabRelease? labRelease = null;
    [JsonIgnore]
    public GitlabRelease? GitlabRelease
    {
        get
        {
            if (RepositoryProvider == RepositoryProvider.Gitlab)
            {
                if (labRelease is null) { Task.Run(GetLatestReleaseInfo).Wait(); }
                return labRelease;
            }
            else
            {
                return null;
            }
        }
    }

    private bool lockLatestReleaseInfo = false;
    public async void GetLatestReleaseInfo()
    {
        if (!lockLatestReleaseInfo)
        {
            lockLatestReleaseInfo = true;
            try
            {
                if (RepositoryProvider == RepositoryProvider.GitHub)
                {
                    hubRelease ??= await GitHubClient.GetLatestRelease(Owner, Repository);
                }
                else
                {
                    labRelease ??= await GitlabClient.GetLatestRelease(Owner, Repository);
                }
            }
            catch(Exception error)
            {
                LoggingSystem.Log($"failed to get release info for {InstallName}\n{error}");
            }

            lockLatestReleaseInfo = false;
        }
    }

    private bool lockDownload = false;
    public ProxyModule? DownloadLatest()
    {
        if (lockDownload) return null;
        lockDownload = true;

        string dl = "";
        if (RepositoryProvider == RepositoryProvider.GitHub)
        {
            if (GitHubRelease is null || GitHubRelease.Assets is null) return null;
            foreach (var asset in GitHubRelease.Assets)
            {
                if (asset.Name is not null && asset.Name.StartsWith(ModuleName) && asset.Name.EndsWith(".dll"))
                {
                    dl = asset.BrowserDownloadURL ?? string.Empty;
                    break;
                }
            }
        }
        else
        {
            if (GitlabRelease is null || GitlabRelease.Assets?.Links is null) return null;
            foreach (var asset in GitlabRelease.Assets.Links)
            {
                if (asset.Name is not null && asset.Name.StartsWith(ModuleName) && asset.Name.EndsWith(".dll"))
                {
                    dl = asset.URL ?? string.Empty;
                    break;
                }
            }

            if (string.IsNullOrEmpty(dl))
            {
                LoggingSystem.Log($"No file found in Asset links gonna go down the deep end!");
                Regex regex = new($@"(\/uploads\/\w+\/{ModuleName}.dll)");
                if (regex.Match(GitlabRelease.Description) is Match match)
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
        }
        string dlTarget = $"downloads\\{InstallName}.dll";
        ProxyModule? module = null;
        if (File.Exists(dlTarget)) { LoggingSystem.Log($"Deleting {dlTarget}"); File.Delete(dlTarget); }
        if (WebResources.DownloadFile(dl, dlTarget))
        {
            if (RepositoryProvider == RepositoryProvider.GitHub && GitHubRelease is not null)
            { module = new ProxyModule(GitHubRelease, ModuleName, Owner, Repository, Client, Server); }
            else if (GitlabRelease is not null)
            { module = new ProxyModule(GitlabRelease, ModuleName, Owner, Repository, Client, Server); }

            if (module is null) return module;

            ProxyModule? toRemoveModule = null;
            foreach (var mod in DataStorage.CachedModules)
            {
                if (mod.InstallName == module.InstallName)
                {
                    toRemoveModule = mod;
                    break;
                }
            }

            if (toRemoveModule is not null) DataStorage.CachedModules.Remove(toRemoveModule);

            DataStorage.CachedModules.Add(module);
        }
        lockDownload = false;
        return module;
    }
}
