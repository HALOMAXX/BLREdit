using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;

using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace BLREdit.Game.Proxy
{
    public class RepositoryProxyModule
    {
        public RepositoryProvider RepositoryProvider { get; set; } = RepositoryProvider.Gitlab;
        public string Owner { get; set; } = "blrevive";
        public string Repository { get; set; } = "modules/loadout-manager";
        public string ModuleName { get; set; } = "LoadoutManager";
        public bool Client { get; set; } = true;
        public bool Server { get; set; } = true;

        private GitHubRelease hubRelease = null;
        [JsonIgnore]
        public GitHubRelease GitHubRelease
        {
            get
            {
                if (RepositoryProvider == RepositoryProvider.GitHub)
                {
                    if (hubRelease is null) { Task.Run(GetLatestReleaseInfo); }
                    return hubRelease;
                }
                else
                {
                    return null;
                }
            }
        }
        private GitlabRelease labRelease = null;
        [JsonIgnore]
        public GitlabRelease GitlabRelease
        {
            get
            {
                if (RepositoryProvider == RepositoryProvider.Gitlab)
                {
                    if (labRelease is null) { Task.Run(GetLatestReleaseInfo); }
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
                catch
                {
                    LoggingSystem.Log($"failed to get release info for {ModuleName}");
                }

                lockLatestReleaseInfo = false;
            }
        }

        private bool lockDownload = false;
        public async Task<ProxyModule> DownloadLatest()
        {
            if (lockDownload) return null;
            lockDownload = true;

            string dl = "";
            if (RepositoryProvider == RepositoryProvider.GitHub)
            {
                if (hubRelease is null) { lockLatestReleaseInfo = true; hubRelease = await GitHubClient.GetLatestRelease(Owner, Repository); lockLatestReleaseInfo = false; }
                foreach (var asset in hubRelease.assets)
                {
                    if (asset.name.StartsWith(ModuleName) && asset.name.EndsWith(".dll"))
                    {
                        dl = asset.browser_download_url;
                        break;
                    }
                }
            }
            else
            {
                if (labRelease is null) { lockLatestReleaseInfo = true; labRelease = await GitlabClient.GetLatestRelease(Owner, Repository); lockLatestReleaseInfo = false; }
                foreach (var asset in labRelease.assets.links)
                {
                    if (asset.name.StartsWith(ModuleName) && asset.name.EndsWith(".dll"))
                    {
                        dl = asset.url;
                        break;
                    }
                }
            }
            string dlTarget = $"downloads\\{ModuleName}.dll";
            if (File.Exists(dlTarget)) { LoggingSystem.Log($"Deleting {dlTarget}"); File.Delete(dlTarget); }
            LoggingSystem.Log($"Downloading {dl}");
            IOResources.WebClient.DownloadFile(dl, dlTarget);
            LoggingSystem.Log($"Finished Downloading {dl}");

            ProxyModule module;

            if (RepositoryProvider == RepositoryProvider.GitHub)
            { module = new ProxyModule(hubRelease, ModuleName, Client, Server); }
            else
            { module = new ProxyModule(labRelease, ModuleName, Client, Server); }

            ProxyModule toRemoveModule = null;
            foreach (var mod in ProxyModule.CachedModules)
            {
                if (mod.ModuleName == module.ModuleName)
                {
                    toRemoveModule = mod;
                    break;
                }
            }

            if (toRemoveModule is not null) ProxyModule.CachedModules.Remove(toRemoveModule);

            ProxyModule.CachedModules.Add(module);

            lockDownload = false;
            return module;
        }
    }
}
