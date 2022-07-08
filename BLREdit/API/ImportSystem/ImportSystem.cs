using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BLREdit
{

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

        //public static readonly Dictionary<float, float> DamagePercentToValue = new Dictionary<float, float>(); not in use

        public static readonly FoxIcon[] Icons = LoadAllIcons();
        public static readonly FoxIcon[] ScopePreviews = LoadAllScopePreviews();

        private static Dictionary<string, List<BLRItem>> ItemLists { get; set; } = new Dictionary<string, List<BLRItem>>();

        public static void Initialize()
        {
            System.Diagnostics.Stopwatch watch = null;
            if (LoggingSystem.IsDebuggingEnabled) watch = LoggingSystem.LogInfo("Initializing Import System");

            ItemLists = IOResources.DeserializeFile<Dictionary<string, List<BLRItem>>>(IOResources.ITEM_LIST_FILE);
            
            UpdateImages();
            ApplyDisplayStats();

            //UnifyItemList();

            if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfoAppend(watch, "Import System");
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
                            double[] damage = UI.MainWindow.CalculateDamage(item, 0);
                            double[] spread = UI.MainWindow.CalculateSpread(item, 0, 0);
                            double recoil = UI.MainWindow.CalculateRecoil(item, 0);
                            double[] range = UI.MainWindow.CalculateRange(item, 0);

                            var desc1 = new DisplayStatDiscriptor();
                            var desc2 = new DisplayStatDiscriptor();
                            var desc3 = new DisplayStatDiscriptor();
                            var desc4 = new DisplayStatDiscriptor();
                            var desc5 = new DisplayStatDiscriptor();
                            var desc6 = new DisplayStatDiscriptor();

                            FormatDisplayStat(ref desc1, "Damage", "⚔:", damage, "0", "", "/");
                            FormatDisplayStat(ref desc2, "Aim", "🔎🎯:", spread[0], "0.00", "°");
                            FormatDisplayStat(ref desc3, "Hip", "🧎‍🎯:", spread[1], "0.00", "°");
                            FormatDisplayStat(ref desc4, "Move", "🚶‍🎯:", spread[2], "0.00", "°");
                            FormatDisplayStat(ref desc5, "Recoil", "💨:", recoil, "0.00", "°");
                            FormatDisplayStat(ref desc6, "Range", "📏:", range, "0", "", "/", 2);

                            item.DisplayStat1 = desc1;
                            item.DisplayStat2 = desc2;
                            item.DisplayStat3 = desc3;
                            item.DisplayStat4 = desc4;
                            item.DisplayStat5 = desc5;
                            item.DisplayStat6 = desc6;
                        }
                        break;
                    case MUZZELS_CATEGORY:
                    case STOCKS_CATEGORY:
                    case BARRELS_CATEGORY:
                        foreach (var item in itemCategory.Value)
                        {
                            double damage = item?.WeaponModifiers?.damage ?? 0;
                            double spread = item?.WeaponModifiers?.accuracy ?? 0;
                            double recoil = item?.WeaponModifiers?.recoil ?? 0;
                            double range = item?.WeaponModifiers?.range ?? 0;
                            double run = item?.WeaponModifiers?.movementSpeed ?? 0;

                            var desc1 = new DisplayStatDiscriptor();
                            var desc2 = new DisplayStatDiscriptor();
                            var desc3 = new DisplayStatDiscriptor();
                            var desc4 = new DisplayStatDiscriptor();
                            var desc5 = new DisplayStatDiscriptor();
                            var desc6 = new DisplayStatDiscriptor();

                            FormatDisplayStat(ref desc1, "Damage", "⚔:", damage, "0", "%");
                            FormatDisplayStat(ref desc2, "Accuracy", "🎯:", spread, "0", "%");
                            FormatDisplayStat(ref desc3, "Recoil", "💨:", recoil, "0", "%");
                            FormatDisplayStat(ref desc4, "Range", "📏:", range, "0", "%");
                            FormatDisplayStat(ref desc5, "Run", "🏃:", run, "0", "%");

                            item.DisplayStat1 = desc1;
                            item.DisplayStat2 = desc2;
                            item.DisplayStat3 = desc3;
                            item.DisplayStat4 = desc4;
                            item.DisplayStat5 = desc5;
                            item.DisplayStat6 = desc6;
                        }
                        break;
                    case SCOPES_CATEGORY:
                        foreach (var item in itemCategory.Value)
                        {
                            var desc1 = new DisplayStatDiscriptor();
                            var desc2 = new DisplayStatDiscriptor();
                            var desc3 = new DisplayStatDiscriptor();
                            var desc4 = new DisplayStatDiscriptor();
                            var desc5 = new DisplayStatDiscriptor();
                            var desc6 = new DisplayStatDiscriptor();

                            FormatDisplayStat(ref desc1, "Zoom", "Zoom:", (1.3 + (item?.WikiStats?.zoom ?? 0)), "0.00", "x");
                            FormatDisplayStat(ref desc2, "ScopeInTime", "Scope In:", (0.0 + (item?.WikiStats?.scopeInTime ?? 0)), "0.00", "s", "+");
                            FormatDisplayStat(ref desc3, "Infrared", "Is Infrared:", item.UID == 45019 || item.UID == 45020 || item.UID == 45021, "");

                            item.DisplayStat1 = desc1;
                            item.DisplayStat2 = desc2;
                            item.DisplayStat3 = desc3;
                            item.DisplayStat4 = desc4;
                            item.DisplayStat5 = desc5;
                            item.DisplayStat6 = desc6;
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

                            var desc1 = new DisplayStatDiscriptor();
                            var desc2 = new DisplayStatDiscriptor();
                            var desc3 = new DisplayStatDiscriptor();
                            var desc4 = new DisplayStatDiscriptor();
                            var desc5 = new DisplayStatDiscriptor();
                            var desc6 = new DisplayStatDiscriptor();

                            FormatDisplayStat(ref desc1, "Ammo", "🔋:", ammo, "0");
                            FormatDisplayStat(ref desc2, "Damage", "⚔:", damage, "0", "%");
                            FormatDisplayStat(ref desc3, "Run", "🏃:", movementSpeed, "0", "%");
                            FormatDisplayStat(ref desc4, "Recoil", "💨:", recoil, "0", "%");
                            FormatDisplayStat(ref desc5, "Range", "📏:", range, "0", "%");

                            if (item.IsValidForItemIDS(40021, 40002))
                            {
                                FormatDisplayStat(ref desc6, "Accuracy", "🎯:", accuracy, "0", "%");
                            }
                            else
                            {
                                FormatDisplayStat(ref desc6, "Reload", "⏱:", reload, "0.00", "s");
                            }

                            item.DisplayStat1 = desc1;
                            item.DisplayStat2 = desc2;
                            item.DisplayStat3 = desc3;
                            item.DisplayStat4 = desc4;
                            item.DisplayStat5 = desc5;
                            item.DisplayStat6 = desc6;
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
                                    prop = "ToxicProtection";
                                    desc = "☣:";
                                    value = item.PawnModifiers.ToxicProtection;
                                    break;
                                case "Prex Chem/Hazmat Respirator-INC":
                                    prop = "IncendiaryProtection";
                                    desc = "🔥:";
                                    value = item.PawnModifiers.IncendiaryProtection;
                                    break;
                                case "Prex Chem/Hazmat Respirator-XPL":
                                    prop = "ExplosiveProtection";
                                    desc = "💥:";
                                    value = item.PawnModifiers.ExplosiveProtection;
                                    break;
                            }

                            var desc1 = new DisplayStatDiscriptor();
                            var desc2 = new DisplayStatDiscriptor();
                            var desc3 = new DisplayStatDiscriptor();
                            var desc4 = new DisplayStatDiscriptor();
                            var desc5 = new DisplayStatDiscriptor();
                            var desc6 = new DisplayStatDiscriptor();

                            FormatDisplayStat(ref desc1, "Health", "❤:", health, "0");
                            FormatDisplayStat(ref desc2, "HeadProtection", "🔰:", dmgReduction, "0.0", "%");
                            FormatDisplayStat(ref desc3, "Run", "🏃‍:", movement, "0");
                            FormatDisplayStat(ref desc4, "HRVDuration", "⌚:", hrv, "0.0");
                            FormatDisplayStat(ref desc5, "HRVRecharge", "♻:", recharge, "0.0", "u/s");
                            if (value != 0)
                            {
                                FormatDisplayStat(ref desc6, prop, desc, value, "0", "%");
                            }
                            item.DisplayStat1 = desc1;
                            item.DisplayStat2 = desc2;
                            item.DisplayStat3 = desc3;
                            item.DisplayStat4 = desc4;
                            item.DisplayStat5 = desc5;
                            item.DisplayStat6 = desc6;
                        }
                        break;
                    case TACTICAL_CATEGORY:
                        foreach (var item in itemCategory.Value)
                        {
                            double hrv = item?.PawnModifiers?.HRVDuration ?? 0;
                            double recharge = item?.PawnModifiers?.HRVRechargeRate ?? 0;

                            var desc1 = new DisplayStatDiscriptor();
                            var desc2 = new DisplayStatDiscriptor();
                            var desc3 = new DisplayStatDiscriptor();
                            var desc4 = new DisplayStatDiscriptor();
                            var desc5 = new DisplayStatDiscriptor();
                            var desc6 = new DisplayStatDiscriptor();

                            FormatDisplayStat(ref desc1, "HRVDuration", "⌚:", hrv, "0.0");
                            FormatDisplayStat(ref desc2, "HRVRecharge", "♻:", recharge, "0.0", "u/s");

                            item.DisplayStat1 = desc1;
                            item.DisplayStat2 = desc2;
                            item.DisplayStat3 = desc3;
                            item.DisplayStat4 = desc4;
                            item.DisplayStat5 = desc5;
                            item.DisplayStat6 = desc6;
                        }
                        break;
                    case UPPER_BODIES_CATEGORY:
                    case LOWER_BODIES_CATEGORY:
                        foreach (var item in itemCategory.Value)
                        {
                            double health = item?.PawnModifiers?.Health ?? 0;
                            double movement = item?.PawnModifiers?.MovementSpeed ?? 0;
                            double gear = item?.PawnModifiers?.GearSlots ?? 0;

                            var desc1 = new DisplayStatDiscriptor();
                            var desc2 = new DisplayStatDiscriptor();
                            var desc3 = new DisplayStatDiscriptor();
                            var desc4 = new DisplayStatDiscriptor();
                            var desc5 = new DisplayStatDiscriptor();
                            var desc6 = new DisplayStatDiscriptor();


                            FormatDisplayStat(ref desc1, "Health", "❤:", health, "0");
                            FormatDisplayStat(ref desc2, "Run", "🏃:", movement, "0");
                            FormatDisplayStat(ref desc3, "Gear", "🧯:", gear, "0");

                            item.DisplayStat1 = desc1;
                            item.DisplayStat2 = desc2;
                            item.DisplayStat3 = desc3;
                            item.DisplayStat4 = desc4;
                            item.DisplayStat5 = desc5;
                            item.DisplayStat6 = desc6;
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

                            var desc1 = new DisplayStatDiscriptor();
                            var desc2 = new DisplayStatDiscriptor();
                            var desc3 = new DisplayStatDiscriptor();
                            var desc4 = new DisplayStatDiscriptor();
                            var desc5 = new DisplayStatDiscriptor();
                            var desc6 = new DisplayStatDiscriptor();

                            string prop = "";
                            string desc = "";
                            double value = 0;

                            bool isPatch = false;

                            switch (item.Name)
                            {
                                case "Incendiary Protection Gear":
                                    prop = "IncendiaryProtection";
                                    desc = "🔥:";
                                    value = IncendiaryProtection;
                                    isPatch = true;
                                    break;
                                case "Toxic Protection Gear":
                                    prop = "ToxicProtection";
                                    desc = "☣:";
                                    value = ToxicProtection;
                                    isPatch = true;
                                    break;
                                case "Explosive Protection Gear":
                                    prop = "ExplosiveProtection";
                                    desc = "💥:";
                                    value = ExplosiveProtection;
                                    isPatch = true;
                                    break;
                                case "Electro Protection Gear":
                                    prop = "ElectroProtection";
                                    desc = "⚡:";
                                    value = ElectroProtection;
                                    isPatch = true;
                                    break;
                                case "Melee Protection Gear":
                                    prop = "MeleeProtection";
                                    desc = "🤜:";
                                    value = MeleeProtection;
                                    isPatch = true;
                                    break;
                                case "Infrared Protection Gear":
                                    prop = "InfraredProtection";
                                    desc = "🌀:";
                                    value = InfraredProtection;
                                    isPatch = true;
                                    break;
                            }
                            if(isPatch)FormatDisplayStat(ref desc1, prop, desc, value, "0", "%");

                            item.DisplayStat1 = desc1;
                            item.DisplayStat2 = desc2;
                            item.DisplayStat3 = desc3;
                            item.DisplayStat4 = desc4;
                            item.DisplayStat5 = desc5;
                            item.DisplayStat6 = desc6;
                        }
                        break;
                    case GRIPS_CATEGORY:
                        foreach (var item in itemCategory.Value)
                        {
                            double damage = item?.WeaponModifiers?.damage ?? 0;
                            double rof = item?.WeaponModifiers?.rateOfFire ?? 0;
                            double recoil = item?.WeaponModifiers?.recoil ?? 0;

                            var desc1 = new DisplayStatDiscriptor();
                            var desc2 = new DisplayStatDiscriptor();
                            var desc3 = new DisplayStatDiscriptor();
                            var desc4 = new DisplayStatDiscriptor();
                            var desc5 = new DisplayStatDiscriptor();
                            var desc6 = new DisplayStatDiscriptor();

                            //FormatDisplayStat(ref desc1, "Damage", "Damage:", damage, "0", "%");
                            FormatDisplayStat(ref desc2, "Recoil", "💨:", recoil, "0", "%");
                            //FormatDisplayStat(ref desc3, "RateOfFire", "ROF:", rof, "0", "%");

                            item.DisplayStat1 = desc1;
                            item.DisplayStat2 = desc2;
                            item.DisplayStat3 = desc3;
                            item.DisplayStat4 = desc4;
                            item.DisplayStat5 = desc5;
                            item.DisplayStat6 = desc6;
                        }
                        break;
                }
            }
        }
        static readonly Brush grey = new SolidColorBrush(Color.FromArgb(136, 136, 136, 136));
        private static void FormatDisplayStat(ref DisplayStatDiscriptor desc, string propertyName, string description, object value, string format, string suffix = "", string prefix = "", int count = -1)
        { 
            desc.PropertyName = propertyName;
            desc.Description = description;

            bool isGrey = false;

            switch (value)
            {
                case double d:
                    desc.Value = prefix + d.ToString(format) + suffix;
                    isGrey = d == 0;
                    break;
                case bool b:
                    desc.Value = b.ToString();
                    isGrey = !b;
                    break;
                case double[] db:
                    if (db.Length > 0)
                    {
                        isGrey = db[0] == 0;
                        string vv = "";
                        int indexes = 0;
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

            if (isGrey)
            {
                desc.DefaultDescriptionColor = grey;
                desc.DefaultValueColor = grey;
            }
        }

        private static void UnifyItemList()
        {
            ImportGear Gear = IOResources.DeserializeFile<ImportGear>(IOResources.GEAR_FILE);
            ImportMods Mods = IOResources.DeserializeFile<ImportMods>(IOResources.MOD_FILE);
            ImportWeapons Weapons = IOResources.DeserializeFile<ImportWeapons>(IOResources.WEAPON_FILE);

            Dictionary<string, List<ImportItem>> ListItems = new()
            {
                { nameof(Gear.attachments), Gear.attachments },
                { nameof(Gear.avatars), Gear.avatars },
                { nameof(Gear.badges), Gear.badges },
                { nameof(Gear.emotes), Gear.emotes },
                { nameof(Gear.hangers), Gear.hangers },
                { nameof(Gear.helmets), Gear.helmets },
                { nameof(Gear.lowerBodies), Gear.lowerBodies },
                { nameof(Gear.tactical), Gear.tactical },
                { nameof(Gear.upperBodies), Gear.upperBodies },

                { nameof(Mods.ammo), Mods.ammo },
                { nameof(Mods.ammos), Mods.ammos },
                { nameof(Mods.barrels), Mods.barrels },
                { nameof(Mods.camosBody), Mods.camosBody },
                { nameof(Mods.camosWeapon), Mods.camosWeapon },
                { nameof(Mods.grips), Mods.grips },
                { nameof(Mods.magazines), Mods.magazines },
                { nameof(Mods.muzzles), Mods.muzzles },
                { nameof(Mods.primarySkins), Mods.primarySkins },
                { nameof(Mods.scopes), Mods.scopes },
                { nameof(Mods.stocks), Mods.stocks },

                { nameof(Weapons.depot), Weapons.depot },
                { nameof(Weapons.primary), Weapons.primary },
                { nameof(Weapons.secondary), Weapons.secondary }
            };

            CleanItems(ListItems);
            LoadWikiStats(ListItems);
            LoadIniStats(ListItems);

            Dictionary<string, List<BLRItem>> Items = new();

            foreach (var itemList in ListItems)
            {
                List<BLRItem> items = new();
                foreach (var item in itemList.Value)
                {
                    items.Add(new BLRItem(item));
                }
                Items.Add(itemList.Key, items);
            }

            IOResources.SerializeFile("itemList.json", Items);
        }

        private static void CleanItems(Dictionary<string, List<ImportItem>> ListItems)
        {

            foreach (var entry in ListItems)
            {
                System.Diagnostics.Stopwatch watch = null;
                if (LoggingSystem.IsDebuggingEnabled) watch = LoggingSystem.LogInfo("Started Cleaning " + entry.Key, "");
                List<ImportItem> ToRemove = new();
                foreach (ImportItem item in entry.Value)
                {
                    if (IsValidItem(item))
                    {
                        item.Category = entry.Key;
                    }
                    else
                    {
                        ToRemove.Add(item);
                    }

                }

                foreach (ImportItem item in ToRemove)
                {
                    entry.Value.Remove(item);
                }

                if (entry.Key == AVATARS_CATEGORY)
                {
                    entry.Value.Add(new ImportItem() { name = "No Avatar", Category = AVATARS_CATEGORY });
                }
                if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfoAppend(watch, " " + ToRemove.Count + " have been Removed");
            }
        }

        private static void UpdateImages()
        {
            foreach (KeyValuePair<string, List<BLRItem>> entry in ItemLists)
            {
                System.Diagnostics.Stopwatch watch = null;
                if (LoggingSystem.IsDebuggingEnabled) watch = LoggingSystem.LogInfo("Updating Images for " + entry.Key, "");
                Parallel.ForEach(entry.Value, item =>
                {
                    item.LoadImage();
                    item.wideImageMale.Freeze();
                    item.wideImageFemale?.Freeze();
                    item.largeSquareImageMale.Freeze();
                    item.largeSquareImageFemale?.Freeze();
                    item.smallSquareImageMale.Freeze();
                    item.smallSquareImageFemale?.Freeze();
                });
                if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfoAppend(watch);
            }
        }

        private static void LoadWikiStats(Dictionary<string, List<ImportItem>> ListItems)
        {
            AssignWikiStats(LoadWikiStatsFromCSV(), ListItems);
        }

        private static WikiStats[] LoadWikiStatsFromCSV()
        {
            List<WikiStats> stats = new();
            StreamReader sr = new(IOResources.ASSET_DIR + "\\BLR Wiki Stats.csv");
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split(',');
                stats.Add(new WikiStats()
                {
                    itemName = parts[0],
                    itemID = int.Parse(parts[1]),
                    damage = float.Parse(parts[2], CultureInfo.InvariantCulture),
                    firerate = float.Parse(parts[3], CultureInfo.InvariantCulture),
                    ammoMag = float.Parse(parts[4], CultureInfo.InvariantCulture),
                    ammoReserve = float.Parse(parts[5], CultureInfo.InvariantCulture),
                    reload = float.Parse(parts[6], CultureInfo.InvariantCulture),
                    swaprate = float.Parse(parts[7], CultureInfo.InvariantCulture),
                    aimSpread = float.Parse(parts[8], CultureInfo.InvariantCulture),
                    hipSpread = float.Parse(parts[9], CultureInfo.InvariantCulture),
                    moveSpread = float.Parse(parts[10], CultureInfo.InvariantCulture),
                    recoil = float.Parse(parts[11], CultureInfo.InvariantCulture),
                    zoom = float.Parse(parts[12], CultureInfo.InvariantCulture),
                    scopeInTime = float.Parse(parts[13], CultureInfo.InvariantCulture),
                    rangeClose = float.Parse(parts[14], CultureInfo.InvariantCulture),
                    rangeFar = float.Parse(parts[15], CultureInfo.InvariantCulture),
                    run = float.Parse(parts[16], CultureInfo.InvariantCulture)
                });
            }
            sr.Close();
            return stats.ToArray();
        }
        private static void AssignWikiStats(WikiStats[] stats, Dictionary<string, List<ImportItem>> ListItems)
        {
            foreach (var entry in ListItems)
            {
                foreach (ImportItem item in entry.Value)
                {
                    bool found = false;
                    foreach (WikiStats stat in stats)
                    {
                        if (!found && stat.itemID == item.uid)
                        {
                            item.WikiStats = new WikiStats()
                            {
                                itemID = item.uid,
                                itemName = item.name,
                                aimSpread = stat.aimSpread,
                                ammoMag = stat.ammoMag,
                                ammoReserve = stat.ammoReserve,
                                damage = stat.damage,
                                firerate = stat.firerate,
                                hipSpread = stat.hipSpread,
                                moveSpread = stat.moveSpread,
                                rangeClose = stat.rangeClose,
                                rangeFar = stat.rangeFar,
                                recoil = stat.recoil,
                                reload = stat.reload,
                                run = stat.run,
                                scopeInTime = stat.scopeInTime,
                                swaprate = stat.swaprate,
                                zoom = stat.zoom
                            };
                            found = true;
                        }
                    }
                }
            }
        }

        private static void LoadIniStats(Dictionary<string, List<ImportItem>> ListItems)
        {
            var iniStats = IOResources.DeserializeFile<IniStats[]>(IOResources.ASSET_DIR + "\\filteredIniStats.json");

            ListItems.TryGetValue(PRIMARY_CATEGORY, out List<ImportItem> primary);
            ListItems.TryGetValue(SECONDARY_CATEGORY, out List<ImportItem> secondary);

            AssignIniStatsTo(primary, iniStats);
            AssignIniStatsTo(secondary, iniStats);
        }

        internal static void AssignIniStatsTo(List<ImportItem> items, IniStats[] stats)
        {
            foreach (ImportItem item in items)
            {
                bool found = false;
                foreach (IniStats stat in stats)
                {
                    if (stat.ItemID == item.uid)
                    {
                        item.IniStats = stat;
                        found = true;
                    }
                }
                if (!found)
                {
                    if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("No IniStats for " + item.name);
                }
            }
        }

        public static List<BLRItem> GetItemListOfType(string Type)
        {
            if (string.IsNullOrEmpty(Type)) return null;
            if (ItemLists.TryGetValue(Type, out List<BLRItem> items))
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
            if (ItemLists.TryGetValue(Type, out List<BLRItem> items))
            {
                return items.ToArray();
            }
            else
            {
                return null;
            }
        }

        public static int GetIDOfItem(BLRItem item)
        {
            if (item == null) return -1;
            if (ItemLists.TryGetValue(item.Category, out List<BLRItem> items))
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
            if (ItemLists.TryGetValue(Type, out List<BLRItem> items))
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
            if(string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(Name)) return -1;
            if (ItemLists.TryGetValue(Type, out List<BLRItem> items))
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
            if (ItemLists.TryGetValue(Type, out List<BLRItem> items))
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
            System.Diagnostics.Stopwatch watch = null;
            if (LoggingSystem.IsDebuggingEnabled) watch = LoggingSystem.LogInfo("Loading All Icons", "");
            var icons = new List<FoxIcon>();
            foreach (var icon in Directory.GetFiles("Assets\\textures"))
            {
                if (icon.StartsWith("\\"))
                {
                    icons.Add(new FoxIcon(icon));
                }
                else
                {
                    icons.Add(new FoxIcon("\\" + icon));
                }
            }

            if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfoAppend(watch);
            return icons.ToArray();
        }
        private static FoxIcon[] LoadAllScopePreviews()
        {
            System.Diagnostics.Stopwatch watch = null;
            if (LoggingSystem.IsDebuggingEnabled) watch = LoggingSystem.LogInfo("Loading All Crosshairs", "");
            var icons = new List<FoxIcon>();
            foreach (var icon in Directory.EnumerateFiles("Assets\\crosshairs"))
            {
                icons.Add(new FoxIcon(icon));
            }
            if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfoAppend(watch);
            return icons.ToArray();
        }


        public static bool IsValidItem(ImportItem item)
        {
            return item.tooltip != "SHOULDN'T BE USED" && !string.IsNullOrEmpty(item.name);
        }
    }
}