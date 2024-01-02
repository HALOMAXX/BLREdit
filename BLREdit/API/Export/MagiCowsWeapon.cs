using BLREdit.API.Export;
using BLREdit.Import;
using BLREdit.UI;
using BLREdit.UI.Views;

using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BLREdit.Export;

public sealed class MagiCowsWeapon : IBLRWeapon
{
    [JsonIgnore] private string receiver = "Assault Rifle";
    public string Receiver { get { return receiver; } set { if (receiver != value) { receiver = value; isDirty = true; } } }

    [JsonIgnore] private int muzzle = 8;
    public int Muzzle { get { return muzzle; } set { if (muzzle != value) { muzzle = value; isDirty = true; } } }

    [JsonIgnore] private string stock = "Silverwood Standard Stock";
    public string Stock { get { return stock; } set { if (stock != value) { stock = value; isDirty = true; } } }

    [JsonIgnore] private string barrel = "Frontier Standard Barrel";
    public string Barrel { get { return barrel; } set { if (barrel != value) { barrel = value; isDirty = true; } } }

    [JsonIgnore] private int magazine = 9;
    public int Magazine { get { return magazine; } set { if (magazine != value) { magazine = value; isDirty = true; } } }

    [JsonIgnore] private int ammo = 2;
    public int Ammo { get { return ammo; } set { if (ammo != value) { ammo = value; isDirty = true; } } }

    [JsonIgnore] private string scope = "No Optic Mod";
    public string Scope { get { return scope; } set { if (scope != value) { scope = value; isDirty = true; } } }

    [JsonIgnore] private string grip = "";
    public string Grip { get { return grip; } set { if (grip != value) { grip = value; isDirty = true; } } }

    [JsonIgnore] private int tag = -1;
    public int Tag { get { return tag; } set { if (tag != value) { tag = value; isDirty = true; } } }

    [JsonIgnore] private int camo = -1;
    public int Camo { get { return camo; } set { if (camo != value) { camo = value; isDirty = true; } } }

    [JsonIgnore] private int skin = -1;
    public int Skin { get { return skin; } set { if (skin != value) { skin = value; isDirty = true; } } }

    [JsonIgnore] private bool isDirty = true;
    [JsonIgnore] public bool IsDirty { get { return (isDirty); } set { isDirty = value; } }

    public override string ToString()
    {
        return LoggingSystem.ObjectToTextWall(this);
    }

    //Clones the string variables
    public MagiCowsWeapon Clone()
    {
        MagiCowsWeapon clone = this.MemberwiseClone() as MagiCowsWeapon ?? new();
        clone.Receiver = string.Copy(this.Receiver);
        clone.Stock = string.Copy(this.Stock);
        clone.Barrel = string.Copy(this.Barrel);
        clone.Scope = string.Copy(this.Scope);
        clone.Grip = string.Copy(this.Grip);
        clone.isDirty = true;
        return clone;
    }

    public bool IsHealthOkAndRepair()
    {
        if (string.IsNullOrEmpty(Barrel) || string.IsNullOrEmpty(Stock) || string.IsNullOrEmpty(Scope))
        {
            if (string.IsNullOrEmpty(Barrel))
            {
                Barrel = NoBarrel;
            }

            if (string.IsNullOrEmpty(Stock))
            {
                Stock = NoStock;
            }

            if (string.IsNullOrEmpty(Scope))
            {
                Scope = NoScope;
            }
            return false;
        }
        return true;
    }

    public BLRItem? GetReceiver()
    {
        var primary = ImportSystem.GetItemByNameAndType(ImportSystem.PRIMARY_CATEGORY, Receiver);
        if (primary is not null)
            return primary;

        var secondary = ImportSystem.GetItemByNameAndType(ImportSystem.SECONDARY_CATEGORY, Receiver);
        if (secondary is not null)
            return secondary;

        return null;
    }

