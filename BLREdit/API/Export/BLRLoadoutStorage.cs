using BLREdit.API.Export;
using BLREdit.API.Utils;
using BLREdit.Game;
using BLREdit.Import;
using BLREdit.UI;
using BLREdit.UI.Views;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Input;
using System.Windows.Media;

using static BLREdit.API.Utils.HelperFunctions;

namespace BLREdit.Export;

public sealed class BLRLoadoutStorage : INotifyPropertyChanged
{
    public ShareableLoadout Shareable { get; }
    private BLREditLoadout? blr;
    public BLREditLoadout BLR { get { if (blr is null) { blr = Shareable.ToBLRLoadout(); string message = string.Empty; blr.Apply = blr.ValidateLoadout(ref message); } return blr; } }

    private static Brush ActiveBrush = new SolidColorBrush(Color.FromArgb(255, 255, 136, 0));
    private static Brush InactiveBrush = new SolidColorBrush(Color.FromArgb(14, 158, 158, 158));
    private static Brush DefaultBrush = new SolidColorBrush(Color.FromArgb(255, 255, 128, 128));
    public Brush ActiveProfileBorder { get { return this.Equals(DataStorage.Settings.DefaultLoadout) ? DefaultBrush : (this.Equals(MainWindow.MainView.Profile) ? ActiveBrush : InactiveBrush); } set { } }

    public BLRLoadoutStorage(ShareableLoadout shareable, BLREditLoadout? blr = null)
    {
        Shareable = shareable;
        this.blr = blr;
        if (BLR is BLREditLoadout l && l.LoadoutReport is LoadoutErrorReport report && !report.IsValid)
        {
            BLR.IsAdvanced.Set(true);
        }
    }


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

