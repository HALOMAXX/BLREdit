using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;
using BLREdit.UI;

using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BLREdit.Game.Proxy;

public class VisualProxyModule : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    public RepositoryProxyModule RepositoryProxyModule { get; set; }

    private Dictionary<string, string> metaData = new();
    public Dictionary<string, string> MetaData
    {
        get
        {
            if (RepositoryProxyModule is not null && metaData.Count <= 0)
            { Task.Run(GetMetaData); }
            return metaData;
        }
    }

    public string Title
    {
        get
        {
            if (MetaData.TryGetValue("og:title", out string title))
            {
                return title;
            }
            return string.Empty;
        }
    }

    private BitmapImage image;
    public BitmapImage Image
    {
        get
        {
            if (MetaData.TryGetValue("og:image", out string imageUrl))
            {
                image ??= new BitmapImage(new Uri(imageUrl));
            }
            return image;
        }
    }

    public string Url
    {
        get
        {
            if (MetaData.TryGetValue("og:url", out string url))
            {
                return url;
            }
            return string.Empty;
        }
    }

    public string Description
    {
        get
        {
            if (MetaData.TryGetValue("og:description", out string desc))
            {
                return desc;
            }
            return string.Empty;
        }
    }

    private bool lockMetaData = false;
    public async void GetMetaData()
    {
        if (!lockMetaData)
        {
            lockMetaData = true;
            string api;
            if (RepositoryProxyModule.RepositoryProvider == RepositoryProvider.GitHub)
            {
                api = $"https://github.com/{RepositoryProxyModule.Owner}/{RepositoryProxyModule.Repository}";
            }
            else
            {
                api = $"https://gitlab.com/{RepositoryProxyModule.Owner}/{RepositoryProxyModule.Repository}";
            }

            LoggingSystem.Log(api);
            var site = await IOResources.HttpClientWeb.GetStringAsync(api);
            HtmlDocument doc = new();
            doc.LoadHtml(site);

            foreach (var docNode in doc.DocumentNode.ChildNodes)
            {
                switch (docNode.Name)
                {
                    case "html":
                        foreach (var htmlNode in docNode.ChildNodes)
                        {
                            switch (htmlNode.Name)
                            {
                                case "head":
                                    foreach (var headNode in htmlNode.ChildNodes)
                                    {
                                        switch (headNode.Name)
                                        {
                                            case "meta":
                                                if (headNode.Attributes.Count >= 2)
                                                {
                                                    if (RepositoryProxyModule.RepositoryProvider == RepositoryProvider.GitHub)
                                                    {
                                                        if (!metaData.ContainsKey(headNode.Attributes[0].Value)) { metaData.Add(headNode.Attributes[0].Value, headNode.Attributes[1].Value); }
                                                    }
                                                    else
                                                    {
                                                        if (!metaData.ContainsKey(headNode.Attributes[1].Value)) { metaData.Add(headNode.Attributes[1].Value, headNode.Attributes[0].Value); }
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
            UpdateProperties();
            lockMetaData = false;
        }
    }

    private ICommand installCommand;
    [JsonIgnore]
    public ICommand InstallCommand
    {
        get
        {
            installCommand ??= new RelayCommand(
                    param => Task.Run(InstallModule)
                );
            return installCommand;
        }
    }

    [JsonIgnore]
    public UIBool Installed
    {
        get
        {
            bool installed = false;
            if (BLRClientWindow.Client is not null)
            {
                var task = Task.Run(GetLatestReleaseDate);
                task.Wait();
                DateTime latestRelease = task.Result;
                foreach (var mod in BLRClientWindow.Client.InstalledModules)
                {
                    if (mod.ModuleName == RepositoryProxyModule.ModuleName && mod.Published >= latestRelease)
                    {
                        installed = true;
                        break;
                    }
                }
            }
            return new UIBool(installed);
        }
    }

    private bool lockInstall = false;
    public async void InstallModule()
    {
        if (lockInstall) return;
        lockInstall = true;

        LoggingSystem.Log($"Begun Installing {RepositoryProxyModule.ModuleName}");

        var releaseDate = await GetLatestReleaseDate();

        ProxyModule module = null;
        foreach (var cachedModule in ProxyModule.CachedModules)
        {
            if (cachedModule.Published >= releaseDate)
            {
                module = cachedModule;
                break;
            }
        }

        module ??= await RepositoryProxyModule.DownloadLatest();

        File.Copy($"downloads\\{module.ModuleName}.dll", $"{BLRClientWindow.Client.ModulesFolder}\\{module.ModuleName}.dll", true);

        BLRClientWindow.Client.InstalledModules.Add(module);

        OnPropertyChanged(nameof(Installed));
        lockInstall = false;
    }

    private async Task<DateTime> GetLatestReleaseDate()
    {
        if (RepositoryProxyModule.RepositoryProvider == RepositoryProvider.GitHub)
        {
            var releaseInfo = await GitHubClient.GetLatestRelease(RepositoryProxyModule.Owner, RepositoryProxyModule.Repository);
            return releaseInfo.published_at;
        }
        else
        {
            var releaseInfo = await GitlabClient.GetLatestRelease(RepositoryProxyModule.Owner, RepositoryProxyModule.Repository);
            return releaseInfo.released_at;
        }
    }

    private void UpdateProperties()
    {
        OnPropertyChanged(nameof(MetaData));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Image));
        OnPropertyChanged(nameof(Url));
        OnPropertyChanged(nameof(Description));
    }
}
