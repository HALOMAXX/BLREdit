using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace BLREdit
{

    public static class ImportSystem
    {
        public static readonly Dictionary<float, float> DamagePercentToValue = new Dictionary<float, float>();

        public static readonly FoxIcon[] Icons = LoadAllIcons();
        public static readonly FoxIcon[] Crosshairs = LoadAllCrosshairs();
        private static readonly ImportGear importGear = JsonSerializer.Deserialize<ImportGear>(File.ReadAllText(IOResources.GEAR_FILE), IOResources.JSO);
        private static readonly ImportMods importMods = JsonSerializer.Deserialize<ImportMods>(File.ReadAllText(IOResources.MOD_FILE), IOResources.JSO);
        private static readonly ImportWeapons importWeapons = JsonSerializer.Deserialize<ImportWeapons>(File.ReadAllText(IOResources.WEAPON_FILE), IOResources.JSO);

        private static WikiStats[] CorrectedItemStats;

        private static IniStats[] IniItemStats;

        public static ImportGear Gear;
        public static ImportMods Mods;
        public static ImportWeapons Weapons;

        internal static void Initialize()
        {
            var watch = LoggingSystem.LogInfo("Initializing Import System");

            UpdateImages();
            CleanItems();
            LoadWikiStats();
            LoadIniStats();

            LoggingSystem.LogInfoAppend(watch, "Import System");
        }

        private static void UpdateImages()
        {
            importGear.UpdateImages();
            importMods.UpdateImages();
            importWeapons.UpdateImages();
        }

        private static void CleanItems()
        {
            Gear = new ImportGear(importGear);
            Mods = new ImportMods(importMods);
            Weapons = new ImportWeapons(importWeapons);
        }

        private static void LoadWikiStats()
        {
            CorrectedItemStats = LoadWikiStatsFromCSV();
            AssignWikiStats(CorrectedItemStats);
            GenerateWikiStats();
        }

        private static void LoadIniStats()
        {
            try
            {
                File.Delete("UpgradedStats.json");
            }
            catch { }

            IniItemStats = JsonSerializer.Deserialize<IniStats[]>(File.ReadAllText(IOResources.ASSET_DIR + "\\filteredIniStats.json"), IOResources.JSOFields);

            AssignIniStatsTo(Weapons.primary, IniItemStats);
            AssignIniStatsTo(Weapons.secondary, IniItemStats);
#if DEBUG
            JsonSerializer.Serialize<IniStats[]>(File.OpenWrite("UpgradedStats.json"), IniItemStats, IOResources.JSOFields);
#endif
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
                if (!found)
                {
                    LoggingSystem.LogInfo("No Wiki Stats for " + item.name);
                }
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

            stats.AddRange(importGear.GetWikiStats());
            stats.AddRange(importMods.GetWikiStats());
            stats.AddRange(importWeapons.GetWikiStats());
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

        internal static void UpdateImagesForImportItems(ImportItem[] items, string categoryName)
        {
            var watch = LoggingSystem.LogInfo("Updating Images for " + categoryName, "");
            Parallel.ForEach(items, item =>
            {
                item.LoadImage();
                if ((categoryName == "Primary" || categoryName == "Secondary") && item.stats != null)
                {
                    item.IniStats = new IniStats() { ItemName = item.name, ItemID = item.uid, ROF = item.stats.rateOfFire, Burst = 1, ApplyTime = (60.0f / item.stats.rateOfFire) };
                }
                item.WideImage.Freeze();
                item.LargeSquareImage.Freeze();
                item.SmallSquareImage.Freeze();
            });
            foreach (ImportItem item in items)
            {
                item.PrepareImages();
            }
            LoggingSystem.LogInfoAppend(watch);
        }

        private static FoxIcon[] LoadAllIcons()
        {
            var watch = LoggingSystem.LogInfo("Loading All Icons","");
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
            var watch = LoggingSystem.LogInfo("Loading All Crosshairs","");
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
            var watch = LoggingSystem.LogInfo("Started Cleaning " + categoryName,"");
            List<ImportItem> cleanedItems = new List<ImportItem>();
            foreach (ImportItem item in importItems)
            {
                if (IsValidItem(item))
                {
                    item.Category = categoryName;
                    cleanedItems.Add(item);
                }
            }
            LoggingSystem.LogInfoAppend(watch);
            return cleanedItems.ToArray();
        }
        public static bool IsValidItem(ImportItem item)
        {
            return item.tooltip != "SHOULDN'T BE USED" && !string.IsNullOrEmpty(item.name);
        }
    }
}