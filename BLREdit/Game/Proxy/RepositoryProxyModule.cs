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
    [JsonIgnore] public string InstallName { get { return $"{PackageFileName}"; } }
    public string Owner { get; set; } = "blrevive";
    public string Repository { get; set; } = "modules/loadout-manager";
    public string ModuleName { get; set; } = "LoadoutManager";
    public string? FileName { get; set; } = null;
    [JsonIgnore] public string PackageFileName { get { if (FileName is null) { return ModuleName; } else { return FileName; } } }
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

        (bool, string) dl;
        if (RepositoryProvider == RepositoryProvider.GitHub)
        {
            dl = GitHubClient.DownloadFileFromRelease(GitHubRelease, $"{InstallName}.dll", ModuleName);
        }
        else
        {
            var packages = GitlabClient.GetGenericPackages(Owner, Repository, ModuleName);
            packages.Wait();

            var result = GitlabClient.DownloadPackage(packages.Result[0], $"{InstallName}.dll", PackageFileName);
            if (result.Item1)
            {
                var rel = new GitlabRelease() { Owner = Owner, Repository = Repository, ReleasedAt = result.Item3 };
                ProxyModule mod = new(rel, Owner, Repository, ModuleName, FileName, Client, Server);
                UpdateModuleCache(mod);
                lockDownload = false;
                return mod;
            }
            else
            {
                lockDownload = false;
                return null;
            }
        }

        ProxyModule? module = null;
        if (dl.Item1)
        {
            if (RepositoryProvider == RepositoryProvider.GitHub && GitHubRelease is not null)
            { module = new ProxyModule(GitHubRelease, Owner, Repository, ModuleName, FileName, Client, Server); }

            if (module is null) return module;

            UpdateModuleCache(module);
        }
        lockDownload = false;
        return module;
    }

    private static void UpdateModuleCache(ProxyModule module)
    {
        var index = DataStorage.CachedModules.IndexOf(module);
        if (index >= 0)
        {
            if (module is null) 
            { LoggingSystem.Log("What the h happened"); }
            DataStorage.CachedModules[index] = module;
        }
        else
        {
            DataStorage.CachedModules.Add(module);
        }
    }
}
