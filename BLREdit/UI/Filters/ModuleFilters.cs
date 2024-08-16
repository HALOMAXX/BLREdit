using BLREdit.UI.Views;

namespace BLREdit.UI;

public sealed class ModuleFilters
{
    public static bool FullFilter(object o)
    {
        if (o is VisualProxyModule item)
        {
            return item.RepositoryProxyModule.ProxyVersion == DataStorage.Settings.SelectedBLReviveVersion;
        }
        return false;
    }
}
