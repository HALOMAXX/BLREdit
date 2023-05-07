using BLREdit.Model.BLR;
using BLREdit.Model.Proxy;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BLREdit.UI.Windows;

public sealed class BLRClientModifyView : INotifyPropertyChanged
{
    #region Event
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Event

    public RangeObservableCollection<ProxyModuleViewModel> ProxyModuleList { get; }
    public BLRClientModel ClientModel { get; }
    public BLRClientModifyView(BLRClientModel client)
    {
        ClientModel = client;
        ProxyModuleList = CreateProxyModuleList(client);
    }

    private static RangeObservableCollection<ProxyModuleViewModel> CreateProxyModuleList(BLRClientModel client)
    {
        RangeObservableCollection<ProxyModuleViewModel> proxyModuleViewModels = new();

        foreach (var module in App.AvailableProxyModules)
        {
            proxyModuleViewModels.Add(new(client,module));
        }

        return proxyModuleViewModels;
    }
}
