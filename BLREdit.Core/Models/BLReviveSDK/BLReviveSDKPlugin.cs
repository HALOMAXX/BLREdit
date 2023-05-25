using BLREdit.Core;
using BLREdit.Core.Utils;

using System.Collections.ObjectModel;
using System.Diagnostics;

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
        var result = IOResources.DeserializeDirectory<BLReviveSDKPlugin>(PluginInfoLocation);
        if (result is not null && result.Count > 0)
        { Plugins.AddRange(result); }
        else
        { Debug.WriteLine("failed to deserialize plugins"); }
    }
}