using BLREdit.Core;
using BLREdit.Core.Utils;

using System.Collections.ObjectModel;

namespace BLREdit.Models.BLReviveSDK;

public sealed class BLReviveSDKPlugin : ModelBase
{
    public static DirectoryInfo PluginInfoLocation { get; }
    public static DirectoryInfo PluginCacheLocation { get; }
    public static RangeObservableCollection<BLReviveSDKPlugin> Plugins { get; } = new();

    static BLReviveSDKPlugin()
    {
        PluginInfoLocation = new DirectoryInfo("Data\\Plugins");
        PluginCacheLocation = new DirectoryInfo("Data\\PluginCache");
        IOResources.DeserializeDirectoryInto(Plugins, PluginInfoLocation);
    }
}