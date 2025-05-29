using BLREdit.Game;
using BLREdit.Game.Proxy;

using System.Collections.ObjectModel;

namespace BLREdit.UI.Views
{
    public sealed class ModuleConfigView(BLRClient client, VisualProxyModule module)
    {
        public BLRClient Client { get; } = client;
        public VisualProxyModule Module { get; } = module;

        private ObservableCollection<ProxyModuleSetting>? settings;
        public ObservableCollection<ProxyModuleSetting> Settings { get { settings ??= Client.LoadModuleSettings(Module); return settings; } }
    }
}
