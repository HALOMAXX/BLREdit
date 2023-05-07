using BLREdit.Game;
using BLREdit.Model.BLR;
using BLREdit.UI;

using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BLREdit.Model.Proxy;
public sealed class ProxyModuleViewModel : INotifyPropertyChanged
{
    #region Event
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Event

    public ProxyModuleModel Module { get; }
    public BLRClientModel Client { get; }
    public ProxyModuleViewModel(BLRClientModel client, ProxyModuleModel module)
    { 
        Module = module;
        Client = client;

        RegisterEvents();

        CheckInstalled();
    }
    ~ProxyModuleViewModel() 
    {
        UnregisterEvents();
    }

    private void RegisterEvents()
    {
        Client.InstalledModules.CollectionChanged += InstalledModulesChanged;
    }
    private void UnregisterEvents() 
    {
        Client.InstalledModules.CollectionChanged -= InstalledModulesChanged;
    }
    private void InstalledModulesChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        CheckInstalled();
    }
    public void CheckInstalled()
    {
        IsInstalled.Set(Client.InstalledModules.Contains(Module));
    }

    public UIBool IsInstalled { get; } = new(false);

    private ICommand _installOrUpdateCommand;
    public ICommand InstallOrUpdateCommand { get { _installOrUpdateCommand ??= new RelayCommand(param => { Task.Run(() => { Module.InstallToClient(Client); }); }); return _installOrUpdateCommand; } }
    private ICommand _removeCommand;
    public ICommand RemoveCommand { get { _removeCommand ??= new RelayCommand(param => { Task.Run(() => { Module.RemoveFromClient(Client); }); }); return _removeCommand; } }
}