    public BLRItem? GetItemByType(string type)
    {
        return type switch
        {
            ImportSystem.CAMOS_WEAPONS_CATEGORY => GetCamo(),
            ImportSystem.HANGERS_CATEGORY => GetTag(),
            ImportSystem.MAGAZINES_CATEGORY => GetMagazine(),
            ImportSystem.BARRELS_CATEGORY => GetBarrel(),
            ImportSystem.SCOPES_CATEGORY => GetScope(),
            ImportSystem.STOCKS_CATEGORY => GetStock(),
            ImportSystem.AMMO_CATEGORY => GetAmmo(),
            ImportSystem.PRIMARY_SKIN_CATEGORY => GetSkin(),
            ImportSystem.GRIPS_CATEGORY => GetGrip(),
            ImportSystem.MUZZELS_CATEGORY => GetMuzzle(),
            _ => null,
        };
    }

    public BLRItem? GetCamo()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_WEAPONS_CATEGORY, Camo);
    }
    public BLRItem? GetTag()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.HANGERS_CATEGORY, Tag);
    }
    public BLRItem? GetMagazine()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.MAGAZINES_CATEGORY, Magazine);
    }
    public BLRItem? GetAmmo()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.AMMO_CATEGORY, Ammo);
    }
    public BLRItem? GetSkin()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.PRIMARY_SKIN_CATEGORY, Skin);
    }

    public BLRItem? GetMuzzle()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.MUZZELS_CATEGORY, Muzzle);
    }
    public BLRItem? GetStock()
    {
        return ImportSystem.GetItemByNameAndType(ImportSystem.STOCKS_CATEGORY, Stock) ?? ImportSystem.GetItemByNameAndType(ImportSystem.STOCKS_CATEGORY, NoStock);
    }
    public BLRItem? GetBarrel()
    {
        return ImportSystem.GetItemByNameAndType(ImportSystem.BARRELS_CATEGORY, Barrel) ?? ImportSystem.GetItemByNameAndType(ImportSystem.BARRELS_CATEGORY, NoBarrel);
    }
    public BLRItem? GetScope()
    {
        return ImportSystem.GetItemByNameAndType(ImportSystem.SCOPES_CATEGORY, Scope) ?? ImportSystem.GetItemByNameAndType(ImportSystem.SCOPES_CATEGORY, NoScope);
    }

    public BLRItem? GetGrip()
    {
        return ImportSystem.GetItemByNameAndType(ImportSystem.GRIPS_CATEGORY, Grip);
    }

    public static MagiCowsWeapon? GetDefaultSetupOfReceiver(BLRItem? item)
    {
        if (item is null) return null;
        var tuple = DefaultWeapons as ITuple;

        for (int i = 0; i < tuple.Length; i++)
        {
            if (tuple[i] is MagiCowsWeapon wpn && wpn.Receiver == item.Name)
            {
                return wpn.Clone();
            }
        }
        return null;
    }

    public void Read(BLRWeapon weapon)
    {
        UndoRedoSystem.CurrentlyBlockedEvents.Value = BlockEvents.All;
        weapon.Receiver = GetReceiver();
        weapon.Barrel = GetBarrel();
        weapon.Magazine = GetMagazine();
        weapon.Muzzle = GetMuzzle();
        weapon.Stock = GetStock();
        weapon.Scope = GetScope();
        weapon.Grip = GetGrip();
        weapon.Tag = GetTag();
        weapon.Camo = GetCamo();
        weapon.Ammo = GetAmmo();
        weapon.Skin = GetSkin();
        UndoRedoSystem.RestoreBlockedEvents();
    }

    public void Write(BLRWeapon weapon)
    {
        Receiver = weapon.Receiver?.Name ?? "Assault Rifle";
        Barrel = weapon.Barrel?.Name ?? "No Barrel Mod";
        Scope = weapon.Scope?.Name ?? "No Optic Mod";
        Stock = weapon.Stock?.Name ?? "No Stock";
        Grip = weapon.Grip?.Name ?? "";
        Muzzle = BLRItem.GetMagicCowsID(weapon.Muzzle, -1);
        Magazine = BLRItem.GetMagicCowsID(weapon.Magazine, -1);
        Tag = BLRItem.GetMagicCowsID(weapon.Tag);
        Camo = BLRItem.GetMagicCowsID(weapon.Camo);
        Ammo = BLRItem.GetMagicCowsID(weapon.Ammo);
        Skin = BLRItem.GetMagicCowsID(weapon.Skin);
        if (WasWrittenTo is not null) { WasWrittenTo(weapon, EventArgs.Empty); }
    }

    public ShareableWeapon ConvertToShareable()
    {
        return new ShareableWeapon()
        {
            Receiver = BLRItem.GetMagicCowsID(GetReceiver()),
            Barrel = BLRItem.GetMagicCowsID(GetBarrel()),
            Scope = BLRItem.GetMagicCowsID(GetScope()),
            Stock = BLRItem.GetMagicCowsID(GetStock()),
            Grip = BLRItem.GetMagicCowsID(GetGrip(), -1),
            Muzzle = Muzzle,
            Magazine = Magazine,
            Tag = Tag,
            Camo = Camo,
            Ammo = Ammo,
            Skin = Skin,
        };
    }

    public static readonly (MagiCowsWeapon HeavyAssaultRifle, MagiCowsWeapon LMGReacon, MagiCowsWeapon TacticalSMG, MagiCowsWeapon BurstfireSMG, MagiCowsWeapon AntiMaterielRifle, MagiCowsWeapon BullpupFullAuto, MagiCowsWeapon AK470Rifle, MagiCowsWeapon CompoundBow, MagiCowsWeapon M4XRifle, MagiCowsWeapon TacticalAssaultRifle, MagiCowsWeapon AssaultRifle, MagiCowsWeapon BoltActionRifle, MagiCowsWeapon LightMachineGun, MagiCowsWeapon BurstfireRifle, MagiCowsWeapon PrestigeAssaultRifle, MagiCowsWeapon SubmachineGun, MagiCowsWeapon CombatRifle, MagiCowsWeapon LightReconRifle, MagiCowsWeapon Shotgun_ARk, MagiCowsWeapon BreechLoadedPistol, MagiCowsWeapon Snub260, MagiCowsWeapon HeavyPistol, MagiCowsWeapon LightPistol, MagiCowsWeapon BurstfirePistol, MagiCowsWeapon PrestigeLightPistol, MagiCowsWeapon MachinePistol, MagiCowsWeapon Revolver, MagiCowsWeapon Shotgun, MagiCowsWeapon HardsuitHRVDecoy, MagiCowsWeapon RocketStinger, MagiCowsWeapon RocketSwarm, MagiCowsWeapon Railgun, MagiCowsWeapon Minigun, MagiCowsWeapon Turret, MagiCowsWeapon RhinoHardsuit, MagiCowsWeapon GrenadeLauncher, MagiCowsWeapon Flamethrower, MagiCowsWeapon Katana, MagiCowsWeapon Airstrike, MagiCowsWeapon GunmanHardsuit, MagiCowsWeapon MK1AssaultAI) DefaultWeapons = (
        HeavyAssaultRifle: new() { Barrel = DefaultBarrel, Grip = "", Magazine = 115, Ammo = 2, Muzzle = 9, Receiver = "Heavy Assault Rifle", Stock = DefaultStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 0  Heavy Assault Rifle
        LMGReacon: new() { Barrel = DefaultBarrel, Grip = "", Magazine = 104, Ammo = 2, Muzzle = 9, Receiver = "LMG Recon", Stock = DefaultStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 1  LMG Recon
        TacticalSMG: new() { Barrel = NoBarrel, Grip = "", Magazine = 107, Ammo = 2, Muzzle = 9, Receiver = "Tactical SMG", Stock = NoStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 2  Tactical SMG
        BurstfireSMG: new() { Barrel = NoBarrel, Grip = "", Magazine = 140, Ammo = 2, Muzzle = 9, Receiver = "Burstfire SMG", Stock = NoStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 3  Burstfire SMG
        AntiMaterielRifle: new() { Barrel = NoBarrel, Grip = "", Magazine = -1, Ammo = 2, Muzzle = NoMuzzle, Receiver = "Anti-Materiel Rifle", Stock = DefaultStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 4  Anti-Materiel Rifle
        BullpupFullAuto: new() { Barrel = "No Grip", Grip = "", Magazine = 149, Ammo = 2, Muzzle = 9, Receiver = "Bullpup Full Auto", Stock = DefaultBullPupStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 5  Bullpup Full Auto
        AK470Rifle: new() { Barrel = DefaultBarrel, Grip = "", Magazine = 195, Ammo = 2, Muzzle = 9, Receiver = "AK470 Rifle", Stock = DefaultStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 6  AK470 Rifle
        CompoundBow: new() { Barrel = NoBarrel, Grip = "", Magazine = 205, Ammo = 17, Muzzle = 0, Receiver = "Compound Bow", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },             // 7  Compound Bow
        M4XRifle: new() { Barrel = DefaultBarrel, Grip = "", Magazine = 185, Ammo = 2, Muzzle = 9, Receiver = "M4X Rifle", Stock = DefaultStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 8  M4X Rifle
        TacticalAssaultRifle: new() { Barrel = DefaultBarrel, Grip = "", Ammo = 2, Magazine = 212, Muzzle = 9, Receiver = "Tactical Assault Rifle", Stock = DefaultStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 9  Tactical Assault Rifle
        AssaultRifle: new() { Barrel = DefaultBarrel, Grip = "", Magazine = 9, Ammo = 2, Muzzle = 9, Receiver = "Assault Rifle", Stock = DefaultStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 10 Assault Rifle
        BoltActionRifle: new() { Barrel = DefaultBarrel, Grip = "", Magazine = 90, Ammo = 2, Muzzle = 9, Receiver = "Bolt-Action Rifle", Stock = DefaultStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 11 Bolt-Action Rifle
        LightMachineGun: new() { Barrel = DefaultBarrel, Grip = "", Magazine = 39, Ammo = 2, Muzzle = 9, Receiver = "Light Machine Gun", Stock = DefaultStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 12 Light Machine Gun
        BurstfireRifle: new() { Barrel = DefaultBarrel, Grip = "", Magazine = 81, Ammo = 2, Muzzle = 9, Receiver = "Burstfire Rifle", Stock = DefaultBullPupStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 13 Burstfire Rifle
        PrestigeAssaultRifle: new() { Barrel = "Prestige Frontier Standard Barrel", Grip = "", Magazine = 226, Ammo = 2, Muzzle = 18, Receiver = "Prestige Assault Rifle", Stock = "Prestige Silverwood Standard Stock", Scope = "Prestige Titan Rail Sight", Tag = 0, Camo = 0 },    // 14 Prestige Assault Rifle
        SubmachineGun: new() { Barrel = DefaultBarrel, Grip = "", Magazine = 0, Ammo = 2, Muzzle = 9, Receiver = "Submachine Gun", Stock = DefaultStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 15 Submachine Gun
        CombatRifle: new() { Barrel = DefaultBarrel, Grip = "", Magazine = 20, Ammo = 2, Muzzle = 9, Receiver = "Combat Rifle", Stock = DefaultStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 16 Combat Rifle
        LightReconRifle: new() { Barrel = DefaultBarrel, Grip = "", Magazine = 221, Ammo = 2, Muzzle = 9, Receiver = "Light Recon Rifle", Stock = DefaultStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 17 Light Recon Rifle
        Shotgun_ARk: new() { Barrel = "Overmatch A-12 Blast", Grip = "", Magazine = 124, Ammo = 2, Muzzle = NoMuzzle, Receiver = "Shotgun AR-k", Stock = DefaultStock, Scope = "Titan Rail Sight", Tag = 0, Camo = 0 },             // 18 Shotgun AR-k
        BreechLoadedPistol: new() { Barrel = NoBarrel, Grip = "", Magazine = 134, Ammo = 9, Muzzle = NoMuzzle, Receiver = "Breech Loaded Pistol", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },                 // 19 Breech Loaded Pistol
        Snub260: new() { Barrel = NoBarrel, Grip = "", Magazine = 177, Ammo = 2, Muzzle = NoMuzzle, Receiver = "Snub 260", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },                 // 20 Snub 260
        HeavyPistol: new() { Barrel = NoBarrel, Grip = "", Magazine = 45, Ammo = 2, Muzzle = NoMuzzle, Receiver = "Heavy Pistol", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },                 // 21 Heavy Pistol
        LightPistol: new() { Barrel = NoBarrel, Grip = "", Magazine = 54, Ammo = 2, Muzzle = NoMuzzle, Receiver = "Light Pistol", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },                 // 22 Light Pistol
        BurstfirePistol: new() { Barrel = NoBarrel, Grip = "", Magazine = 72, Ammo = 2, Muzzle = NoMuzzle, Receiver = "Burstfire Pistol", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },                 // 23 Burstfire Pistol
        PrestigeLightPistol: new() { Barrel = NoBarrel, Grip = "", Magazine = 227, Ammo = 2, Muzzle = NoMuzzle, Receiver = "Prestige Light Pistol", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },                 // 24 Prestige Light Pistol
        MachinePistol: new() { Barrel = NoBarrel, Grip = "", Magazine = 63, Ammo = 2, Muzzle = NoMuzzle, Receiver = "Machine Pistol", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },                 // 25 Machine Pistol
        Revolver: new() { Barrel = NoBarrel, Grip = "", Magazine = 99, Ammo = 2, Muzzle = NoMuzzle, Receiver = "Revolver", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },                 // 26 Revolver
        Shotgun: new() { Barrel = "Titan FFB", Grip = "Briar BrGR1", Magazine = 29, Ammo = 2, Muzzle = NoMuzzle, Receiver = "Shotgun", Stock = DefaultStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },                 // 27 Shotgun
        HardsuitHRVDecoy: new() { Barrel = NoBarrel, Grip = "", Magazine = -1, Ammo = -1, Muzzle = NoMuzzle, Receiver = "Hardsuit HRV Decoy", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },
        RocketStinger: new() { Barrel = NoBarrel, Grip = "", Magazine = -1, Ammo = -1, Muzzle = NoMuzzle, Receiver = "Rocket Stinger", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },
        RocketSwarm: new() { Barrel = NoBarrel, Grip = "", Magazine = -1, Ammo = -1, Muzzle = NoMuzzle, Receiver = "Rocket Swarm", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },
        Railgun: new() { Barrel = NoBarrel, Grip = "", Magazine = -1, Ammo = -1, Muzzle = NoMuzzle, Receiver = "Railgun", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },
        Minigun: new() { Barrel = NoBarrel, Grip = "", Magazine = -1, Ammo = -1, Muzzle = NoMuzzle, Receiver = "Minigun", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },
        Turret: new() { Barrel = NoBarrel, Grip = "", Magazine = -1, Ammo = -1, Muzzle = NoMuzzle, Receiver = "Turret", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },
        RhinoHardsuit: new() { Barrel = NoBarrel, Grip = "", Magazine = -1, Ammo = -1, Muzzle = NoMuzzle, Receiver = "Rhino Hardsuit", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },
        GrenadeLauncher: new() { Barrel = NoBarrel, Grip = "", Magazine = -1, Ammo = -1, Muzzle = NoMuzzle, Receiver = "Grenade Launcher", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },
        Flamethrower: new() { Barrel = NoBarrel, Grip = "", Magazine = -1, Ammo = -1, Muzzle = NoMuzzle, Receiver = "Flamethrower", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },
        Katana: new() { Barrel = NoBarrel, Grip = "", Magazine = -1, Ammo = -1, Muzzle = NoMuzzle, Receiver = "Katana", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },
        Airstrike: new() { Barrel = NoBarrel, Grip = "", Magazine = -1, Ammo = -1, Muzzle = NoMuzzle, Receiver = "Airstrike", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },
        GunmanHardsuit: new() { Barrel = NoBarrel, Grip = "", Magazine = -1, Ammo = -1, Muzzle = NoMuzzle, Receiver = "Gunman Hardsuit", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 },
        MK1AssaultAI: new() { Barrel = NoBarrel, Grip = "", Magazine = -1, Ammo = -1, Muzzle = NoMuzzle, Receiver = "MK1 Assault AI", Stock = NoStock, Scope = "No Optic Mod", Tag = 0, Camo = 0 }
    );

    public const int NoMuzzle = 0;
    public const int NoMagazine = -1;
    public const int NoTag = 0;
    public const int NoCamo = 0;
    public const string NoBarrel = "No Barrel Mod";
    public const string NoGrip = "";
    public const string NoStock = "No Stock";
    public const string NoScope = "No Optic Mod";


    public const string DefaultBarrel = "Frontier Standard Barrel";
    public const string DefaultStock = "Silverwood Standard Stock";
    public const string DefaultBullPupStock = "MMRS BP-SR Tactical";

    public event EventHandler? WasWrittenTo;
}