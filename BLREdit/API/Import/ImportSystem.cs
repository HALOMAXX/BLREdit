using BLREdit.Properties;
using BLREdit.UI;
using BLREdit.UI.Views;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace BLREdit.Import;

public static class ImportSystem
{
    public const string PRIMARY_CATEGORY = "primary";           //primary
    public const string SECONDARY_CATEGORY = "secondary";       //secondary
    public const string MUZZELS_CATEGORY = "muzzles";           //muzzles
    public const string MAGAZINES_CATEGORY = "magazines";       //magazines
    public const string STOCKS_CATEGORY = "stocks";             //stocks
    public const string SCOPES_CATEGORY = "scopes";             //scopes
    public const string BARRELS_CATEGORY = "barrels";           //barrels
    public const string GRIPS_CATEGORY = "grips";               //grips
    public const string HANGERS_CATEGORY = "hangers";           //hangers
    public const string CAMOS_BODIES_CATEGORY = "camosBody";    //camosBody
    public const string CAMOS_WEAPONS_CATEGORY = "camosWeapon"; //camosWeapon
    public const string HELMETS_CATEGORY = "helmets";           //helmets
    public const string LOWER_BODIES_CATEGORY = "lowerBodies";  //lowerBodies
    public const string UPPER_BODIES_CATEGORY = "upperBodies";  //upperBodies
    public const string ATTACHMENTS_CATEGORY = "attachments";   //attachments
    public const string AVATARS_CATEGORY = "avatars";           //avatars
    public const string TACTICAL_CATEGORY = "tactical";         //tactical
    public const string BADGES_CATEGORY = "badges";
    public const string AMMO_CATEGORY = "ammos";
    public const string EMOTES_CATEGORY = "emotes";
    public const string SHOP_CATEGORY = "shop";
    public const string PRIMARY_SKIN_CATEGORY = "primarySkins";
    public const string ANNOUNCER_VOICE_CATEGORY = "dialogPackAnnouncers";
    public const string PLAYER_VOICE_CATEGORY = "dialogPackPlayers";

    public const string EMBLEM_ALPHA_CATEGORY = "emblem_alpha";
    public const string EMBLEM_BACKGROUND_CATEGORY = "emblem_background";
    public const string EMBLEM_COLOR_CATEGORY = "emblem_color";
    public const string EMBLEM_ICON_CATEGORY = "emblem_icon";
    public const string EMBLEM_SHAPE_CATEGORY = "emblem_shape";

    public const string TITLES_CATEGORY = "titles";

    public static List<FoxIcon> ScopePreviews { get; } = new();

    public static Dictionary<string?, ObservableCollection<BLRItem>> ItemLists { get; private set; } = new();

    static bool IsInitialized = false;
    static readonly object initLock = new();
    public static void Initialize()
    {
        lock (initLock)
        {
            if (IsInitialized) return;
            IsInitialized = true;
            LoggingSystem.Log("Initializing Import System");
            var watch = Stopwatch.StartNew();
            LoadAllScopePreviews();
            LoggingSystem.Log($"[ImportSystem]:Finished loading ScopePreviews in {watch.ElapsedMilliseconds}ms");
            watch.Restart();
            LoadItems();
            LoggingSystem.Log($"[ImportSystem]:Finished loading items in {watch.ElapsedMilliseconds}ms");
            watch.Restart();
            UpdateImages();
            LoggingSystem.Log($"[ImportSystem]:Finished loading images in {watch.ElapsedMilliseconds}ms");
            watch.Restart();
            ApplyDisplayStats();
            LoggingSystem.Log($"[ImportSystem]:Finished loading stats in {watch.ElapsedMilliseconds}ms");
            watch.Stop();
            LoggingSystem.Log("Initializing Import System Finished");
        }
    }

