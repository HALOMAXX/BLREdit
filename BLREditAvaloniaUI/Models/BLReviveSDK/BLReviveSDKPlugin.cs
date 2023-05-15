using System.Collections.ObjectModel;
using System.IO;

namespace BLREdit.Models.BLReviveSDK;

public sealed class BLReviveSDKPlugin : ModelBase
{
    public static DirectoryInfo PluginInfoLocation { get; }
    public static DirectoryInfo PluginCacheLocation { get; }
    public static RangeObservableCollection<BLReviveSDKPlugin> Plugins { get; }

    static BLReviveSDKPlugin()
    {
        PluginInfoLocation = new DirectoryInfo("Data\\PluginInfos");
        PluginCacheLocation = new DirectoryInfo("Data\\PluginCache");
        Plugins = new(IOResources.DeserializeDirectory<BLReviveSDKPlugin>(PluginInfoLocation));
    }


}
