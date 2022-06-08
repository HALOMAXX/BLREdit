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

        private static Dictionary<string, List<ImportItem>> ItemLists { get; set; } = new Dictionary<string, List<ImportItem>>();

        public static void Initialize()
        {
            var watch = LoggingSystem.LogInfo("Initializing Import System");

            ImportGear Gear = IOResources.DeserializeFile<ImportGear>(IOResources.GEAR_FILE);
            ImportMods Mods = IOResources.DeserializeFile<ImportMods>(IOResources.MOD_FILE);
            ImportWeapons Weapons = IOResources.DeserializeFile<ImportWeapons>(IOResources.WEAPON_FILE);

            ItemLists.Add(nameof(Gear.attachments), Gear.attachments);
            ItemLists.Add(nameof(Gear.avatars), Gear.avatars);
            ItemLists.Add(nameof(Gear.badges), Gear.badges);
            ItemLists.Add(nameof(Gear.emotes), Gear.emotes);
            ItemLists.Add(nameof(Gear.hangers), Gear.hangers);
            ItemLists.Add(nameof(Gear.helmets), Gear.helmets);
            ItemLists.Add(nameof(Gear.lowerBodies), Gear.lowerBodies);
            ItemLists.Add(nameof(Gear.tactical), Gear.tactical);
            ItemLists.Add(nameof(Gear.upperBodies), Gear.upperBodies);

            ItemLists.Add(nameof(Mods.ammo), Mods.ammo);
            ItemLists.Add(nameof(Mods.ammos), Mods.ammos);
            ItemLists.Add(nameof(Mods.barrels), Mods.barrels);
            ItemLists.Add(nameof(Mods.camosBody), Mods.camosBody);
            ItemLists.Add(nameof(Mods.camosWeapon), Mods.camosWeapon);
            ItemLists.Add(nameof(Mods.grips), Mods.grips);
            ItemLists.Add(nameof(Mods.magazines), Mods.magazines);
            ItemLists.Add(nameof(Mods.muzzles), Mods.muzzles);
            ItemLists.Add(nameof(Mods.primarySkins), Mods.primarySkins);
            ItemLists.Add(nameof(Mods.scopes), Mods.scopes);
            ItemLists.Add(nameof(Mods.stocks), Mods.stocks);

            //LoggingSystem.LogInfo(Gear.helmets[0].weaponModifiers.ToString());
            //LoggingSystem.LogInfo(Gear.helmets[0].pawnModifiers.ToString());

            ItemLists.Add(nameof(Weapons.depot), Weapons.depot);
            ItemLists.Add(nameof(Weapons.primary), Weapons.primary);
            ItemLists.Add(nameof(Weapons.secondary), Weapons.secondary);

            CleanItems();
            UpdateImages();
            LoadWikiStats();
            LoadIniStats();

            LoggingSystem.LogInfoAppend(watch, "Import System");
        }

        private static void CleanItems()
        {

            foreach (KeyValuePair<string, List<ImportItem>> entry in ItemLists)
            {
                var watch = LoggingSystem.LogInfo("Started Cleaning " + entry.Key, "");
                List<ImportItem> ToRemove = new List<ImportItem>();
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

                if (entry.Key == "avatars")
                {
                    entry.Value.Add(new ImportItem() { name = "No Avatar", Category = "avatars" });
                }
                LoggingSystem.LogInfoAppend(watch, " " + ToRemove.Count + " have been Removed");
            }
        }

        private static void UpdateImages()
        {
            foreach (KeyValuePair<string, List<ImportItem>> entry in ItemLists)
            {
                var watch = LoggingSystem.LogInfo("Updating Images for " + entry.Key, "");
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
                LoggingSystem.LogInfoAppend(watch);
            }
        }

        private static void LoadWikiStats()
        {
            AssignWikiStats(LoadWikiStatsFromCSV());
        }

        private static WikiStats[] LoadWikiStatsFromCSV()
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
        private static void AssignWikiStats(WikiStats[] stats)
        {
            foreach (KeyValuePair<string, List<ImportItem>> entry in ItemLists)
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

        private static void LoadIniStats()
        {
            var iniStats = IOResources.DeserializeFile<IniStats[]>(IOResources.ASSET_DIR + "\\filteredIniStats.json");
            
            ItemLists.TryGetValue("primary", out List<ImportItem> primary);
            ItemLists.TryGetValue("secondary", out List<ImportItem> secondary);

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
                    LoggingSystem.LogInfo("No IniStats for " + item.name);
                }
            }
        }

        public static List<ImportItem> GetItemListOfType(string Type)
        {
            if (string.IsNullOrEmpty(Type)) return null;
            if (ItemLists.TryGetValue(Type, out List<ImportItem> items))
            {
                return items;
            }
            else
            {
                return null;
            }
        }

        public static ImportItem[] GetItemArrayOfType(string Type)
        {
            if (string.IsNullOrEmpty(Type)) return null;
            if (ItemLists.TryGetValue(Type, out List<ImportItem> items))
            {
                return items.ToArray();
            }
            else
            {
                return null;
            }
        }

        public static int GetIDOfItem(ImportItem item)
        {
            if (item == null) return -1;
            if (ItemLists.TryGetValue(item.Category, out List<ImportItem> items))
            {
                return items.IndexOf(item);
            }
            else
            {
                return -1;
            }
        }

        public static ImportItem GetItemByIDAndType(string Type, int ID)
        {
            if (ID < 0 || string.IsNullOrEmpty(Type)) return null;
            if (ItemLists.TryGetValue(Type, out List<ImportItem> items))
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
            if (ItemLists.TryGetValue(Type, out List<ImportItem> items))
            {
                foreach (ImportItem item in items)
                {
                    if (item.name == Name)
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

        public static ImportItem GetItemByNameAndType(string Type, string Name)
        {
            if (string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(Name)) return null;
            if (ItemLists.TryGetValue(Type, out List<ImportItem> items))
            {
                foreach (ImportItem item in items)
                {
                    if (item.name == Name)
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

        //public static IniStats[] GetFromWeapons(ImportItem[] items1, ImportItem[] items2)
        //{
        //    List<IniStats> stats = new List<IniStats>();
        //    foreach (ImportItem item in items1)
        //    {
        //        if (item.IniStats != null)
        //        {
        //            stats.Add(item.IniStats);
        //        }
        //    }
        //    foreach (ImportItem item in items2)
        //    {
        //        if (item.IniStats != null)
        //        {
        //            stats.Add(item.IniStats);
        //        }
        //    }
        //    return stats.ToArray();
        //}

        //public static ImportItem GetItemByID(int index, ImportItem[] items)
        //{
        //    if (index < 0 || index >= items.Length)
        //    { return null; }
        //    return items[index];
        //}


        //public static ImportItem GetItemByName(string name, ImportItem[] items)
        //{
        //    foreach (ImportItem item in items)
        //    {
        //        if (item.name == name)
        //        {
        //            return item;
        //        }
        //    }
        //    return null;
        //}

        //public static int GetGearID(ImportItem item)
        //{
        //    return GetItemID(item, Gear.attachments);
        //}
        //public static int GetTacticalID(ImportItem item)
        //{
        //    return GetItemID(item, Gear.tactical);
        //}
        //public static int GetMuzzleID(ImportItem item)
        //{
        //    return GetItemID(item, Mods.muzzles);
        //}
        //public static int GetMagazineID(ImportItem item)
        //{
        //    return GetItemID(item, Mods.magazines);
        //}
        //public static int GetTagID(ImportItem item)
        //{
        //    return GetItemID(item, Gear.hangers);
        //}
        //public static int GetHelmetID(ImportItem item)
        //{
        //    return GetItemID(item, Gear.helmets);
        //}
        //public static int GetUpperBodyID(ImportItem item)
        //{
        //    return GetItemID(item, Gear.upperBodies);
        //}
        //public static int GetLowerBodyID(ImportItem item)
        //{
        //    return GetItemID(item, Gear.lowerBodies);
        //}
        //public static int GetCamoBodyID(ImportItem item)
        //{
        //    return GetItemID(item, Mods.camosBody);
        //}
        //public static int GetCamoWeaponID(ImportItem item)
        //{
        //    return GetItemID(item, Mods.camosWeapon);
        //}
        //public static int GetAvatarID(ImportItem item)
        //{
        //    return GetItemID(item, Gear.avatars);
        //}

        //public static int GetItemID(ImportItem item, ImportItem[] items)
        //{
        //    if (item == null) { return 0; }
        //    for (int i = 0; i < items.Length; i++)
        //    {
        //        if (items[i].uid == item.uid)
        //        {
        //            return i;
        //        }
        //    }
        //    return 0;
        //}

        private static FoxIcon[] LoadAllIcons()
        {
            var watch = LoggingSystem.LogInfo("Loading All Icons", "");
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


        public static bool IsValidItem(ImportItem item)
        {
            return item.tooltip != "SHOULDN'T BE USED" && !string.IsNullOrEmpty(item.name);
        }
    }
}