using BLREdit.Export;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.UI.Views;

public sealed class MainWindowView : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    private string windowTitle = "";
    public string WindowTitle { get { return windowTitle; } set { windowTitle = value; OnPropertyChanged(); } }

    public void UpdateWindowTitle()
    {
        string BuildTag = "";

#if DEBUG
        BuildTag = "[Debug Build]:";
#elif RELEASE
        BuildTag = "[Release Build]:";
#elif PUBLISH
        BuildTag = "[Release Build]:";
#endif

        var PlayerProfile = ExportSystem.GetOrAddProfileSettings(BLREditSettings.Settings?.PlayerName ?? "");

        WindowTitle = $"{BuildTag}{App.CurrentRepo} - {App.CurrentVersion}, {BLREditSettings.Settings?.PlayerName} Playtime:[{new TimeSpan(0,0, PlayerProfile.PlayTime)}]";
    }
}
