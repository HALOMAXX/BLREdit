using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;
using BLREdit.Game;
using BLREdit.Game.Proxy;
using BLREdit.UI;

using HtmlAgilityPack;

using System;
using System.CodeDom;
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
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    public VisualProxyModule(RepositoryProxyModule module)
    { RepositoryProxyModule = module; MainWindow.ClientWindow.PropertyChanged += ActiveClientChanged; }

    public RepositoryProxyModule RepositoryProxyModule { get; set; }

    #region MetaData
    private readonly Dictionary<string, string> metaData = new();
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

    private BitmapImage? image;
    public BitmapImage? Image
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

            var site = await WebResources.HttpClientWeb.GetStringAsync(api);
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


    private ICommand? installCommand;
    public ICommand InstallCommand
    {
        get
        {
            installCommand ??= new RelayCommand(
                    param => Task.Run(() => InstallModule(MainWindow.ClientWindow.Client))
                );
            return installCommand;
        }
    }

    private ICommand? removeCommand;
    public ICommand RemoveCommand
    {
        get
        {
            removeCommand ??= new RelayCommand(
                    param => Task.Run(() => RemoveModule(MainWindow.ClientWindow.Client))
                );
            return removeCommand;
        }
    }

    private ProxyModule? installedModule;
    public ProxyModule? InstalledModule
    { get { return installedModule; } }

    public UIBool Installed { get; } = new(false);

    public UIBool UpToDate { get; } = new(false);

    private DateTime? releaseDate;
    public DateTime? ReleaseDate 
    { 
        get 
        {
            if (releaseDate is null)
            {
                Task.Run(async () => { releaseDate = await GetLatestReleaseDate(); }).Wait();
            }
            return releaseDate; 
        }
    }

    private void CheckForInstall(BLRClient? client)
    {
        Installed.Set(false);
        if (client is null) return;

        foreach (var mod in client.InstalledModules)
        {
            if (mod.InstallName == RepositoryProxyModule.InstallName)
            {
                installedModule = mod;
                Installed.Set(true);
                break;
            }
        }
    }

    private void CheckForUpdate(BLRClient? client)
    {
        UpToDate.Set(false);
        if (client is null || Installed.IsNot || installedModule is null) return;
        try
        {
            var isUpToDate = installedModule.Published >= ReleaseDate;
            UpToDate.Set(isUpToDate);
        }
        catch (Exception error)
        {
            LoggingSystem.Log($"Failed to get Updated Info Reason:\n{error}");
        }
    }


    [JsonIgnore] public UIBool LockInstall { get; } = new(false);
    public void InstallModule(BLRClient? client)
    {
        if (client is null || LockInstall.Is) return;
        LockInstall.Set(true);
        try
        {
            LoggingSystem.Log($"Begun Installing {RepositoryProxyModule.InstallName}");

            LoggingSystem.Log($"Got Latest Release Date:[{ReleaseDate}]");

            ProxyModule? module = null;
            foreach (var cachedModule in DataStorage.CachedModules)
            {
                if (cachedModule.InstallName == RepositoryProxyModule.InstallName && cachedModule.Published >= ReleaseDate)
                {
                    module = cachedModule;
                    break;
                }
            }

            if (module is not null) 
            { 
                if (File.Exists($"downloads\\{module.InstallName}")) 
                { 
                    LoggingSystem.Log($"Found {module.InstallName} in cache");
                } 
                else 
                { 
                    module = null; DataStorage.CachedModules.Clear();
                } 
            } 
            else LoggingSystem.Log($"{RepositoryProxyModule.InstallName} is not in cache");

            module ??= RepositoryProxyModule.DownloadLatest();

            if (module is not null)
            {
                File.Copy($"downloads\\{module.InstallName}.dll", $"{client.ModulesFolder}\\{module.InstallName}.dll", true);
                LoggingSystem.Log($"Copied {module.InstallName} from Cache to client module location");
                for (int i = 0; i < client.InstalledModules.Count; i++)
                {
                    if (client.InstalledModules[i].InstallName == module.InstallName)
                    { LoggingSystem.Log($"Removing {client.InstalledModules[i]}"); client.InstalledModules.RemoveAt(i); client.Invalidate(); i--; }
                }
                client.InstalledModules.Add(module);
                client.Invalidate();
                LoggingSystem.Log($"Added {RepositoryProxyModule.InstallName} to installed modules of {client}");
            }
        }
        catch (Exception error)
        {
            LoggingSystem.MessageLog($"failed to install module:{RepositoryProxyModule.InstallName} reason:{error}", "Error"); //TODO: Add Localization
        }
        finally
        {
            CheckForInstall(client);
            CheckForUpdate(client);
            OnPropertyChanged(nameof(InstalledModule));
            OnPropertyChanged(nameof(Installed));
            OnPropertyChanged(nameof(UpToDate));
            LoggingSystem.Log($"Finished Installing {RepositoryProxyModule.InstallName}");
            LockInstall.Set(false);
        }
    }

    [JsonIgnore] public UIBool LockRemove { get; } = new(false);
    public void RemoveModule(BLRClient? client)
    { 
        if (client is null || LockRemove.Is) return;
        LockRemove.Set(true);
        try
        {
            client.RemoveModule(RepositoryProxyModule.InstallName);
        }
        catch (Exception error)
        {
            LoggingSystem.Log($"Failed to remove Module({RepositoryProxyModule.InstallName})\nReason: {error}");
        }
        finally
        {
            CheckForInstall(client);
            CheckForUpdate(client);
            OnPropertyChanged(nameof(InstalledModule));
            OnPropertyChanged(nameof(Installed));
            OnPropertyChanged(nameof(UpToDate));
            LoggingSystem.Log($"Finished removing {RepositoryProxyModule.InstallName}");
            LockRemove.Set(false);
        }
    }

    public void ActiveClientChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Client")
        {
            var client = MainWindow.ClientWindow.Client;
            LoggingSystem.Log($"New Client Got Set {client}");

            CheckForInstall(client);
            CheckForUpdate(client);

            LoggingSystem.Log($"[{RepositoryProxyModule.InstallName}]: Installed:{Installed.Is}, UpToDate:{UpToDate.Is}");

            OnPropertyChanged(nameof(InstalledModule));
            OnPropertyChanged(nameof(Installed));
            OnPropertyChanged(nameof(UpToDate));
        }
    }

    private async Task<DateTime> GetLatestReleaseDate()
    {
        if (RepositoryProxyModule.RepositoryProvider == RepositoryProvider.GitHub)
        {
            var releaseInfo = await GitHubClient.GetLatestRelease(RepositoryProxyModule.Owner, RepositoryProxyModule.Repository);
            return releaseInfo?.PublishedAt ?? DateTime.MinValue;
        }
        else
        {
            var releaseInfo = await GitlabClient.GetLatestRelease(RepositoryProxyModule.Owner, RepositoryProxyModule.Repository);
            return releaseInfo?.ReleasedAt ?? DateTime.MinValue;
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
