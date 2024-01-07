using BLREdit.API.Export;
using BLREdit.Game;
using BLREdit.UI;
using BLREdit.UI.Views;

using System;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace BLREdit.Export;

public sealed class BLRLoadoutStorage(ShareableLoadout shareable, BLRLoadout? blr = null)
{
    public ShareableLoadout Shareable { get; } = shareable;
    private BLRLoadout? blr = blr;
    public BLRLoadout BLR { get { if (blr is null) { blr = Shareable.ToBLRLoadout(); string message = string.Empty; blr.Apply = blr.ValidateLoadout(ref message); } return blr; } }
    static bool isExchanging = false;
    public static event EventHandler? ProfileGotRemoved;

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

    public static void Exchange(int from, int to)
    {
        if (from == to) return;
        if (isExchanging) return;
        isExchanging = true;
        DataStorage.ShareableProfiles.Exchange(from, to);
        DataStorage.Loadouts.Exchange(from, to);
        DataStorage.ShareableProfiles.SignalExchange();
        DataStorage.Loadouts.SignalExchange();
        isExchanging = false;
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
        var shar = share ?? MagiCowsLoadout.DefaultLoadout1.ConvertToShareable();
        shar.Name = Name;
        shar.Apply = true;
        var blr = loadout ?? shar.ToBLRLoadout();
        //blr?.Write(share);
        var load = new BLRLoadoutStorage(shar, blr);
        DataStorage.ShareableLoadouts.Add(shar);
        DataStorage.Loadouts.Add(load);
        return load;
    }
}