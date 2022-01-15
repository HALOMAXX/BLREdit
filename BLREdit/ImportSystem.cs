using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BLREdit {

    public static class ImportSystem
    {
        public static readonly Dictionary<float, float> DamagePercentToValue = new Dictionary<float, float>();

        public static readonly FoxIcon[] Icons = LoadAllIcons();
        private static readonly ImportGear importGear = JsonSerializer.Deserialize<ImportGear>(File.ReadAllText(IOResources.GEAR_FILE), IOResources.JSO);
        private static readonly ImportMods importMods = JsonSerializer.Deserialize<ImportMods>(File.ReadAllText(IOResources.MOD_FILE), IOResources.JSO);
        private static readonly ImportWeapons importWeapons = JsonSerializer.Deserialize<ImportWeapons>(File.ReadAllText(IOResources.WEAPON_FILE), IOResources.JSO);

        private static WikiStats[] CorrectedItemStats;

        public static ImportGear Gear;
        public static ImportMods Mods;
        public static ImportWeapons Weapons;

        internal static void Initialize()
        {
            LoggingSystem.LogInfo("Initializing Import System");

            //AddAllPercentageConversions();

            importGear.UpdateImages();
            importMods.UpdateImages();
            importWeapons.UpdateImages();
            Gear = new ImportGear(importGear);
            Mods = new ImportMods(importMods);
            Weapons = new ImportWeapons(importWeapons);

            WikiStats[] stats = LoadWikiStatsFromCSV();

            foreach (WikiStats statsItem in stats)
            {
                LoggingSystem.LogInfo(statsItem.ToString());
            }

            AssignWikiStats(stats);

            GenerateWikiStats();

            JsonSerializer.Serialize(File.OpenWrite("generatedWikiStats.json"), CorrectedItemStats, IOResources.JSO);

            LoggingSystem.LogInfo("Finished Initializing Import System");
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
                    if (stat.itemID == item.uid)
                    {
                        item.WikiStats = new WikiStats() {
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
                    }
                }
                if (!found)
                {
                    //LoggingSystem.LogInfo("Not Same Item");
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

            CorrectedItemStats = stats.ToArray();
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
            StreamReader sr = new StreamReader("BLR Wiki Stats.csv");
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split(',');
                stats.Add(new WikiStats() {
                    itemName = parts[0],
                    itemID = int.Parse(parts[1]),
                    damage = float.Parse(parts[2], CultureInfo.CurrentCulture) / 100.0f,
                    firerate = float.Parse(parts[3], CultureInfo.CurrentCulture) / 100.0f,
                    ammoMag = float.Parse(parts[4], CultureInfo.CurrentCulture) / 100.0f,
                    ammoReserve = float.Parse(parts[5], CultureInfo.CurrentCulture) / 100.0f,
                    reload = float.Parse(parts[6], CultureInfo.CurrentCulture) / 100.0f,
                    swaprate = float.Parse(parts[7], CultureInfo.CurrentCulture) / 100.0f,
                    aimSpread = float.Parse(parts[8], CultureInfo.CurrentCulture) / 100.0f,
                    hipSpread = float.Parse(parts[9], CultureInfo.CurrentCulture) / 100.0f,
                    moveSpread = float.Parse(parts[10], CultureInfo.CurrentCulture) / 100.0f,
                    recoil = float.Parse(parts[11], CultureInfo.CurrentCulture) / 100.0f,
                    zoom = float.Parse(parts[12], CultureInfo.CurrentCulture) / 100.0f,
                    scopeInTime = float.Parse(parts[13], CultureInfo.CurrentCulture) / 100.0f,
                    rangeClose = float.Parse(parts[14], CultureInfo.CurrentCulture) / 100.0f,
                    rangeFar = float.Parse(parts[15], CultureInfo.CurrentCulture) / 100.0f,
                    run = float.Parse(parts[16], CultureInfo.CurrentCulture) / 100.0f
                });
            }
            sr.Close();
            return stats.ToArray();
        }

        internal static void UpdateImagesForImportItems(ImportItem[] items, string categoryName)
        {
            LoggingSystem.LogInfo("Updating Images for " + categoryName);
            foreach (ImportItem item in items)
            {
                item.LoadImage();
            }
            LoggingSystem.LogInfo("Done Updating Images for " + categoryName);
        }

        private static FoxIcon[] LoadAllIcons()
        {
            LoggingSystem.LogInfo("Loading All Icons");
            var icons = new List<FoxIcon>();
            foreach (var icon in Directory.EnumerateFiles("Assets\\textures"))
            {
                icons.Add(new FoxIcon(icon));
            }
            LoggingSystem.LogInfo("Finished Loading All Icons");
            return icons.ToArray();
        }

        public static ImportItem[] CleanItems(ImportItem[] importItems, string categoryName)
        {
            LoggingSystem.LogInfo("Cleaning " + categoryName);
            List<ImportItem> cleanedItems = new List<ImportItem>();
            foreach (ImportItem item in importItems)
            {
                if ( categoryName=="Attachments" || categoryName=="Tactical" || !string.IsNullOrEmpty(item.icon))
                {
                    if (!string.IsNullOrEmpty(item.name))
                    {
                        item.Category = categoryName;
                        item.WikiStats = new WikiStats() { itemName = item.name, itemID = item.uid };
                        cleanedItems.Add(item);
                    }
                }
            }
            LoggingSystem.LogInfo("Finished Cleaning " + categoryName);
            return cleanedItems.ToArray();
        }
    }

    public class FoxIcon
    {
        public string Name { get; set; } = "";
        public Uri Icon { get; set; } = null;

        public FoxIcon(string file)
        {
            string[] fileparts = file.Split('\\');
            Name = fileparts[fileparts.Length-1].Split('.')[0];
            Icon = new Uri(AppDomain.CurrentDomain.BaseDirectory + file, UriKind.Absolute);
        }

        public static BitmapImage CreateEmptyBitmap()
        {
            return new BitmapImage();
        }
    }
    public class ImportGear
    {
        public ImportItem[] attachments { get; set; }
        public ImportItem[] avatars { get; set; }
        public ImportItem[] badges { get; set; }
        public object[] crosshairs { get; set; }
        public ImportItem[] emotes { get; set; }
        public ImportItem[] hangers { get; set; }
        public ImportItem[] helmets { get; set; }
        public ImportItem[] lowerBodies { get; set; }
        public ImportItem[] tactical { get; set; }
        public ImportItem[] upperBodies { get; set; }

        internal void AssignWikiStats(WikiStats[] stats)
        {
            ImportSystem.AssignWikiStatsTo(attachments, stats);
            ImportSystem.AssignWikiStatsTo(avatars, stats);
            ImportSystem.AssignWikiStatsTo(badges, stats);
            ImportSystem.AssignWikiStatsTo(emotes, stats);
            ImportSystem.AssignWikiStatsTo(hangers, stats);
            ImportSystem.AssignWikiStatsTo(helmets, stats);
            ImportSystem.AssignWikiStatsTo(lowerBodies, stats);
            ImportSystem.AssignWikiStatsTo(tactical, stats);
            ImportSystem.AssignWikiStatsTo(upperBodies, stats);
        }

        public WikiStats[] GetWikiStats()
        {
            List<WikiStats> stats = new List<WikiStats>();
            stats.AddRange(ImportSystem.GetWikiStats(attachments));
            stats.AddRange(ImportSystem.GetWikiStats(avatars));
            stats.AddRange(ImportSystem.GetWikiStats(badges));
            stats.AddRange(ImportSystem.GetWikiStats(emotes));
            stats.AddRange(ImportSystem.GetWikiStats(hangers));
            stats.AddRange(ImportSystem.GetWikiStats(helmets));
            stats.AddRange(ImportSystem.GetWikiStats(lowerBodies));
            stats.AddRange(ImportSystem.GetWikiStats(tactical));
            stats.AddRange(ImportSystem.GetWikiStats(upperBodies));
            return stats.ToArray();
        }

        public void UpdateImages()
        {
            ImportSystem.UpdateImagesForImportItems(attachments, "Attachments");
            ImportSystem.UpdateImagesForImportItems(avatars, "Avatars");
            ImportSystem.UpdateImagesForImportItems(badges, "Badges");
            ImportSystem.UpdateImagesForImportItems(emotes, "Emotes");
            ImportSystem.UpdateImagesForImportItems(hangers, "Hangers");
            ImportSystem.UpdateImagesForImportItems(helmets, "Helmets");
            ImportSystem.UpdateImagesForImportItems(lowerBodies, "LowerBodies");
            ImportSystem.UpdateImagesForImportItems(tactical, "Tactical");
            ImportSystem.UpdateImagesForImportItems(upperBodies, "UpperBodies");
        }

        public ImportGear() { }
        public ImportGear(ImportGear gear)
        {
            attachments = ImportSystem.CleanItems(gear.attachments, "Attachments");
            avatars = ImportSystem.CleanItems(gear.avatars, "Avatars");
            badges = ImportSystem.CleanItems(gear.badges, "Badges");
            emotes = ImportSystem.CleanItems(gear.emotes, "Emotes");
            hangers = ImportSystem.CleanItems(gear.hangers, "Hangers");
            helmets = ImportSystem.CleanItems(gear.helmets, "Helmets");
            lowerBodies = ImportSystem.CleanItems(gear.lowerBodies, "LowerBodies");
            tactical = ImportSystem.CleanItems(gear.tactical, "Tactical");
            upperBodies = ImportSystem.CleanItems(gear.upperBodies, "UpperBodies");
        }

        public override string ToString()
        {
            string TextWall = "{";

            TextWall += "Atachments:\n";
            foreach (ImportItem item in attachments)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nAvatars:\n";
            foreach (ImportItem item in avatars)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nBadges:\n";
            foreach (ImportItem item in badges)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nEmotes:\n";
            foreach (ImportItem item in emotes)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nHelmets:\n";
            foreach (ImportItem item in helmets)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nHelmets:\n";
            foreach (ImportItem item in helmets)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nLowerBodies:\n";
            foreach (ImportItem item in lowerBodies)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nTactical:\n";
            foreach (ImportItem item in tactical)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nUpperBodies:\n";
            foreach (ImportItem item in upperBodies)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            return TextWall + "}";
        }
    }

    public class ImportMods
    {
        public ImportItem[] ammo { get; set; }
        public ImportItem[] ammos { get; set; }
        public ImportItem[] barrels { get; set; }
        public ImportItem[] camos { get; set; }
        public ImportItem[] grips { get; set; }
        public ImportItem[] magazines { get; set; }
        public ImportItem[] muzzles { get; set; }
        public ImportItem[] primarySkins { get; set; }
        public ImportItem[] scopes { get; set; }
        public object[] secondarySkins { get; set; }
        public ImportItem[] stocks { get; set; }

        internal void AssignWikiStats(WikiStats[] stats)
        {
            ImportSystem.AssignWikiStatsTo(barrels, stats);
            ImportSystem.AssignWikiStatsTo(magazines, stats);
            ImportSystem.AssignWikiStatsTo(muzzles, stats);
            ImportSystem.AssignWikiStatsTo(scopes, stats);
            ImportSystem.AssignWikiStatsTo(stocks, stats);
        }

        public WikiStats[] GetWikiStats()
        {
            List<WikiStats> stats = new List<WikiStats>();
            stats.AddRange(ImportSystem.GetWikiStats(barrels));
            stats.AddRange(ImportSystem.GetWikiStats(magazines));
            stats.AddRange(ImportSystem.GetWikiStats(muzzles));
            stats.AddRange(ImportSystem.GetWikiStats(scopes));
            stats.AddRange(ImportSystem.GetWikiStats(stocks));
            return stats.ToArray();
        }

        public void UpdateImages()
        {
            ImportSystem.UpdateImagesForImportItems(ammo, "ammo");
            ImportSystem.UpdateImagesForImportItems(ammos, "ammos");
            ImportSystem.UpdateImagesForImportItems(barrels, "barrel");
            ImportSystem.UpdateImagesForImportItems(camos, "camo");
            ImportSystem.UpdateImagesForImportItems(grips, "grip");
            ImportSystem.UpdateImagesForImportItems(magazines, "magazine");
            ImportSystem.UpdateImagesForImportItems(muzzles, "muzzle");
            ImportSystem.UpdateImagesForImportItems(primarySkins, "primarySkin");
            ImportSystem.UpdateImagesForImportItems(scopes, "scope");
            ImportSystem.UpdateImagesForImportItems(stocks, "stock");
        }

        public ImportMods() { }
        public ImportMods(ImportMods mods)
        {
            ammo = ImportSystem.CleanItems(mods.ammo, "ammo");
            ammos = ImportSystem.CleanItems(mods.ammos, "ammos");
            barrels = ImportSystem.CleanItems(mods.barrels, "barrel");
            camos = ImportSystem.CleanItems(mods.camos, "camo");
            grips = ImportSystem.CleanItems(mods.grips, "grip");
            magazines = ImportSystem.CleanItems(mods.magazines, "magazine");
            muzzles = ImportSystem.CleanItems(mods.muzzles, "muzzle");
            primarySkins = ImportSystem.CleanItems(mods.primarySkins, "primarySkin");
            scopes = ImportSystem.CleanItems(mods.scopes, "scope");
            stocks = ImportSystem.CleanItems(mods.stocks, "stock");
        }
        public override string ToString()
        {
            string TextWall = "{";

            TextWall += "Ammo:\n";
            foreach (ImportItem item in ammo)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nAmmos:\n";
            foreach (ImportItem item in ammos)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nBarrels:\n";
            foreach (ImportItem item in barrels)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nCamos:\n";
            foreach (ImportItem item in camos)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nGrips:\n";
            foreach (ImportItem item in grips)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nMagazines:\n";
            foreach (ImportItem item in magazines)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nMuzzles:\n";
            foreach (ImportItem item in muzzles)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nPrimarySkins:\n";
            foreach (ImportItem item in primarySkins)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nScopes:\n";
            foreach (ImportItem item in scopes)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nStocks:\n";
            foreach (ImportItem item in stocks)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            return TextWall + "}";
        }
    }

    public class ImportWeapons
    {
        public ImportItem[] depot { get; set; }
        public ImportItem[] primary { get; set; }
        public ImportItem[] secondary { get; set; }

        internal void AssignWikiStats(WikiStats[] stats)
        {
            ImportSystem.AssignWikiStatsTo(depot, stats);
            ImportSystem.AssignWikiStatsTo(primary, stats);
            ImportSystem.AssignWikiStatsTo(secondary, stats);
        }

        public WikiStats[] GetWikiStats()
        {
            List<WikiStats> stats = new List<WikiStats>();
            stats.AddRange(ImportSystem.GetWikiStats(depot));
            stats.AddRange(ImportSystem.GetWikiStats(primary));
            stats.AddRange(ImportSystem.GetWikiStats(secondary));
            return stats.ToArray();
        }

        public void UpdateImages()
        {
            ImportSystem.UpdateImagesForImportItems(depot, "Depot");
            ImportSystem.UpdateImagesForImportItems(primary, "Primary");
            ImportSystem.UpdateImagesForImportItems(secondary, "Secondary");
        }

        public ImportWeapons() { }
        public ImportWeapons(ImportWeapons weapons)
        {
            depot = ImportSystem.CleanItems(weapons.depot, "Depot");
            primary = ImportSystem.CleanItems(weapons.primary, "Primary");
            secondary = ImportSystem.CleanItems(weapons.secondary, "Secondary");
        }

        public override string ToString()
        {
            string TextWall = "{";

            TextWall += "Depot:\n";
            foreach (ImportItem item in depot)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nPrimaries:\n";
            foreach (ImportItem item in primary)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            TextWall += "\nSecondaries:\n";
            foreach (ImportItem item in secondary)
            {
                TextWall += "\t" + item.ToString() + "\n";
            }

            return TextWall + "}";
        }
    }

    public class ImportItem
    {
        public string Category { get; set; }
        public string _class { get; set; }
        public string icon { get; set; }
        public BitmapSource Image { get; private set; }
        public string name { get; set; }
        public Pawnmodifiers pawnModifiers { get; set; }
        public string tooltip { get; set; }
        public int uid { get; set; }
        public int[] validFor { get; set; }
        public Weaponmodifiers weaponModifiers { get; set; }
        public string[] supportedMods { get; set; }
        public Stats stats { get; set; }
        public WikiStats WikiStats { get; set; }

        public ImportItem()
        {
            WikiStats = new WikiStats() { itemName = name, itemID = uid };
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[Class={0}, Icon={1}, Name={2}, PawnModifiers={3}, Tooltip={4}, UID={5}, ValidFor={6}, WeaponModifiers={7}, SupportedMods={8}, Stats={9}]", _class, icon, name, pawnModifiers, tooltip, uid, PrintIntArray(validFor), weaponModifiers, PrintStringArray(supportedMods), stats);
            return sb.ToString();
        }

        public bool IsValidFor(ImportItem item)
        {
            if (validFor == null || item == null) { return true; }
            foreach (int id in validFor)
            {
                if (id == item.uid)
                { return true; }
            }
            return false;
        }

        public void LoadImage()
        {
            Image = GetBitmapImage(icon);
        }

        public static BitmapSource GetBitmapImage(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                foreach (FoxIcon icon in ImportSystem.Icons)
                {
                    if (icon.Name == name)
                    {
                        return new BitmapImage(icon.Icon);
                    }
                }
            }
            return FoxIcon.CreateEmptyBitmap();
        }

        private string PrintIntArray(int[] ints)
        {
            string array = "[";
            if (ints != null)
            {
                foreach (int i in ints)
                {
                    array += ' ' + i + ',';
                }
            }
            return array + ']';
        }

        private string PrintStringArray(string[] strings)
        {
            string array = "[";
            if (strings != null)
            {
                foreach (string s in strings)
                {
                    array += ' ' + s + ',';
                }
            }
            return array + ']';
        }
    }
    public class Pawnmodifiers
    {
        public int BodyDamageReduction { get; set; }
        public int ElectroProtection { get; set; }
        public int ExplosiveProtection { get; set; }
        public int GearSlots { get; set; }
        public int HRVDuration { get; set; }
        public int HRVRechargeRate { get; set; }
        public int Health { get; set; }
        public int HealthRecharge { get; set; }
        public int HelmetDamageReduction { get; set; }
        public int IncendiaryProtection { get; set; }
        public int InfraredProtection { get; set; }
        public int LegsDamageReduction { get; set; }
        public int MeleeProtection { get; set; }
        public int MeleeRange { get; set; }
        public int MovementSpeed { get; set; }
        public int PermanentHealthProtection { get; set; }
        public float SprintMultiplier { get; set; }
        public int Stamina { get; set; }
        public int SwitchWeaponSpeed { get; set; }
        public int ToxicProtection { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("BodyDamageReduction:{0}, ElectroProtection:{1}, ExplosiveProtection:{2}, GearSlots:{3}, HRVDuration:{4}, HRVRechargeRate:{5}, Health:{6}, HealthRecharge:{7}, HelmetDamageReduction:{8}, IncendiaryProtection:{9}, InfraredProtection:{10}, LegsDamageReduction:{11}, MeleeProtection:{12}, MeleeRange:{13}, MovementSpeed:{14}, PermanentHealthProtection:{15}, SprintMultiplier:{16}, Stamina:{17}, SwitchWeaponSpeed:{18}, ToxicProtection{19}", BodyDamageReduction, ElectroProtection, ExplosiveProtection, GearSlots, HRVDuration, HRVRechargeRate, Health, HealthRecharge, HelmetDamageReduction, IncendiaryProtection, InfraredProtection, LegsDamageReduction, MeleeProtection, MeleeRange, MovementSpeed, PermanentHealthProtection, SprintMultiplier, Stamina, SwitchWeaponSpeed, ToxicProtection);
            return sb.ToString();
        }
    }

    public class Weaponmodifiers
    {
        public int accuracy { get; set; }
        public int ammo { get; set; }
        public int damage { get; set; }
        public int movementSpeed { get; set; }
        public int range { get; set; }
        public int rateOfFire { get; set; }
        public int rating { get; set; }
        public int recoil { get; set; }
        public int reloadSpeed { get; set; }
        public int switchWeaponSpeed { get; set; }
        public int weaponWeight { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Accuracy:{0}, Ammo:{1}, Damage:{2}, MovementSpeed:{3}, Range:{4}, RateOfFire:{5}, Rating:{6}, Recoil:{7}, ReloadSpeed:{8}, WeaponSwitchSpeed:{9}, WeaponWeight:{10}", accuracy, ammo, damage, movementSpeed, range, rateOfFire, rating, recoil, reloadSpeed, switchWeaponSpeed, weaponWeight);
            return sb.ToString();
        }
    }

    public class Stats
    {
        public float accuracy { get; set; }
        public float damage { get; set; }
        public float movementSpeed { get; set; }
        public float range { get; set; }
        public int rateOfFire { get; set; }
        public float recoil { get; set; }
        public float reloadSpeed { get; set; }
        public float weaponWeight { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Accuracy:{0}, Damage:{1}, MovementSpeed:{2}, Range:{3}, RateOfFire:{4}, Recoil:{5}, ReloadSpeed:{6}, WeaponWeight:{7}", accuracy, damage, movementSpeed, range, rateOfFire, recoil, reloadSpeed, weaponWeight);
            return sb.ToString();
        }

    }

    public class WikiStats
    {
        public int itemID { get; set; }
        public string itemName { get; set; }
        public float damage { get; set; }
        public float firerate { get; set; }
        public float ammoMag { get; set; }
        public float ammoReserve { get; set; }
        public float reload { get; set; }
        public float swaprate { get; set; }
        public float aimSpread { get; set; }
        public float hipSpread { get; set; }
        public float moveSpread { get; set; }
        public float recoil { get; set; }
        public float zoom { get; set; }
        public float scopeInTime { get; set; }
        public float rangeClose { get; set; }
        public float rangeFar { get; set; }
        public float run { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[ ID={0}, Name={1}, Damage={2}, ROF={3}, Ammo={4}/{5}, Reload={6}, SwapRate={7}, Aim={8}, Hip={9}, Move={10}, Recoil={11}, Zoom={12}, Scope In={13}, Range={14}/{15}, Run={16}]",
                itemID, itemName, damage, firerate, ammoMag, ammoReserve, reload, swaprate, aimSpread, hipSpread, moveSpread, recoil, zoom, scopeInTime, rangeClose, rangeFar, run);
            return sb.ToString();
        }
    }
}