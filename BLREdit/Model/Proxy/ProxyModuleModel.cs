using BLREdit.API.REST_API;
using BLREdit.Model.BLR;
using BLREdit.UI;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

    private UIBool _isClientModule = new(true);
    private UIBool _isServerModule = new(true);
    private UIBool _isRequiredModule = new(false);

    private ProxyModuleRepository _repository;

    public UIBool IsClientModule { get { return _isClientModule; } set { _isClientModule = value; OnPropertyChanged(); } }
    public UIBool IsServerModule { get { return _isServerModule; } set { _isServerModule = value; OnPropertyChanged(); } }
    public UIBool IsRequiredModule { get { return _isRequiredModule; } set { _isRequiredModule = value; OnPropertyChanged(); } }

    public ProxyModuleRepository Repository { get { return _repository; } set { _repository = value; OnPropertyChanged(); } }

    [JsonIgnore] public UIBool IsInstalling { get; set; } = new(false);
    public void InstallToClient(BLRClientModel client)
    {
        if (IsInstalling.Is) return;
        IsInstalling.Set(true);

        Repository.DownloadModuleToCache();

        var info = new FileInfo($"downloads\\moduleCache\\{Repository.FullName}.dll");
        if (info.Exists)
        {
            info.CopyTo($"{client.ProxyModuleFolder}{Repository.FullName}.dll");
        }

        IsInstalling.Set(false);
    }

    [JsonIgnore] public UIBool IsRemoving { get; set; } = new(false);
    public void RemoveFromClient(BLRClientModel client)
    {
        if (IsRemoving.Is) return;
        IsRemoving.Set(true);
        if (client.InstalledModules.Contains(this))
        {
            var info = new FileInfo($"{client.ProxyModuleFolder}{this.Repository.FullName}.dll");
            if (info.Exists) { info.Delete(); }
            client.InstalledModules.Remove(this);
        }
        IsRemoving.Set(false);
    }
}
