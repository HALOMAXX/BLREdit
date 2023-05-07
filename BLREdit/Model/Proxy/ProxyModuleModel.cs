using BLREdit.API.REST_API;
using BLREdit.Model.BLR;
using BLREdit.UI;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BLREdit.Model.Proxy;

public sealed class ProxyModuleModel : INotifyPropertyChanged
{
    #region Event
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Event

    public static RangeObservableCollection<ProxyModuleModel> CachedModules { get; } = IOResources.DeserializeFile<RangeObservableCollection<ProxyModuleModel>>($"ModuleCache.json") ?? new();

    private UIBool _isClientModule = new(true);
    private UIBool _isServerModule = new(true);
    private UIBool _isRequiredModule = new(false);

    private ProxyModuleRepository _repository;

    public UIBool IsClientModule { get { return _isClientModule; } set { _isClientModule = value; OnPropertyChanged(); } }
    public UIBool IsServerModule { get { return _isServerModule; } set { _isServerModule = value; OnPropertyChanged(); } }
    public UIBool IsRequiredModule { get { return _isRequiredModule; } set { _isRequiredModule = value; OnPropertyChanged(); } }

    public ProxyModuleRepository Repository { get { return _repository; } set { _repository = value; OnPropertyChanged(); } }

    [JsonIgnore] public UIBool IsChanging { get; set; } = new(false);
    public void InstallToClient(BLRClientModel client)
    {
        if (IsChanging.Is) return;
        IsChanging.Set(true);
        try
        {
            DownloadModuleToCache();

            var info = new FileInfo($"downloads\\moduleCache\\{Repository.FullName}.dll");
            if (info.Exists)
            {
                info.CopyTo($"{client.ProxyModuleFolder}{Repository.FullName}.dll", true);
                client.InstalledModules.Add(this);
            }
        }
        catch { throw; }
        finally 
        { 
            IsChanging.Set(false);
            LoggingSystem.Log($"[{this}]: Finished Installing to [{client}]");
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is ProxyModuleModel model)
        { 
            return model.GetHashCode() == GetHashCode();
        }
        return false;
    }

    public override string ToString()
    {
        return Repository.ToString();
    }

    public override int GetHashCode()
    {
        return Repository.GetHashCode();
    }

    private bool downloading = false;
    public void DownloadModuleToCache()
    {
        if (downloading) return;
        downloading = true;
        int index = CachedModules.IndexOf(this);
        DateTime latestReleaseDate = this.Repository.GetLatestReleaseDateTime();
        if (index >= 0 || index == -1)
        {
            if (index == -1 || CachedModules[index].Repository.ReleaseTime < latestReleaseDate)
            {
                this.Repository.DownloadModuleToCache();
                if (index >= 0) { CachedModules.RemoveAt(index); }
                CachedModules.Add(this);
            }
        }
        downloading = false;
    }
    public void RemoveFromClient(BLRClientModel client)
    {
        if (IsChanging.Is) return;
        IsChanging.Set(true);
        try
        { 
            if (client.InstalledModules.Contains(this))
            {
                var info = new FileInfo($"{client.ProxyModuleFolder}{this.Repository.FullName}.dll");
                if (info.Exists) { info.Delete(); }
                client.InstalledModules.Remove(this);
            }
        }
        catch { throw; }
        finally 
        { 
            IsChanging.Set(false);
            LoggingSystem.Log($"[{this}]: Finished Removing from [{client}]");
        }
    }
}