    private static void LoadItems()
    {
        ItemLists = IOResources.DeserializeFile<Dictionary<string?, ObservableCollection<BLRItem>>>($"{IOResources.ASSET_DIR}{IOResources.JSON_DIR}{IOResources.ITEM_LIST_FILE}") ?? new();
    }

    public static void ApplyDisplayStats()
    {
        foreach (var itemCategory in ItemLists)
        {
            switch (itemCategory.Key)
            {
                case PRIMARY_CATEGORY:
                case SECONDARY_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        if (item is null) continue;
                        item.Category = itemCategory.Key;
                        var (DamageIdeal, DamageMax) = BLRWeapon.CalculateDamage(item, 0);
                        var (ZoomSpread, HipSpread, MovmentSpread) = BLRWeapon.CalculateSpread(item, 0, 0, item, item);
                        var (RecoilHip, _RecoilZoom) = BLRWeapon.CalculateRecoil(item, 0);
                        var (IdealRange, MaxRange, _TracerRange) = BLRWeapon.CalculateRange(item, 0);

                        item.DisplayStat1 = FormatDisplayStat(nameof(item.Damage), Resources.lbl_Damage, new double[] { DamageIdeal, DamageMax }, StatsEnum.None, "0", "", "/");
                        item.DisplayStat2 = FormatDisplayStat(nameof(item.Aim), Resources.lbl_SpreadAim, ZoomSpread, StatsEnum.None, "0.00", "°");
                        item.DisplayStat3 = FormatDisplayStat(nameof(item.Hip), Resources.lbl_SpreadHipfire, HipSpread, StatsEnum.None, "0.00", "°");
                        item.DisplayStat4 = FormatDisplayStat(nameof(item.Move), Resources.lbl_SpreadMove, MovmentSpread, StatsEnum.None, "0.00", "°");
                        item.DisplayStat5 = FormatDisplayStat(nameof(item.Recoil), Resources.lbl_RecoilHip, RecoilHip, StatsEnum.None, "0.00", "°");
                        item.DisplayStat6 = FormatDisplayStat(nameof(item.Range), Resources.lbl_Range, new double[] { IdealRange, MaxRange }, StatsEnum.None, "0", "", "/", 2);
                    }
                    break;
                case MUZZELS_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        if(item is null) continue;
                        item.Category = itemCategory.Key;
                        double damage = item.WeaponModifiers?.Damage ?? 0;
                        double spread = item.WeaponModifiers?.Accuracy ?? 0;
                        double recoil = item.WeaponModifiers?.Recoil ?? 0;
                        double range = item.WeaponModifiers?.Range ?? 0;
                        double run = item.WeaponModifiers?.MovementSpeed ?? 0;

                        item.DisplayStat1 = FormatDisplayStat(nameof(item.Damage), Resources.lbl_Damage, damage, StatsEnum.Normal, "0", "%");
                        item.DisplayStat2 = FormatDisplayStat(nameof(item.Accuracy), Resources.lbl_Accuracy, spread, StatsEnum.Normal, "0", "%");
                        item.DisplayStat3 = FormatDisplayStat(nameof(item.Recoil), Resources.lbl_Recoil, recoil, StatsEnum.Normal, "0", "%");
                        item.DisplayStat4 = FormatDisplayStat(nameof(item.Range), Resources.lbl_Range, range, StatsEnum.Normal, "0", "%");
                        item.DisplayStat5 = FormatDisplayStat(nameof(item.Run), Resources.lbl_Run, run, StatsEnum.Normal, "0", "%");
                    }
                    break;
                case STOCKS_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        if (item is null) continue;
                        item.Category = itemCategory.Key;
                        double damage = item.WeaponModifiers?.Damage ?? 0;
                        double spread = item.WeaponModifiers?.Accuracy ?? 0;
                        double recoil = item.WeaponModifiers?.Recoil ?? 0;
                        double range = item.WeaponModifiers?.Range ?? 0;
                        double run = item.WeaponModifiers?.MovementSpeed ?? 0;
                        double reload = item.WeaponModifiers?.ReloadSpeed ?? 0;

                        item.DisplayStat1 = FormatDisplayStat(nameof(item.Damage), Resources.lbl_Damage, damage, StatsEnum.Normal, "0", "%");
                        item.DisplayStat2 = FormatDisplayStat(nameof(item.Accuracy), Resources.lbl_Accuracy, spread, StatsEnum.Normal, "0", "%");
                        item.DisplayStat3 = FormatDisplayStat(nameof(item.Recoil), Resources.lbl_Recoil, recoil, StatsEnum.Normal, "0", "%");
                        item.DisplayStat4 = FormatDisplayStat(nameof(item.Range), Resources.lbl_Range, range, StatsEnum.Normal, "0", "%");
                        item.DisplayStat5 = FormatDisplayStat(nameof(item.Run), Resources.lbl_Run, run, StatsEnum.Normal, "0", "%");

                        if (item.IsValidForItemIDS(40020))
                        { item.DisplayStat6 = FormatDisplayStat(nameof(item.Reload), Resources.lbl_ReloadEmpty, reload, StatsEnum.Normal, "0", "%"); }
                    }
                    break;
                case BARRELS_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        if (item is null) continue;
                        item.Category = itemCategory.Key;
                        double damage = item.WeaponModifiers?.Damage ?? 0;
                        double spread = item.WeaponModifiers?.Accuracy ?? 0;
                        double recoil = item.WeaponModifiers?.Recoil ?? 0;
                        double range = item.WeaponModifiers?.Range ?? 0;
                        double run = item.WeaponModifiers?.MovementSpeed ?? 0;

