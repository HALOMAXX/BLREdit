using BLREdit.UI.Views;

namespace BLREdit.UI;

public sealed class ModuleFilters
{
    public static bool FullFilter(object o)
    {
        if (o is VisualProxyModule item)
        {
            if (DataStorage.Settings.SelectedSDKType == "BLRevive" && item.RepositoryProxyModule.ProxyVersion == "BLRevive") { return true; } else if 
                (DataStorage.Settings.SelectedSDKType == "Proxy" && item.RepositoryProxyModule.ProxyVersion != "BLRevive") { return true; } else
                { return false; }
        }
        return false;
    }
}
