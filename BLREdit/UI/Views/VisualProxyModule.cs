using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;
using BLREdit.Game;
using BLREdit.Game.Proxy;
using BLREdit.UI;

using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BLREdit.UI.Views;

public sealed class VisualProxyModule : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    public VisualProxyModule()
    { MainWindow.ClientWindow.PropertyChanged += ActiveClientChanged; }

    public RepositoryProxyModule RepositoryProxyModule { get; set; }

    #region MetaData
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
    #endregion MetaData


    private ICommand installCommand;
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

    private ProxyModule installedModule;
    public ProxyModule InstalledModule
    { get { return installedModule; } }

    private UIBool installed = new(false);
    public UIBool Installed
    { get { return installed; } }

    private UIBool upToDate = new(false);
    public UIBool UpToDate
    { get { return upToDate; } }

    private void CheckForInstall()
    {
        Installed.SetBool(false);
        if (MainWindow.ClientWindow.Client is not null)
        {
            foreach (var mod in MainWindow.ClientWindow.Client.InstalledModules)
            {
                if (mod.InstallName == RepositoryProxyModule.InstallName)
                {
                    installedModule = mod;
                    Installed.SetBool(true);
                    break;
                }
            }
        }
    }

    private void CheckForUpdate()
    {
        upToDate.SetBool(false);
        if (MainWindow.ClientWindow.Client is not null && installed.Is && installedModule is not null)
        {
            try
            {
                var task = Task.Run(GetLatestReleaseDate);
                task.Wait();
                DateTime latestRelease = task.Result;
                var isUpToDate = installedModule.Published >= latestRelease;
                UpToDate.SetBool(isUpToDate);
            }
            catch (Exception error)
            {
                LoggingSystem.Log($"Failed to get Updated Info Reason:\n{error.Message}");
            }
        }
    }


    private bool lockInstall = false;
    public async void InstallModule()
    {
        if (lockInstall) return;
        lockInstall = true;
        try
        {
            LoggingSystem.Log($"Begun Installing {RepositoryProxyModule.InstallName}");

            if (MainWindow.ClientWindow.Client.Patched.IsNot)
            {
                LoggingSystem.Log($"We have to patch the client before installing any modules or installing will fail");
                MainWindow.ClientWindow.Client.PatchClient();
            }

            var releaseDate = await GetLatestReleaseDate();
            LoggingSystem.Log($"Got Latest Release Date:[{releaseDate}]");

            ProxyModule module = null;
            foreach (var cachedModule in ProxyModule.CachedModules)
            {
                if (cachedModule.InstallName == RepositoryProxyModule.InstallName && cachedModule.Published >= releaseDate)
                {
                    module = cachedModule;
                    break;
                }
            }

            if (module is not null) LoggingSystem.Log($"Found {module.InstallName} in cache"); else LoggingSystem.Log($"{RepositoryProxyModule.InstallName} is not in cache");

            module ??= await RepositoryProxyModule.DownloadLatest();

            if (module is not null)
            {
                File.Copy($"downloads\\{module.InstallName}.dll", $"{MainWindow.ClientWindow.Client.ModulesFolder}\\{module.InstallName}.dll", true);
                LoggingSystem.Log($"Copied {module.InstallName} from Cache to client module location");
                for (int i = 0; i < MainWindow.ClientWindow.Client.InstalledModules.Count; i++)
                {
                    if (MainWindow.ClientWindow.Client.InstalledModules[i].InstallName == module.InstallName)
                    { LoggingSystem.Log($"Removing {MainWindow.ClientWindow.Client.InstalledModules[i]}"); MainWindow.ClientWindow.Client.InstalledModules.RemoveAt(i); i--; }
                }
                MainWindow.ClientWindow.Client.InstalledModules.Add(module);
                LoggingSystem.Log($"Added {RepositoryProxyModule.InstallName} to installed modules of {MainWindow.ClientWindow.Client}");
            }
        }
        catch (Exception error)
        {
            LoggingSystem.Log($"failed to install module:{RepositoryProxyModule.InstallName} reason:{error.Message}\n{error.StackTrace}");
        }

        CheckForInstall();
        CheckForUpdate();
        OnPropertyChanged(nameof(InstalledModule));
        OnPropertyChanged(nameof(Installed));
        OnPropertyChanged(nameof(UpToDate));
        LoggingSystem.Log($"Finished Installing {RepositoryProxyModule.InstallName}");
        lockInstall = false;
    }

    public void ActiveClientChanged(object sender, EventArgs eventArgs)
    {
        LoggingSystem.Log($"New Client Got Set {MainWindow.ClientWindow.Client}");

        CheckForInstall();
        CheckForUpdate();

        LoggingSystem.Log($"[{RepositoryProxyModule.InstallName}]: Installed:{Installed.Is}, UpToDate:{UpToDate.Is}");

        OnPropertyChanged(nameof(InstalledModule));
        OnPropertyChanged(nameof(Installed));
        OnPropertyChanged(nameof(UpToDate));
    }

    private async Task<DateTime> GetLatestReleaseDate()
    {
        if (RepositoryProxyModule.RepositoryProvider == RepositoryProvider.GitHub)
        {
            var releaseInfo = await GitHubClient.GetLatestRelease(RepositoryProxyModule.Owner, RepositoryProxyModule.Repository);
            return releaseInfo?.published_at ?? DateTime.MinValue;
        }
        else
        {
            var releaseInfo = await GitlabClient.GetLatestRelease(RepositoryProxyModule.Owner, RepositoryProxyModule.Repository);
            return releaseInfo?.released_at ?? DateTime.MinValue;
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
