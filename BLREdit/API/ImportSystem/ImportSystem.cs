using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace BLREdit
{

    public static class ImportSystem
    {
        //public static readonly Dictionary<float, float> DamagePercentToValue = new Dictionary<float, float>(); not in use

        public static readonly FoxIcon[] Icons = LoadAllIcons();
        public static readonly FoxIcon[] Crosshairs = LoadAllCrosshairs();

        private static Dictionary<string, List<BLRItem>> ItemLists { get; set; } = new Dictionary<string, List<BLRItem>>();

        public static void Initialize()
        {
            System.Diagnostics.Stopwatch watch = null;
            if (LoggingSystem.IsDebuggingEnabled) watch = LoggingSystem.LogInfo("Initializing Import System");

            //ImportGear Gear = IOResources.DeserializeFile<ImportGear>(IOResources.GEAR_FILE);
            //ImportMods Mods = IOResources.DeserializeFile<ImportMods>(IOResources.MOD_FILE);
            //ImportWeapons Weapons = IOResources.DeserializeFile<ImportWeapons>(IOResources.WEAPON_FILE);

            //ItemLists.Add(nameof(Gear.attachments), Gear.attachments);
            //ItemLists.Add(nameof(Gear.avatars), Gear.avatars);
            //ItemLists.Add(nameof(Gear.badges), Gear.badges);
            //ItemLists.Add(nameof(Gear.emotes), Gear.emotes);
            //ItemLists.Add(nameof(Gear.hangers), Gear.hangers);
            //ItemLists.Add(nameof(Gear.helmets), Gear.helmets);
            //ItemLists.Add(nameof(Gear.lowerBodies), Gear.lowerBodies);
            //ItemLists.Add(nameof(Gear.tactical), Gear.tactical);
            //ItemLists.Add(nameof(Gear.upperBodies), Gear.upperBodies);

            //ItemLists.Add(nameof(Mods.ammo), Mods.ammo);
            //ItemLists.Add(nameof(Mods.ammos), Mods.ammos);
            //ItemLists.Add(nameof(Mods.barrels), Mods.barrels);
            //ItemLists.Add(nameof(Mods.camosBody), Mods.camosBody);
            //ItemLists.Add(nameof(Mods.camosWeapon), Mods.camosWeapon);
            //ItemLists.Add(nameof(Mods.grips), Mods.grips);
            //ItemLists.Add(nameof(Mods.magazines), Mods.magazines);
            //ItemLists.Add(nameof(Mods.muzzles), Mods.muzzles);
            //ItemLists.Add(nameof(Mods.primarySkins), Mods.primarySkins);
            //ItemLists.Add(nameof(Mods.scopes), Mods.scopes);
            //ItemLists.Add(nameof(Mods.stocks), Mods.stocks);

            //ItemLists.Add(nameof(Weapons.depot), Weapons.depot);
            //ItemLists.Add(nameof(Weapons.primary), Weapons.primary);
            //ItemLists.Add(nameof(Weapons.secondary), Weapons.secondary);

            ItemLists = IOResources.DeserializeFile<Dictionary<string, List<BLRItem>>>(IOResources.ITEM_LIST_FILE);

            //CleanItems();
            UpdateImages();
            //LoadWikiStats();
            //LoadIniStats();

            //Dictionary<string, List<BLRItem>> Items = new();

            //foreach (var itemList in ItemLists)
            //{
            //    List<BLRItem> items = new List<BLRItem>();
            //    foreach (var item in itemList.Value)
            //    {
            //        items.Add(new BLRItem(item));
            //    }
            //    Items.Add(itemList.Key, items);
            //}

            //IOResources.SerializeFile("itemList.json", Items);

            if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfoAppend(watch, "Import System");
        }

        private static void CleanItems()
        {

            //foreach (KeyValuePair<string, List<BLRItem>> entry in ItemLists)
            //{
            //    System.Diagnostics.Stopwatch watch = null;
            //    if (LoggingSystem.IsDebuggingEnabled) watch = LoggingSystem.LogInfo("Started Cleaning " + entry.Key, "");
            //    List<ImportItem> ToRemove = new();
            //    foreach (ImportItem item in entry.Value)
            //    {
            //        if (IsValidItem(item))
            //        {
            //            item.Category = entry.Key;
            //        }
            //        else
            //        {
            //            ToRemove.Add(item);
            //        }

            //    }

            //    foreach (ImportItem item in ToRemove)
            //    { 
            //        entry.Value.Remove(item);
            //    }

            //    if (entry.Key == "avatars")
            //    {
            //        entry.Value.Add(new ImportItem() { name = "No Avatar", Category = "avatars" });
            //    }
            //    if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfoAppend(watch, " " + ToRemove.Count + " have been Removed");
            //}
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

        //private static void LoadWikiStats()
        //{
        //    AssignWikiStats(LoadWikiStatsFromCSV());
        //}

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
        //private static void AssignWikiStats(WikiStats[] stats)
        //{
        //    foreach (KeyValuePair<string, List<ImportItem>> entry in ItemLists)
        //    {
        //        foreach (BLRItem item in entry.Value)
        //        {
        //            bool found = false;
        //            foreach (WikiStats stat in stats)
        //            {
        //                if (!found && stat.itemID == item.UID)
        //                {
        //                    item.WikiStats = new WikiStats()
        //                    {
        //                        itemID = item.UID,
        //                        itemName = item.Name,
        //                        aimSpread = stat.aimSpread,
        //                        ammoMag = stat.ammoMag,
        //                        ammoReserve = stat.ammoReserve,
        //                        damage = stat.damage,
        //                        firerate = stat.firerate,
        //                        hipSpread = stat.hipSpread,
        //                        moveSpread = stat.moveSpread,
        //                        rangeClose = stat.rangeClose,
        //                        rangeFar = stat.rangeFar,
        //                        recoil = stat.recoil,
        //                        reload = stat.reload,
        //                        run = stat.run,
        //                        scopeInTime = stat.scopeInTime,
        //                        swaprate = stat.swaprate,
        //                        zoom = stat.zoom
        //                    };
        //                    found = true;
        //                }
        //            }
        //        }
        //    }
        //}

        //private static void LoadIniStats()
        //{
        //    var iniStats = IOResources.DeserializeFile<IniStats[]>(IOResources.ASSET_DIR + "\\filteredIniStats.json");

        //    ItemLists.TryGetValue("primary", out List<ImportItem> primary);
        //    ItemLists.TryGetValue("secondary", out List<ImportItem> secondary);

        //    AssignIniStatsTo(primary, iniStats);
        //    AssignIniStatsTo(secondary, iniStats);
        //}

        //internal static void AssignIniStatsTo(List<ImportItem> items, IniStats[] stats)
        //{
        //    foreach (ImportItem item in items)
        //    {
        //        bool found = false;
        //        foreach (IniStats stat in stats)
        //        {
        //            if (stat.ItemID == item.uid)
        //            {
        //                item.IniStats = stat;
        //                found = true;
        //            }
        //        }
        //        if (!found)
        //        {
        //            if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("No IniStats for " + item.name);
        //        }
        //    }
        //}

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
        private static FoxIcon[] LoadAllCrosshairs()
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


        public static bool IsValidItem(BLRItem item)
        {
            return item.Tooltip != "SHOULDN'T BE USED" && !string.IsNullOrEmpty(item.Name);
        }
    }
}