                        item.DisplayStat1 = FormatDisplayStat(nameof(item.Damage), Resources.lbl_Damage, damage, StatsEnum.Normal, "0", "%");
                        item.DisplayStat2 = FormatDisplayStat(nameof(item.Accuracy), Resources.lbl_Accuracy, spread, StatsEnum.Normal, "0", "%");
                        item.DisplayStat3 = FormatDisplayStat(nameof(item.Recoil), Resources.lbl_Recoil, recoil, StatsEnum.Normal, "0", "%");
                        item.DisplayStat4 = FormatDisplayStat(nameof(item.Range), Resources.lbl_Range, range, StatsEnum.Normal, "0", "%");
                        item.DisplayStat5 = FormatDisplayStat(nameof(item.Run), Resources.lbl_Run, run, StatsEnum.Normal, "0", "%");
                    }
                    break;
                case SCOPES_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        if (item is null) continue;
                        item.Category = itemCategory.Key;
                        item.DisplayStat1 = FormatDisplayStat(nameof(item.Zoom), Resources.lbl_Zoom, (1.3 + (item.WikiStats?.Zoom ?? 0)), StatsEnum.Normal, "0.00", "x");
                        item.DisplayStat2 = FormatDisplayStat(nameof(item.ScopeInTime), Resources.lbl_ScopeInTime, (0.0 + (item.WikiStats?.ScopeInTime ?? 0)), StatsEnum.Normal, "0.00", "s", "+");
                        item.DisplayStat3 = FormatDisplayStat(nameof(item.Infrared), Resources.lbl_Infrared, item.UID == 45019 || item.UID == 45020 || item.UID == 45021, StatsEnum.Normal, "");
                    }
                    break;
                case MAGAZINES_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        if (item is null) continue;
                        item.Category = itemCategory.Key;
                        double ammo = item.WeaponModifiers?.Ammo ?? 0;
                        double range = item.WeaponModifiers?.Range ?? 0;
                        double reload = item.WikiStats?.Reload ?? 0;
                        double movementSpeed = item.WeaponModifiers?.MovementSpeed ?? 0;
                        double damage = item.WeaponModifiers?.Damage ?? 0;
                        double recoil = item.WeaponModifiers?.Recoil ?? 0;
                        double accuracy = item.WeaponModifiers?.Accuracy ?? 0;

                        item.DisplayStat1 = FormatDisplayStat(nameof(item.Ammo), Resources.lbl_Ammo, ammo, StatsEnum.Normal, "0");
                        item.DisplayStat2 = FormatDisplayStat(nameof(item.Damage), Resources.lbl_Damage, damage, StatsEnum.Normal, "0", "%");
                        item.DisplayStat3 = FormatDisplayStat(nameof(item.Run), Resources.lbl_Run, movementSpeed, StatsEnum.Normal, "0", "%");
                        item.DisplayStat4 = FormatDisplayStat(nameof(item.Recoil), Resources.lbl_Recoil, recoil, StatsEnum.Normal, "0", "%");
                        item.DisplayStat5 = FormatDisplayStat(nameof(item.Range), Resources.lbl_Range, range, StatsEnum.Normal, "0", "%");

                        if (item.IsValidForItemIDS(40021, 40002))
                        {
                            item.DisplayStat6 = FormatDisplayStat(nameof(item.Accuracy), Resources.lbl_Accuracy, accuracy, StatsEnum.Normal, "0", "%");
                        }
                        else
                        {
                            item.DisplayStat6 = FormatDisplayStat(nameof(item.Reload), Resources.lbl_ReloadEmpty, reload, StatsEnum.Inverted, "0.00", "s");
                        }
                    }
                    break;
                case HELMETS_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        if (item is null) continue;
                        item.Category = itemCategory.Key;
                        double health = item.PawnModifiers?.Health ?? 0;
                        double dmgReduction = item.PawnModifiers?.HelmetDamageReduction ?? 0;
                        double movement = item.PawnModifiers?.MovementSpeed ?? 0;
                        double hrv = item.PawnModifiers?.HRVDuration ?? 0;
                        double recharge = item.PawnModifiers?.HRVRechargeRate ?? 0;

                        string prop = "";
                        string desc = "";
                        double value = 0;

                        switch (item.UID)
                        {
                            case 30345:
                                prop = nameof(item.ExplosiveProtection);
                                desc = Resources.lbl_ExplosiveProtection;
                                value = item.PawnModifiers?.ExplosiveProtection ?? 0;
                                break;
                            case 30346:
                                prop = nameof(item.ToxicProtection);
                                desc = Resources.lbl_ToxicProtection;
                                value = item.PawnModifiers?.ToxicProtection ?? 0;
                                break;
                            case 30347:
                                prop = nameof(item.IncendiaryProtection);
                                desc = Resources.lbl_IncendiaryProtection;
                                value = item.PawnModifiers?.IncendiaryProtection ?? 0;
                                break;
                        }

                        item.DisplayStat1 = FormatDisplayStat(nameof(item.Health), Resources.lbl_Health, health, StatsEnum.Normal, "0", "%");
                        if (recharge >= 10)
                        {
                            item.DisplayStat2 = FormatDisplayStat(nameof(item.HRVRecharge), Resources.lbl_HRVRecharge, recharge, StatsEnum.Normal, "0", " u/s", "", -1, 6.59);
                        }
                        else
                        {
                            item.DisplayStat2 = FormatDisplayStat(nameof(item.HRVRecharge), Resources.lbl_HRVRecharge, recharge, StatsEnum.Normal, "0.0", " u/s", "", -1, 6.59);
                        }
                        
                        item.DisplayStat3 = FormatDisplayStat(nameof(item.HeadProtection), Resources.lbl_HeadArmor, dmgReduction, StatsEnum.Normal, "0.0", "%");
                        item.DisplayStat4 = FormatDisplayStat(nameof(item.Run), Resources.lbl_Run, movement, StatsEnum.Normal, "0", "%");
                        item.DisplayStat5 = FormatDisplayStat(nameof(item.HRVDuration), Resources.lbl_HRVDuration, hrv, StatsEnum.Normal, "0.0", "u", "", -1, 69.9);

                        if (value != 0)
                        {
                            item.DisplayStat6 = FormatDisplayStat(prop, desc, value, StatsEnum.Normal, "0", "%");
                        }
                    }
                    break;
                case TACTICAL_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        if (item is null) continue;
                        item.Category = itemCategory.Key;
                        double hrv = item.PawnModifiers?.HRVDuration ?? 0;
                        double recharge = item.PawnModifiers?.HRVRechargeRate ?? 0;

                        item.DisplayStat1 = FormatDisplayStat(nameof(item.HRVDuration), Resources.lbl_HRVDuration, hrv, StatsEnum.Normal, "0.0", "u", "", -1, 0);
                        item.DisplayStat2 = FormatDisplayStat(nameof(item.HRVRecharge), Resources.lbl_HRVRecharge, recharge, StatsEnum.Normal, "0.0", "u/s", "", -1, 0);
                    }
                    break;
                case UPPER_BODIES_CATEGORY:
                case LOWER_BODIES_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        if (item is null) continue;
                        item.Category = itemCategory.Key;
                        double health = item.PawnModifiers?.Health ?? 0;
                        double movement = item.PawnModifiers?.MovementSpeed ?? 0;
                        double gear = item.PawnModifiers?.GearSlots ?? 0;

                        item.DisplayStat1 = FormatDisplayStat(nameof(item.Health), Resources.lbl_Health, health, StatsEnum.Normal, "0", "%");
                        item.DisplayStat2 = FormatDisplayStat(nameof(item.Run), Resources.lbl_Run, movement, StatsEnum.Normal, "0", "%");
                        item.DisplayStat3 = FormatDisplayStat(nameof(item.GearSlots), Resources.lbl_GearSlots, gear, StatsEnum.Normal, "0");
                    }
                    break;
                case ATTACHMENTS_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        if (item is null) continue;
                        item.Category = itemCategory.Key;
                        double ElectroProtection = item.PawnModifiers?.ElectroProtection ?? 0;
                        double ExplosiveProtection = item.PawnModifiers?.ExplosiveProtection ?? 0;
                        double IncendiaryProtection = item.PawnModifiers?.IncendiaryProtection ?? 0;
                        double MeleeProtection = item.PawnModifiers?.MeleeProtection ?? 0;
                        double ToxicProtection = item.PawnModifiers?.ToxicProtection ?? 0;
                        double InfraredProtection = item.PawnModifiers?.InfraredProtection ?? 0;

                        string prop = "";
                        string desc = "";
                        double value = 0;

                        switch (item.UID)
                        {
                            case 12021:
                                prop = nameof(item.IncendiaryProtection);
                                desc = Resources.lbl_IncendiaryProtection;
                                value = IncendiaryProtection;
                                break;
                            case 12022:
                                prop = nameof(item.ToxicProtection);
                                desc = Resources.lbl_ToxicProtection;
                                value = ToxicProtection;
                                break;
                            case 12023:
                                prop = nameof(item.ExplosiveProtection);
                                desc = Resources.lbl_ExplosiveProtection;
                                value = ExplosiveProtection;
                                break;
                            case 12024:
                                prop = nameof(item.ElectroProtection);
                                desc = Resources.lbl_ElectroProtection;
                                value = ElectroProtection;
                                break;
                            case 12025:
                                prop = nameof(item.MeleeProtection);
                                desc = Resources.lbl_MeleeProtection;
                                value = MeleeProtection;
                                break;
                            case 12030:
                                prop = nameof(item.InfraredProtection);
                                desc = Resources.lbl_InfraredProtection;
                                value = InfraredProtection;
                                break;
                        }
                        item.DisplayStat1 = FormatDisplayStat(prop, desc, value, StatsEnum.Normal, "0", "%");
                    }
                    break;
                case GRIPS_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        if (item is null) continue;
                        item.Category = itemCategory.Key;
                        double _damage = item.WeaponModifiers?.Damage ?? 0;
                        double _rof = item.WeaponModifiers?.RateOfFire ?? 0;
                        double recoil = item.WeaponModifiers?.Recoil ?? 0;

                        //FormatDisplayStat(ref desc1, "Damage", "Damage:", damage, "0", "%");
                        //FormatDisplayStat(ref desc3, "RateOfFire", "ROF:", rof, "0", "%");

                        item.DisplayStat1 = FormatDisplayStat(nameof(item.Recoil), Resources.lbl_Recoil, recoil, StatsEnum.Normal, "0", "%");
                    }
                    break;
                case SHOP_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        if (item is null) continue;
                        item.Category = itemCategory.Key;
                        item.DisplayStat1 = FormatDisplayStat(nameof(item.CP), Resources.lbl_CP, item.CP, StatsEnum.None, "0");
                    }
                    break;
                default:
                    foreach (var item in itemCategory.Value)
                    {
                        if (item is null) continue;
                        item.Category = itemCategory.Key;
                    }
                    break;
            }

            if (CollectionViewSource.GetDefaultView(itemCategory.Value) is CollectionView view)
            {
                view.Filter += new Predicate<object>(ItemFilters.FullFilter);
            }
        }
    }

    internal static void UpdateArmorImages(bool female)
    {
        foreach (var upper in ItemLists[UPPER_BODIES_CATEGORY])
        {
            upper.UpdateImage(female);
        }
        foreach (var lower in ItemLists[LOWER_BODIES_CATEGORY])
        {
            lower.UpdateImage(female);
        }
    }

    static readonly Brush grey = new SolidColorBrush(Color.FromArgb(136, 136, 136, 136));

    static readonly Brush defaultGreen = new SolidColorBrush(Color.FromArgb(255, 110, 175, 125));
    static readonly Brush highlightGreen = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));

    static readonly Brush defaultRed = new SolidColorBrush(Color.FromArgb(255, 200, 60, 50));
    static readonly Brush highlightRed = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
    private static DisplayStatDiscriptor? FormatDisplayStat(string propertyName, string description, object value, StatsEnum type, string format, string suffix = "", string prefix = "", int count = -1, double defaultval = 0)
    {
        if (string.IsNullOrEmpty(description)) return null;
        
        DisplayStatDiscriptor desc = new()
        {
            PropertyName = propertyName,
            Description = description
        };

        bool isGrey = false;
        bool isPositive = false;

        switch (value)
        {
            case double d:
                desc.Value = prefix + d.ToString(format) + suffix;
                isGrey = d == 0;
                isPositive = d > defaultval;
                break;
            case long l:
                desc.Value = prefix + l.ToString(format) + suffix;
                isGrey = l == 0;
                isPositive = l > defaultval;
                break;
            case int i:
                desc.Value = prefix + i.ToString(format) + suffix;
                isGrey = i == 0;
                isPositive = i > defaultval;
                break;
            case bool b:
                desc.Value = b.ToString();
                isGrey = !b;
                isPositive = b;
                break;
            case double[] db:
                if (db.Length > 0)
                {
                    isGrey = db[0] == 0;
                    isPositive = db[0] > 0;
                    string vv = "";
                    int indexes;
                    if (count < 0)
                    {
                        indexes = db.Length;
                    }
                    else
                    {
                        indexes = count;
                    }
                    for (int i = 0; i < indexes; i++)
                    {
                        if (i > 0)
                        { vv += prefix; }
                        vv += db[i].ToString(format);
                    }
                    desc.Value = vv;
                }
                break;
        }

        if (type != StatsEnum.None)
        {
            if (type == StatsEnum.Inverted)
            {
                isPositive = !isPositive;
            }
            if (isPositive)
            {
                //Green
                desc.DefaultValueColor = defaultGreen;
                desc.HighlightValueColor = highlightGreen;
            }
            else
            {
                //Red
                desc.DefaultValueColor = defaultRed;
                desc.HighlightValueColor = highlightRed;
            }
        }

        if (isGrey)
        {
            desc.DefaultDescriptionColor = grey;
            desc.DefaultValueColor = grey;
        }

        return desc;
    }

    private static void UpdateImages()
    {
        Parallel.ForEach(ItemLists, (entry) =>
        {
            foreach (var item in entry.Value)
            {
                item.LoadImage();
            }
        });
    }

    public static ObservableCollection<BLRItem>? GetItemListOfType(string Type)
    {
        if (string.IsNullOrEmpty(Type)) return null;
        if (ItemLists.TryGetValue(Type, out ObservableCollection<BLRItem> items))
        {
            return items;
        }
        else
        {
            return null;
        }
    }

    public static BLRItem[]? GetItemArrayOfType(string Type)
    {
        if (string.IsNullOrEmpty(Type)) return null;
        if (ItemLists.TryGetValue(Type, out ObservableCollection<BLRItem> items))
        {
            BLRItem[] array = new BLRItem[items.Count];
            items.CopyTo(array, 0);
            return array;
        }
        else
        {
            return null;
        }
    }

    public static int GetIDOfItem(BLRItem? item)
    {
        if (item is null) return -1;
        if (ItemLists.TryGetValue(item.Category, out ObservableCollection<BLRItem> items))
        {
            return items.IndexOf(item);
        }
        else
        {
            return -1;
        }
    }

    public static BLRItem? GetItemByIDAndType(string Type, int ID)
    {
        if (ID < 0 || string.IsNullOrEmpty(Type)) return null;
        if (ItemLists.TryGetValue(Type, out ObservableCollection<BLRItem> items))
        {
            if (ID < items.Count)
            {
                return items[ID];
            }
            else
            {
                return null;
            }

        }
        else
        {
            return null;
        }
    }

    public static BLRItem? GetItemByLMIDAndType(string Type, int LMID)
    {
        if (ItemLists.TryGetValue(Type, out ObservableCollection<BLRItem> items))
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].LMID == LMID) return items[i];
                if (items[i].LMID == -69 && i == LMID) return items[i];
            }
        }
        return null;
    }

    public static BLRItem? GetItemByUIDAndType(string Type, int UID)
    {
        if (string.IsNullOrEmpty(Type)) return null;
        if (ItemLists.TryGetValue(Type, out ObservableCollection<BLRItem> items))
        {
            foreach (var item in items)
            {
                if (item.UID == UID)
                {
                    return item;
                }
            }
            return null;
        }
        else
        {
            return null;
        }
    }

    public static int GetIDByNameAndType(string? Type, string? Name)
    {
        if (Type is null || Name is null || string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(Name)) return -1;
        if (ItemLists.TryGetValue(Type, out ObservableCollection<BLRItem> items))
        {
            foreach (BLRItem item in items)
            {
                if (item.Name == Name)
                {
                    return items.IndexOf(item);
                }
            }
            return -1;
        }
        else
        {
            return -1;
        }
    }

    public static BLRItem? GetItemByNameAndType(string? Type, string? Name)
    {
        if (Type is null || Name is null || string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(Name)) return null;
        if (ItemLists.TryGetValue(Type, out ObservableCollection<BLRItem> items))
        {
            foreach (BLRItem item in items)
            {
                if (item.Name == Name)
                {
                    return item;
                }
            }
            return null;
        }
        else
        {
            return null;
        }
    }

    private static void LoadAllScopePreviews()
    {
        foreach (var icon in Directory.EnumerateFiles($"Assets\\crosshairs"))
        {
            ScopePreviews.Add(new FoxIcon(icon));
        }
    }
}