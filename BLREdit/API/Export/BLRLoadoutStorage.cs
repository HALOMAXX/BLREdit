using BLREdit.API.Export;
using BLREdit.Game;
using BLREdit.UI;
using BLREdit.UI.Views;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Input;
using System.Windows.Media;

namespace BLREdit.Export;

public sealed class BLRLoadoutStorage(ShareableLoadout shareable, BLRLoadout? blr = null) : INotifyPropertyChanged
{
    public ShareableLoadout Shareable { get; } = shareable;
    private BLRLoadout? blr = blr;
    public BLRLoadout BLR { get { if (blr is null) { blr = Shareable.ToBLRLoadout(); string message = string.Empty; blr.Apply = blr.ValidateLoadout(ref message); } return blr; } }

    private static Brush ActiveBrush = new SolidColorBrush(Color.FromArgb(255, 255, 136, 0));
    private static Brush InactiveBrush = new SolidColorBrush(Color.FromArgb(14, 158, 158, 158));
    public Brush ActiveProfileBorder { get { return this.Equals(MainWindow.MainView.Profile) ? ActiveBrush : InactiveBrush; } set { } }

    #region Events
    public static event EventHandler? ProfileGotRemoved;
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    public void TriggerChangeNotify(string property = nameof(ActiveProfileBorder))
    { OnPropertyChanged(property); }

    public void Remove()
    {
        int indexShare = DataStorage.ShareableLoadouts.IndexOf(Shareable);
        int indexLoadout = DataStorage.Loadouts.IndexOf(this);
        LoggingSystem.Log($"Removing({indexShare}, {indexLoadout}): {Shareable.Name}");
        UndoRedoSystem.DoAction(() => { DataStorage.ShareableLoadouts.Remove(Shareable); }, () => { DataStorage.ShareableLoadouts.Insert(indexShare, Shareable); });
        UndoRedoSystem.DoAction(() => { DataStorage.Loadouts.Remove(this); }, () => { DataStorage.Loadouts.Insert(indexLoadout, this); });
        UndoRedoSystem.EndUndoRecord();
        ProfileGotRemoved?.Invoke(this, new EventArgs());
    }

    private ICommand? removeLoadoutCommand;
    [JsonIgnore]
    public ICommand RemoveLoadoutCommand
    {
        get
        {
            removeLoadoutCommand ??= new RelayCommand(
                    param => Remove()
                );
            return removeLoadoutCommand;
        }
    }

    public static BLRLoadoutStorage AddNewLoadoutSet(string Name = "New Loadout", BLRLoadout? loadout = null, ShareableLoadout? share = null)
    {
        string message = "";
        var shar = share ?? MagiCowsLoadout.DefaultLoadout1.ConvertToShareable();
        shar.Name = Name;
        shar.Apply = true;
        var blr = loadout ?? shar.ToBLRLoadout();
        blr.Apply = blr.ValidateLoadout(ref message);
        var load = new BLRLoadoutStorage(shar, blr);
        DataStorage.ShareableLoadouts.Add(shar);
        DataStorage.Loadouts.Add(load);
        return load;
    }
}