using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace BLREdit
{

    public static class ImportSystem
    {
        public static readonly Dictionary<float, float> DamagePercentToValue = new Dictionary<float, float>();

        public static readonly FoxIcon[] Icons = LoadAllIcons();
        public static readonly FoxIcon[] Crosshairs = LoadAllCrosshairs();
        //private static readonly ImportGear importGear 
        //private static readonly ImportMods importMods 
        //private static readonly ImportWeapons importWeapons 

        private static WikiStats[] CorrectedItemStats;

        private static IniStats[] IniItemStats;

        public static ImportGear Gear { get; private set; } = IOResources.Deserialize<ImportGear>(IOResources.GEAR_FILE);
        public static ImportMods Mods { get; private set; } = IOResources.Deserialize<ImportMods>(IOResources.MOD_FILE);
        public static ImportWeapons Weapons { get; private set; } = IOResources.Deserialize<ImportWeapons>(IOResources.WEAPON_FILE);

        internal static void Initialize()
        {
            var watch = LoggingSystem.LogInfo("Initializing Import System");

            CleanItems();
            UpdateImages();
            LoadWikiStats();
            LoadIniStats();

            //UpgradeIniStats();

            LoggingSystem.LogInfo("BodyCamoCount:" + Mods.camosBody.Length);
            //LoggingSystem.LogInfo("WeaponCamoCount:" + Mods.camosWeapon.Length);

            LoggingSystem.LogInfoAppend(watch, "Import System");
        }

        private static void UpgradeIniStats()
        {
            IOResources.Serialize<IniStats[]>("upgraded.json", IniItemStats);
        }

        private static void UpdateImages()
        {
            Gear.UpdateImages();
            Mods.UpdateImages();
            Weapons.UpdateImages();
        }

        private static void CleanItems()
        {
            Gear = new ImportGear(Gear);
            Mods = new ImportMods(Mods);
            Weapons = new ImportWeapons(Weapons);
        }

        private static void LoadWikiStats()
        {
            CorrectedItemStats = LoadWikiStatsFromCSV();
            AssignWikiStats(CorrectedItemStats);
            GenerateWikiStats();
        }

        private static void LoadIniStats()
        {
            IniItemStats = IOResources.Deserialize<IniStats[]>(IOResources.ASSET_DIR + "\\filteredIniStats.json");

            AssignIniStatsTo(Weapons.primary, IniItemStats);
            AssignIniStatsTo(Weapons.secondary, IniItemStats);
        }

        public static IniStats[] GetFromWeapons(ImportItem[] items1, ImportItem[] items2)
        {
            List<IniStats> stats = new List<IniStats>();
            foreach (ImportItem item in items1)
            {
                if (item.IniStats != null)
                {
                    stats.Add(item.IniStats);
                }
            }
            foreach (ImportItem item in items2)
            {
                if (item.IniStats != null)
                {
                    stats.Add(item.IniStats);
                }
            }
            return stats.ToArray();
        }

        public static ImportItem GetItemByID(int index, ImportItem[] items)
        {
            if (index < 0)
            { return null; }
            return items[index];
        }


        public static ImportItem GetItemByName(string name, ImportItem[] items)
        {
            foreach (ImportItem item in items)
            {
                if (item.name == name)
                {
                    return item;
                }
            }
            return null;
        }

        internal static void AssignIniStatsTo(ImportItem[] items, IniStats[] stats)
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
                    LoggingSystem.LogInfo("No IniStats for " + item.name);
                }
            }
        }

        public static int GetGearID(ImportItem item)
        {
            return GetItemID(item, Gear.attachments);
        }
        public static int GetTacticalID(ImportItem item)
        {
            return GetItemID(item, Gear.tactical);
        }

        public static int GetMuzzleID(ImportItem item)
        {
            return GetItemID(item, Mods.muzzles);
        }
        public static int GetMagazineID(ImportItem item)
        {
            return GetItemID(item, Mods.magazines);
        }
        public static int GetTagID(ImportItem item)
        {
            return GetItemID(item, Gear.hangers);
        }
        public static int GetHelmetID(ImportItem item)
        {
            return GetItemID(item, Gear.helmets);
        }
        public static int GetUpperBodyID(ImportItem item)
        {
            return GetItemID(item, Gear.upperBodies);
        }
        public static int GetLowerBodyID(ImportItem item)
        {
            return GetItemID(item, Gear.lowerBodies);
        }
        public static int GetCamoBodyID(ImportItem item)
        {
            return GetItemID(item, Mods.camosBody);
        }
        public static int GetItemID(ImportItem item, ImportItem[] items)
        {
            if (item == null) { return -1; }
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].uid == item.uid)
                {
                    return i;
                }
            }
            return -1;
        }

        internal static void AssignWikiStatsTo(ImportItem[] items, WikiStats[] stats)
        {
            foreach (ImportItem item in items)
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
                //if (!found)
                //{
                //    LoggingSystem.LogInfo("No Wiki Stats for " + item.name);
                //}
            }
        }

        internal static void AssignWikiStats(WikiStats[] stats)
        {
            Gear.AssignWikiStats(stats);
            Mods.AssignWikiStats(stats);
            Weapons.AssignWikiStats(stats);
        }

        internal static void GenerateWikiStats()
        {
            List<WikiStats> stats = new List<WikiStats>();

            stats.AddRange(Gear.GetWikiStats());
            stats.AddRange(Mods.GetWikiStats());
            stats.AddRange(Weapons.GetWikiStats());
        }

        internal static WikiStats[] GetWikiStats(ImportItem[] items)
        {
            List<WikiStats> stats = new List<WikiStats>();
            foreach (var item in items)
            {
                stats.Add(item.WikiStats);
            }
            return stats.ToArray();
        }

        internal static WikiStats[] LoadWikiStatsFromCSV()
        {
            List<WikiStats> stats = new List<WikiStats>();
            StreamReader sr = new StreamReader(IOResources.ASSET_DIR + "\\BLR Wiki Stats.csv");
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

        internal static void UpdateImagesForImportItems(ImportItem[] items)
        {
            if (items.Length > 0)
            {
                var watch = LoggingSystem.LogInfo("Updating Images for " + items[0].Category, "");

                Parallel.ForEach(items, item =>
                {
                    item.LoadImage();
                    item.wideImageMale.Freeze();
                    item.wideImageFemale?.Freeze();
                    item.largeSquareImageMale.Freeze();
                    item.largeSquareImageFemale?.Freeze();
                    item.smallSquareImageMale.Freeze();
                    item.smallSquareImageFemale?.Freeze();
                });
                LoggingSystem.LogInfoAppend(watch);
            }
        }

        private static FoxIcon[] LoadAllIcons()
        {
            var watch = LoggingSystem.LogInfo("Loading All Icons", "");
            var icons = new List<FoxIcon>();
            foreach (var icon in Directory.EnumerateFiles("Assets\\textures"))
            {
                icons.Add(new FoxIcon(icon));
            }
            LoggingSystem.LogInfoAppend(watch);
            return icons.ToArray();
        }
        private static FoxIcon[] LoadAllCrosshairs()
        {
            var watch = LoggingSystem.LogInfo("Loading All Crosshairs", "");
            var icons = new List<FoxIcon>();
            foreach (var icon in Directory.EnumerateFiles("Assets\\crosshairs"))
            {
                icons.Add(new FoxIcon(icon));
            }
            LoggingSystem.LogInfoAppend(watch);
            return icons.ToArray();
        }

        public static ImportItem[] CleanItems(ImportItem[] importItems, string categoryName)
        {
            var watch = LoggingSystem.LogInfo("Started Cleaning " + categoryName, "");
            List<ImportItem> cleanedItems = new List<ImportItem>();
            foreach (ImportItem item in importItems)
            {
                if (IsValidItem(item))
                {
                    item.Category = categoryName;
                    cleanedItems.Add(item);
                }
            }
            LoggingSystem.LogInfoAppend(watch, " items:" + cleanedItems.Count + " are Left");
            return cleanedItems.ToArray();
        }
        public static bool IsValidItem(ImportItem item)
        {
            return item.tooltip != "SHOULDN'T BE USED" && !string.IsNullOrEmpty(item.name);
        }
    }
}