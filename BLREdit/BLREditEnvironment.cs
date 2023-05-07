using BLREdit.Model.Proxy;
using BLREdit.UI;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace BLREdit;

public sealed class BLREditEnvironment : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    public static RangeObservableCollection<Thread> AppThreads { get; } = new();
    public static RangeObservableCollection<ProxyModuleModel> AvailableProxyModules { get; } = new();
    public static string BLREditLocation { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
    public static CultureInfo DefaultCulture { get; } = CultureInfo.CreateSpecificCulture("en-US");
    public static UIBool IsRunning { get; } = new(true);
}