    public void Repair()
    {
        if (BLR is BLREditLoadout l && l.LoadoutReport is LoadoutErrorReport report)
        {
            if (!report.PrimaryReport.IsValid)
            {
                l.Primary.RemoveIncompatibleAttachments();
                l.Primary.AddMissingDefaultAttachments();
            }

            if (!report.SecondaryReport.IsValid)
            {
                l.Secondary.RemoveIncompatibleAttachments();
                l.Secondary.AddMissingDefaultAttachments();
            }

            if (!report.GearReport.IsValid || report.GearReport.HasDuplicates)
            {
                if (HasAnyFlags(report.GearReport.Gear4Report, ItemReport.Invalid, ItemReport.Duplicate))
                {
                    BLR.Gear4 = null;
                }
                if (HasAnyFlags(report.GearReport.Gear3Report, ItemReport.Invalid, ItemReport.Duplicate))
                {
                    BLR.Gear3 = null;
                }
                if (HasAnyFlags(report.GearReport.Gear2Report, ItemReport.Invalid, ItemReport.Duplicate))
                {
                    BLR.Gear2 = null;
                }
                if (report.GearReport.Gear1Report.HasFlag(ItemReport.Invalid))
                {
                    BLR.Gear1 = null;
                }
            }

            if (report.ExtraReport.HasMissingItems)
            {
                #region DepotCheck
                if (HasAnyFlags(report.ExtraReport.Depot1Report, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Depot1 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 0);
                }
                if (HasAnyFlags(report.ExtraReport.Depot2Report, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Depot2 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 1);
                }
                if (HasAnyFlags(report.ExtraReport.Depot3Report, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Depot3 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 2);
                }
                if (HasAnyFlags(report.ExtraReport.Depot4Report, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Depot4 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 3);
                }
                if (HasAnyFlags(report.ExtraReport.Depot5Report, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Depot5 = ImportSystem.GetItemByIDAndType(ImportSystem.SHOP_CATEGORY, 4);
                }
                #endregion DepotCheck

                #region TauntCheck
                if (HasAnyFlags(report.ExtraReport.Taunt1Report, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Taunt1 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 0);
                }
                if (HasAnyFlags(report.ExtraReport.Taunt2Report, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Taunt2 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 1);
                }
                if (HasAnyFlags(report.ExtraReport.Taunt3Report, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Taunt3 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 2);
                }
                if (HasAnyFlags(report.ExtraReport.Taunt4Report, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Taunt4 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 3);
                }
                if (HasAnyFlags(report.ExtraReport.Taunt5Report, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Taunt5 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 4);
                }
                if (HasAnyFlags(report.ExtraReport.Taunt6Report, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Taunt6 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 5);
                }
                if (HasAnyFlags(report.ExtraReport.Taunt7Report, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Taunt7 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 6);
                }
                if (HasAnyFlags(report.ExtraReport.Taunt8Report, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Taunt8 = ImportSystem.GetItemByIDAndType(ImportSystem.EMOTES_CATEGORY, 7);
                }
                #endregion TauntCheck

                #region EmblemCheck
                if (HasAnyFlags(report.ExtraReport.TopIconReport, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.EmblemIcon = ImportSystem.GetItemByIDAndType(ImportSystem.EMBLEM_ICON_CATEGORY, 17);
                }
                if (HasAnyFlags(report.ExtraReport.TopColorReport, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.EmblemIconColor = ImportSystem.GetItemByIDAndType(ImportSystem.EMBLEM_COLOR_CATEGORY, 2);
                }
                if (HasAnyFlags(report.ExtraReport.MiddleIconReport, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.EmblemShape = ImportSystem.GetItemByIDAndType(ImportSystem.EMBLEM_SHAPE_CATEGORY, 0);
                }
                if (HasAnyFlags(report.ExtraReport.MiddleColorReport, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.EmblemShapeColor = ImportSystem.GetItemByIDAndType(ImportSystem.EMBLEM_COLOR_CATEGORY, 6);
                }
                if (HasAnyFlags(report.ExtraReport.BottomIconReport, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.EmblemBackground = ImportSystem.GetItemByIDAndType(ImportSystem.EMBLEM_BACKGROUND_CATEGORY, 0);
                }
                if (HasAnyFlags(report.ExtraReport.BottomColorReport, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.EmblemBackgroundColor = ImportSystem.GetItemByIDAndType(ImportSystem.EMBLEM_COLOR_CATEGORY, 6);
                }
                #endregion EmblemCheck

                #region ExtraETC
                if (HasAnyFlags(report.ExtraReport.AnnouncerReport, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.AnnouncerVoice = ImportSystem.GetItemByIDAndType(ImportSystem.ANNOUNCER_VOICE_CATEGORY, 0);
                }
                if (HasAnyFlags(report.ExtraReport.PlayerReport, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.PlayerVoice = ImportSystem.GetItemByIDAndType(ImportSystem.PLAYER_VOICE_CATEGORY, 0);
                }
                if (HasAnyFlags(report.ExtraReport.TitleReport, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Title = ImportSystem.GetItemByIDAndType(ImportSystem.TITLES_CATEGORY, 0);
                }
                #endregion ExtraETC

                #region Armor
                if (HasAnyFlags(report.GearReport.HelmetReport, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.Helmet = ImportSystem.GetItemByIDAndType(ImportSystem.HELMETS_CATEGORY, 0);
                }
                if (HasAnyFlags(report.GearReport.UpperBodyReport, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.UpperBody = ImportSystem.GetItemByIDAndType(ImportSystem.UPPER_BODIES_CATEGORY, 0);
                }
                if (HasAnyFlags(report.GearReport.LowerBodyReport, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.LowerBody = ImportSystem.GetItemByIDAndType(ImportSystem.LOWER_BODIES_CATEGORY, 0);
                }

                if (HasAnyFlags(report.GearReport.TacticalReport, ItemReport.Invalid))
                {
                    BLR.Tactical = ImportSystem.GetItemByIDAndType(ImportSystem.TACTICAL_CATEGORY, 0);
                }
                if (HasAnyFlags(report.GearReport.AvatarReport, ItemReport.Invalid))
                {
                    BLR.Avatar = null;
                }
                if (HasAnyFlags(report.GearReport.BodyCamoReport, ItemReport.Invalid, ItemReport.Missing))
                {
                    BLR.BodyCamo = ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_BODIES_CATEGORY, 0);
                }
                if (HasAnyFlags(report.GearReport.TrophyReport, ItemReport.Invalid))
                {
                    BLR.Trophy = null;
                }
                #endregion Armor
                BLR.IsAdvanced.Set(false);
            }
        }
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

    private ICommand? repairLoadoutCommand;
    [JsonIgnore]
    public ICommand RepairLoadoutCommand
    {
        get
        {
            repairLoadoutCommand ??= new RelayCommand(
                    param => Repair()
                );
            return repairLoadoutCommand;
        }
    }

    public BLRLoadoutStorage Duplicate()
    {
        return AddNewLoadoutSet($"{Shareable.Name} Duplicate", null, Shareable.Duplicate());
    }

    public static BLRLoadoutStorage AddNewLoadoutSet(string Name = "New Loadout", BLREditLoadout? loadout = null, ShareableLoadout? share = null)
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