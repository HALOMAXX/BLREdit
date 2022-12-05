using BLREdit.UI;
using BLREdit.UI.Views;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

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
    public const string DEPOT_CATEGORY = "depot";
    public const string EMOTES_CATEGORY = "emotes";
    public const string SHOP_CATEGORY = "shop";

    public static readonly FoxIcon[] Icons = LoadAllIcons();
    public static readonly FoxIcon[] ScopePreviews = LoadAllScopePreviews();

    private static Dictionary<string, ObservableCollection<BLRItem>> ItemLists { get; set; } = new();


    public static void Initialize()
    {
        LoggingSystem.Log("Initializing Import System");
        ItemLists = IOResources.DeserializeFile<Dictionary<string, ObservableCollection<BLRItem>>>($"{IOResources.ASSET_DIR}{IOResources.JSON_DIR}{IOResources.ITEM_LIST_FILE}");
        UpdateImages();
        ApplyDisplayStats();
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
                        var (DamageIdeal, DamageMax) = BLRWeapon.CalculateDamage(item, 0);
                        var (ZoomSpread, HipSpread, MovmentSpread) = BLRWeapon.CalculateSpread(item, 0, 0);
                        var (RecoilHip, _RecoilZoom) = BLRWeapon.CalculateRecoil(item, 0);
                        var (IdealRange, MaxRange, _TracerRange) = BLRWeapon.CalculateRange(item, 0);

                        item.DisplayStat1 = FormatDisplayStat(LanguageKeys.DAMAGE, LanguageSet.GetWord(LanguageKeys.DAMAGE) + ':', new double[] { DamageIdeal, DamageMax }, StatsEnum.None, "0", "", "/");
                        item.DisplayStat2 = FormatDisplayStat(LanguageKeys.AIM, LanguageSet.GetWord(LanguageKeys.AIM) + ':', ZoomSpread, StatsEnum.None, "0.00", "°");
                        item.DisplayStat3 = FormatDisplayStat(LanguageKeys.HIP, LanguageSet.GetWord(LanguageKeys.HIP) + ':', HipSpread, StatsEnum.None, "0.00", "°");
                        item.DisplayStat4 = FormatDisplayStat(LanguageKeys.MOVE, LanguageSet.GetWord(LanguageKeys.MOVE) + ':', MovmentSpread, StatsEnum.None, "0.00", "°");
                        item.DisplayStat5 = FormatDisplayStat(LanguageKeys.RECOIL, LanguageSet.GetWord(LanguageKeys.RECOIL) + ':', RecoilHip, StatsEnum.None, "0.00", "°");
                        item.DisplayStat6 = FormatDisplayStat(LanguageKeys.RANGE, LanguageSet.GetWord(LanguageKeys.RANGE) + ':', new double[] { IdealRange, MaxRange }, StatsEnum.None, "0", "", "/", 2);
                    }
                    break;
                case MUZZELS_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        double damage = item?.WeaponModifiers?.damage ?? 0;
                        double spread = item?.WeaponModifiers?.accuracy ?? 0;
                        double recoil = item?.WeaponModifiers?.recoil ?? 0;
                        double range = item?.WeaponModifiers?.range ?? 0;
                        double run = item?.WeaponModifiers?.movementSpeed ?? 0;

                        item.DisplayStat1 = FormatDisplayStat(LanguageKeys.DAMAGE, LanguageSet.GetWord(LanguageKeys.DAMAGE) + ':', damage, StatsEnum.Normal, "0", "%");
                        item.DisplayStat2 = FormatDisplayStat(LanguageKeys.ACCURACY, LanguageSet.GetWord(LanguageKeys.ACCURACY) + ':', spread, StatsEnum.Normal, "0", "%");
                        item.DisplayStat3 = FormatDisplayStat(LanguageKeys.RECOIL, LanguageSet.GetWord(LanguageKeys.RECOIL) + ':', recoil, StatsEnum.Normal, "0", "%");
                        item.DisplayStat4 = FormatDisplayStat(LanguageKeys.RANGE, LanguageSet.GetWord(LanguageKeys.RANGE) + ':', range, StatsEnum.Normal, "0", "%");
                        item.DisplayStat5 = FormatDisplayStat(LanguageKeys.RUN, LanguageSet.GetWord(LanguageKeys.RUN) + ':', run, StatsEnum.Normal, "0", "%");
                    }
                    break;
                case STOCKS_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        double damage = item?.WeaponModifiers?.damage ?? 0;
                        double spread = item?.WeaponModifiers?.accuracy ?? 0;
                        double recoil = item?.WeaponModifiers?.recoil ?? 0;
                        double range = item?.WeaponModifiers?.range ?? 0;
                        double run = item?.WeaponModifiers?.movementSpeed ?? 0;
                        double reload = item?.WeaponModifiers?.reloadSpeed ?? 0;

                        item.DisplayStat1 = FormatDisplayStat(LanguageKeys.DAMAGE, LanguageSet.GetWord(LanguageKeys.DAMAGE) + ':', damage, StatsEnum.Normal, "0", "%");
                        item.DisplayStat2 = FormatDisplayStat(LanguageKeys.ACCURACY, LanguageSet.GetWord(LanguageKeys.ACCURACY) + ':', spread, StatsEnum.Normal, "0", "%");
                        item.DisplayStat3 = FormatDisplayStat(LanguageKeys.RECOIL, LanguageSet.GetWord(LanguageKeys.RECOIL) + ':', recoil, StatsEnum.Normal, "0", "%");
                        item.DisplayStat4 = FormatDisplayStat(LanguageKeys.RANGE, LanguageSet.GetWord(LanguageKeys.RANGE) + ':', range, StatsEnum.Normal, "0", "%");
                        item.DisplayStat5 = FormatDisplayStat(LanguageKeys.RUN, LanguageSet.GetWord(LanguageKeys.RUN) + ':', run, StatsEnum.Normal, "0", "%");
                        //FormatDisplayStat(ref desc6, LanguageKeys.RELOAD, LanguageSet.GetWord(LanguageKeys.RELOAD) + ':', reload, StatsEnum.Normal, "0", "%");

                        if (item.IsValidForItemIDS(40020))
                        { item.DisplayStat6 = FormatDisplayStat(LanguageKeys.RELOAD, LanguageSet.GetWord(LanguageKeys.RELOAD) + ':', reload, StatsEnum.Normal, "0", "%"); }
                    }
                    break;
                case BARRELS_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        double damage = item?.WeaponModifiers?.damage ?? 0;
                        double spread = item?.WeaponModifiers?.accuracy ?? 0;
                        double recoil = item?.WeaponModifiers?.recoil ?? 0;
                        double range = item?.WeaponModifiers?.range ?? 0;
                        double run = item?.WeaponModifiers?.movementSpeed ?? 0;

                        item.DisplayStat1 = FormatDisplayStat(LanguageKeys.DAMAGE, LanguageSet.GetWord(LanguageKeys.DAMAGE) + ':', damage, StatsEnum.Normal, "0", "%");
                        item.DisplayStat2 = FormatDisplayStat(LanguageKeys.ACCURACY, LanguageSet.GetWord(LanguageKeys.ACCURACY) + ':', spread, StatsEnum.Normal, "0", "%");
                        item.DisplayStat3 = FormatDisplayStat(LanguageKeys.RECOIL, LanguageSet.GetWord(LanguageKeys.RECOIL) + ':', recoil, StatsEnum.Normal, "0", "%");
                        item.DisplayStat4 = FormatDisplayStat(LanguageKeys.RANGE, LanguageSet.GetWord(LanguageKeys.RANGE) + ':', range, StatsEnum.Normal, "0", "%");
                        item.DisplayStat5 = FormatDisplayStat(LanguageKeys.RUN, LanguageSet.GetWord(LanguageKeys.RUN) + ':', run, StatsEnum.Normal, "0", "%");
                    }
                    break;
                case SCOPES_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        item.DisplayStat1 = FormatDisplayStat(LanguageKeys.ZOOM, LanguageSet.GetWord(LanguageKeys.ZOOM) + ':', (1.3 + (item?.WikiStats?.zoom ?? 0)), StatsEnum.Normal, "0.00", "x");
                        item.DisplayStat2 = FormatDisplayStat(LanguageKeys.SCOPE_IN_TIME, LanguageSet.GetWord(LanguageKeys.SCOPE_IN_TIME) + ':', (0.0 + (item?.WikiStats?.scopeInTime ?? 0)), StatsEnum.Normal, "0.00", "s", "+");
                        item.DisplayStat3 = FormatDisplayStat(LanguageKeys.INFRARED, LanguageSet.GetWord(LanguageKeys.INFRARED) + ':', item.UID == 45019 || item.UID == 45020 || item.UID == 45021, StatsEnum.Normal, "");
                    }
                    break;
                case MAGAZINES_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        double ammo = item?.WeaponModifiers?.ammo ?? 0;
                        double range = item?.WeaponModifiers?.range ?? 0;
                        double reload = item?.WikiStats?.reload ?? 0;
                        double movementSpeed = item?.WeaponModifiers?.movementSpeed ?? 0;
                        double damage = item?.WeaponModifiers?.damage ?? 0;
                        double recoil = item?.WeaponModifiers?.recoil ?? 0;
                        double accuracy = item?.WeaponModifiers?.accuracy ?? 0;

                        item.DisplayStat1 = FormatDisplayStat(LanguageKeys.AMMO, LanguageSet.GetWord(LanguageKeys.AMMO) + ':', ammo, StatsEnum.Normal, "0");
                        item.DisplayStat2 = FormatDisplayStat(LanguageKeys.DAMAGE, LanguageSet.GetWord(LanguageKeys.DAMAGE) + ':', damage, StatsEnum.Normal, "0", "%");
                        item.DisplayStat3 = FormatDisplayStat(LanguageKeys.RUN, LanguageSet.GetWord(LanguageKeys.RUN) + ':', movementSpeed, StatsEnum.Normal, "0", "%");
                        item.DisplayStat4 = FormatDisplayStat(LanguageKeys.RECOIL, LanguageSet.GetWord(LanguageKeys.RECOIL) + ':', recoil, StatsEnum.Normal, "0", "%");
                        item.DisplayStat5 = FormatDisplayStat(LanguageKeys.RANGE, LanguageSet.GetWord(LanguageKeys.RANGE) + ':', range, StatsEnum.Normal, "0", "%");

                        if (item.IsValidForItemIDS(40021, 40002))
                        {
                            item.DisplayStat6 = FormatDisplayStat(LanguageKeys.ACCURACY, LanguageSet.GetWord(LanguageKeys.ACCURACY) + ':', accuracy, StatsEnum.Normal, "0", "%");
                        }
                        else
                        {
                            item.DisplayStat6 = FormatDisplayStat(LanguageKeys.RELOAD, LanguageSet.GetWord(LanguageKeys.RELOAD) + ':', reload, StatsEnum.Inverted, "0.00", "s");
                        }
                    }
                    break;
                case HELMETS_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        double health = item?.PawnModifiers?.Health ?? 0;
                        double dmgReduction = item?.PawnModifiers?.HelmetDamageReduction ?? 0;
                        double movement = item?.PawnModifiers?.MovementSpeed ?? 0;
                        double hrv = item?.PawnModifiers?.HRVDuration ?? 0;
                        double recharge = item?.PawnModifiers?.HRVRechargeRate ?? 0;

                        string prop = "";
                        string desc = "";
                        double value = 0;

                        switch (item.Name)
                        {
                            case "Prex Chem/Hazmat Respirator-TOX":
                                prop = LanguageKeys.TOXIC_PROTECTION;
                                desc = LanguageSet.GetWord(LanguageKeys.TOXIC_PROTECTION) + ':';
                                value = item.PawnModifiers.ToxicProtection;
                                break;
                            case "Prex Chem/Hazmat Respirator-INC":
                                prop = LanguageKeys.INCENDIARY_PROTECTION;
                                desc = LanguageSet.GetWord(LanguageKeys.INCENDIARY_PROTECTION) + ':';
                                value = item.PawnModifiers.IncendiaryProtection;
                                break;
                            case "Prex Chem/Hazmat Respirator-XPL":
                                prop = LanguageKeys.EXPLOSIVE_PROTECTION;
                                desc = LanguageSet.GetWord(LanguageKeys.EXPLOSIVE_PROTECTION) + ':';
                                value = item.PawnModifiers.ExplosiveProtection;
                                break;
                        }

                        item.DisplayStat1 = FormatDisplayStat(LanguageKeys.HEALTH, LanguageSet.GetWord(LanguageKeys.HEALTH) + ':', health, StatsEnum.Normal, "0", "%");
                        item.DisplayStat2 = FormatDisplayStat(LanguageKeys.HEAD_PROTECTION, LanguageSet.GetWord(LanguageKeys.HEAD_PROTECTION) + ':', dmgReduction, StatsEnum.Normal, "0.0", "%");
                        item.DisplayStat3 = FormatDisplayStat(LanguageKeys.RUN, LanguageSet.GetWord(LanguageKeys.RUN) + ':', movement, StatsEnum.Normal, "0", "%");
                        item.DisplayStat4 = FormatDisplayStat(LanguageKeys.HRV_DURATION, LanguageSet.GetWord(LanguageKeys.HRV_DURATION) + ':', hrv, StatsEnum.Normal, "0.0", "u", "", -1, 69.9);
                        item.DisplayStat5 = FormatDisplayStat(LanguageKeys.HRV_RECHARGE, LanguageSet.GetWord(LanguageKeys.HRV_RECHARGE) + ':', recharge, StatsEnum.Normal, "0.0", "u/s", "", -1, 6.59);
                        if (value != 0)
                        {
                            item.DisplayStat6 = FormatDisplayStat(prop, desc, value, StatsEnum.Normal, "0", "%");
                        }
                    }
                    break;
                case TACTICAL_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        double hrv = item?.PawnModifiers?.HRVDuration ?? 0;
                        double recharge = item?.PawnModifiers?.HRVRechargeRate ?? 0;

                        item.DisplayStat1 = FormatDisplayStat(LanguageKeys.HRV_DURATION, LanguageSet.GetWord(LanguageKeys.HRV_DURATION) + ':', hrv, StatsEnum.Normal, "0.0", "u", "", -1, 0);
                        item.DisplayStat2 = FormatDisplayStat(LanguageKeys.HRV_RECHARGE, LanguageSet.GetWord(LanguageKeys.HRV_RECHARGE) + ':', recharge, StatsEnum.Normal, "0.0", "u/s", "", -1, 0);
                    }
                    break;
                case UPPER_BODIES_CATEGORY:
                case LOWER_BODIES_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        double health = item?.PawnModifiers?.Health ?? 0;
                        double movement = item?.PawnModifiers?.MovementSpeed ?? 0;
                        double gear = item?.PawnModifiers?.GearSlots ?? 0;

                        item.DisplayStat1 = FormatDisplayStat(LanguageKeys.HEALTH, LanguageSet.GetWord(LanguageKeys.HEALTH) + ':', health, StatsEnum.Normal, "0", "%");
                        item.DisplayStat2 = FormatDisplayStat(LanguageKeys.RUN, LanguageSet.GetWord(LanguageKeys.RUN) + ':', movement, StatsEnum.Normal, "0", "%");
                        item.DisplayStat3 = FormatDisplayStat(LanguageKeys.GEAR_SLOTS, LanguageSet.GetWord(LanguageKeys.GEAR_SLOTS) + ':', gear, StatsEnum.Normal, "0");
                    }
                    break;
                case ATTACHMENTS_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        double ElectroProtection = item?.PawnModifiers?.ElectroProtection ?? 0;
                        double ExplosiveProtection = item?.PawnModifiers?.ExplosiveProtection ?? 0;
                        double IncendiaryProtection = item?.PawnModifiers?.IncendiaryProtection ?? 0;
                        double MeleeProtection = item?.PawnModifiers?.MeleeProtection ?? 0;
                        double ToxicProtection = item?.PawnModifiers?.ToxicProtection ?? 0;
                        double InfraredProtection = item?.PawnModifiers?.InfraredProtection ?? 0;

                        string prop = "";
                        string desc = "";
                        double value = 0;

                        bool isPatch = false;

                        switch (item.Name)
                        {
                            case "Incendiary Protection Gear":
                                prop = LanguageKeys.INCENDIARY_PROTECTION;
                                desc = LanguageSet.GetWord(LanguageKeys.INCENDIARY_PROTECTION) + ':';
                                value = IncendiaryProtection;
                                isPatch = true;
                                break;
                            case "Toxic Protection Gear":
                                prop = LanguageKeys.TOXIC_PROTECTION;
                                desc = LanguageSet.GetWord(LanguageKeys.TOXIC_PROTECTION) + ':';
                                value = ToxicProtection;
                                isPatch = true;
                                break;
                            case "Explosive Protection Gear":
                                prop = LanguageKeys.EXPLOSIVE_PROTECTION;
                                desc = LanguageSet.GetWord(LanguageKeys.EXPLOSIVE_PROTECTION) + ':';
                                value = ExplosiveProtection;
                                isPatch = true;
                                break;
                            case "Electro Protection Gear":
                                prop = LanguageKeys.ELECTRO_PROTECTION;
                                desc = LanguageSet.GetWord(LanguageKeys.ELECTRO_PROTECTION) + ':';
                                value = ElectroProtection;
                                isPatch = true;
                                break;
                            case "Melee Protection Gear":
                                prop = LanguageKeys.MELEE_PROTECTION;
                                desc = LanguageSet.GetWord(LanguageKeys.MELEE_PROTECTION) + ':';
                                value = MeleeProtection;
                                isPatch = true;
                                break;
                            case "Infrared Protection Gear":
                                prop = LanguageKeys.INFRARED_PROTECTION;
                                desc = LanguageSet.GetWord(LanguageKeys.INFRARED_PROTECTION) + ':';
                                value = InfraredProtection;
                                isPatch = true;
                                break;
                        }
                        item.DisplayStat1 = FormatDisplayStat(prop, desc, value, StatsEnum.Normal, "0", "%");
                    }
                    break;
                case GRIPS_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        double _damage = item?.WeaponModifiers?.damage ?? 0;
                        double _rof = item?.WeaponModifiers?.rateOfFire ?? 0;
                        double recoil = item?.WeaponModifiers?.recoil ?? 0;

                        //FormatDisplayStat(ref desc1, "Damage", "Damage:", damage, "0", "%");
                        //FormatDisplayStat(ref desc3, "RateOfFire", "ROF:", rof, "0", "%");

                        item.DisplayStat1 = FormatDisplayStat(LanguageKeys.RECOIL, LanguageSet.GetWord(LanguageKeys.RECOIL) + ':', recoil, StatsEnum.Normal, "0", "%");
                    }
                    break;
                case SHOP_CATEGORY:
                    foreach (var item in itemCategory.Value)
                    {
                        item.DisplayStat1 = FormatDisplayStat("CP", LanguageSet.GetWord("CP") + ':', item.CP, StatsEnum.None, "0");
                    }
                    break;
            }

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(itemCategory.Value);
            if (view != null)
            {
                view.Filter += new Predicate<object>(ItemFilters.FullFilter);
            }
        }
        if (ItemLists.TryGetValue(ImportSystem.EMOTES_CATEGORY, out ObservableCollection<BLRItem> list))
        {
            LoggingSystem.Log(list.Count.ToString());
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
    private static DisplayStatDiscriptor FormatDisplayStat(string propertyName, string description, object value, StatsEnum type, string format, string suffix = "", string prefix = "", int count = -1, double defaultval = 0)
    {
        DisplayStatDiscriptor desc = new()
        {
            PropertyName = propertyName,
            Description = description
        };

        bool isGrey = false;
        bool isPositive = false;

        if (!string.IsNullOrEmpty(description))
        {
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
        foreach (var entry in ItemLists)
        {
            LoggingSystem.Log($"Updating Images for {entry.Key}");

            foreach (var item in entry.Value)
            {
                item.LoadImage();
            }
        }
    }

    public static ObservableCollection<BLRItem> GetItemListOfType(string Type)
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

    public static BLRItem[] GetItemArrayOfType(string Type)
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

    public static int GetIDOfItem(BLRItem item)
    {
        if (item == null) return -1;
        if (ItemLists.TryGetValue(item.Category, out ObservableCollection<BLRItem> items))
        {
            return items.IndexOf(item);
        }
        else
        {
            return -1;
        }
    }

    public static BLRItem GetItemByIDAndType(string Type, int ID)
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

    public static int GetIDByNameAndType(string Type, string Name)
    {
        if (string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(Name)) return -1;
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

    public static BLRItem GetItemByNameAndType(string Type, string Name)
    {
        if (string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(Name)) return null;
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

    private static FoxIcon[] LoadAllIcons()
    {
        LoggingSystem.Log("Loading All Icons");
        var icons = new List<FoxIcon>();
        foreach (var icon in Directory.GetFiles($"Assets\\textures"))
        {
            icons.Add(new FoxIcon(icon));
        }
        return icons.ToArray();
    }

    private static FoxIcon[] LoadAllScopePreviews()
    {
        LoggingSystem.Log("Loading All Crosshairs");
        var icons = new List<FoxIcon>();
        foreach (var icon in Directory.EnumerateFiles($"Assets\\crosshairs"))
        {
            icons.Add(new FoxIcon(icon));
        }
        return icons.ToArray();
    }
}