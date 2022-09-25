using BLREdit.Import;

using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BLREdit.Export;

public sealed class MagiCowsWeapon
{
    [JsonIgnore] private string reciever = "Assault Rifle";
    public string Receiver { get { return reciever; } set { if (reciever != value) { reciever = value; isDirty = true; } } } 

    [JsonIgnore] private int muzzle = 8;
    public int Muzzle { get { return muzzle; } set { if (muzzle != value) { muzzle = value; isDirty = true; } } }

    [JsonIgnore] private string stock = "Silverwood Standard Stock";
    public string Stock { get { return stock; } set { if (stock != value) { stock = value; isDirty = true; } } }

    [JsonIgnore] private string barrel = "Frontier Standard Barrel";
    public string Barrel { get { return barrel; } set { if (barrel != ConvertBarrel(value)) { barrel = ConvertBarrel(value); isDirty = true; } } }

    [JsonIgnore] private int magazine = 9;
    public int Magazine { get { return magazine; } set { if (magazine != value) { magazine = value; isDirty = true; } } }

    [JsonIgnore] private int ammo = 9;
    public int Ammo { get { return ammo; } set { if (ammo != value) { ammo = value; isDirty = true; } } }

    [JsonIgnore] private string scope = "No Optic Mod";
    public string Scope { get { return scope; } set { if (scope != value) { scope = value; isDirty = true; } } }

    [JsonIgnore] private string grip = "";
    public string Grip { get { return grip; } set { if (grip != value) { grip = value; isDirty = true; } } }

    [JsonIgnore] private int tag = -1;
    public int Tag { get { return tag; } set { if (tag != value) { tag = value; isDirty = true; } } }

    [JsonIgnore] private int camo = -1;
    public int Camo { get { return camo; } set { if (camo != value) { camo = value; isDirty = true; } } }

    [JsonIgnore] private bool isDirty = true;
    [JsonIgnore] public bool IsDirty { get { return (isDirty); } set { isDirty = value; } }

    public static string ConvertBarrel(string Barrel)
    {
        switch (Barrel)
        {
            case "ArmCom BAR-01":
                return "Armcom BAR-01";
            case "ArmCom BARl1":
                return "Armcom BARl1";
            default:
                return Barrel;
        }
    }

    public static string ConvertBarrelBack(string Barrel)
    {
        switch (Barrel)
        {
            case "Armcom BAR-01":
                return "ArmCom BAR-01";
            case "Armcom BARl1":
                return "ArmCom BARl1";
            default:
                return Barrel;
        }
    }

    public override string ToString()
    {
        return LoggingSystem.ObjectToTextWall(this);
    }

    //Clones the string variables
    public MagiCowsWeapon Clone()
    {
        MagiCowsWeapon clone = this.MemberwiseClone() as MagiCowsWeapon;
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

    public BLRItem GetReciever()
    {
        BLRItem primary = ImportSystem.GetItemByNameAndType(ImportSystem.PRIMARY_CATEGORY, Receiver);
        if (primary != null)
            return primary;

        BLRItem secondary = ImportSystem.GetItemByNameAndType(ImportSystem.SECONDARY_CATEGORY, Receiver);
        if (secondary != null)
            return secondary;

        return null;
    }

    public BLRItem GetCamo()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_WEAPONS_CATEGORY, Camo);
    }
    public BLRItem GetTag()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.HANGERS_CATEGORY, Tag);
    }
    public BLRItem GetMagazine()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.MAGAZINES_CATEGORY, Magazine);
    }
    public BLRItem GetAmmo()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.AMMO_CATEGORY, Ammo);
    }
    public BLRItem GetMuzzle()
    {
        return ImportSystem.GetItemByIDAndType(ImportSystem.MUZZELS_CATEGORY, Muzzle);
    }
    public BLRItem GetStock()
    {
        return ImportSystem.GetItemByNameAndType(ImportSystem.STOCKS_CATEGORY, Stock) ?? ImportSystem.GetItemByNameAndType(ImportSystem.STOCKS_CATEGORY, NoStock);
    }
    public BLRItem GetBarrel()
    {
        return ImportSystem.GetItemByNameAndType(ImportSystem.BARRELS_CATEGORY, ConvertBarrelBack(Barrel)) ?? ImportSystem.GetItemByNameAndType(ImportSystem.BARRELS_CATEGORY, NoBarrel);
    }
    public BLRItem GetScope()
    {
        return ImportSystem.GetItemByNameAndType(ImportSystem.SCOPES_CATEGORY, Scope) ?? ImportSystem.GetItemByNameAndType(ImportSystem.SCOPES_CATEGORY, NoScope);
    }

    public BLRItem GetGrip()
    {
        return ImportSystem.GetItemByNameAndType(ImportSystem.GRIPS_CATEGORY, Grip);
    }

    public static MagiCowsWeapon GetDefaultSetupOfReciever(BLRItem item)
    {
        var tuple = DefaultWeapons as ITuple;
        for(int i = 0; i< tuple.Length; i++)
        {
            MagiCowsWeapon wpn = (tuple[i] as MagiCowsWeapon);
            if (wpn.Receiver == item.Name)
            {
                return wpn.Clone();
            }
        }
        return null;
    }

    public static readonly (MagiCowsWeapon HeavyAssaultRifle, MagiCowsWeapon LMGReacon, MagiCowsWeapon TacticalSMG, MagiCowsWeapon BurstfireSMG, MagiCowsWeapon AntiMaterielRifle, MagiCowsWeapon BullpupFullAuto, MagiCowsWeapon AK470Rifle, MagiCowsWeapon CompoundBow, MagiCowsWeapon M4XRifle, MagiCowsWeapon TacticalAssaultRifle, MagiCowsWeapon AssaultRifle, MagiCowsWeapon BoltActionRifle, MagiCowsWeapon LightMachineGun, MagiCowsWeapon BurstfireRifle, MagiCowsWeapon PrestigeAssaultRifle, MagiCowsWeapon SubmachineGun, MagiCowsWeapon CombatRifle, MagiCowsWeapon LightReconRifle, MagiCowsWeapon Shotgun_ARk, MagiCowsWeapon BreechLoadedPistol, MagiCowsWeapon Snub260, MagiCowsWeapon HeavyPistol, MagiCowsWeapon LightPistol, MagiCowsWeapon BurstfirePistol, MagiCowsWeapon PrestigeLightPistol, MagiCowsWeapon MachinePistol, MagiCowsWeapon Revolver, MagiCowsWeapon Shotgun, MagiCowsWeapon HardsuitHRVDecoy, MagiCowsWeapon RocketStinger, MagiCowsWeapon RocketSwarm, MagiCowsWeapon Railgun, MagiCowsWeapon Minigun, MagiCowsWeapon Turret, MagiCowsWeapon RhinoHardsuit, MagiCowsWeapon GrenadeLauncher, MagiCowsWeapon Flamethrower, MagiCowsWeapon Katana, MagiCowsWeapon Airstrike, MagiCowsWeapon GunmanHardsuit, MagiCowsWeapon MK1AssaultAI) DefaultWeapons = (
        HeavyAssaultRifle : new() { Barrel = DefaultBarrel,                          Grip = "",             Magazine = 115,  Muzzle = 9,            Receiver = "Heavy Assault Rifle",       Stock = DefaultStock,                           Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 0  Heavy Assault Rifle
        LMGReacon : new() { Barrel = DefaultBarrel,                          Grip = "",             Magazine = 104,  Muzzle = 9,            Receiver = "LMG Recon",                 Stock = DefaultStock,                           Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 1  LMG Recon
        TacticalSMG : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = 107,  Muzzle = 9,            Receiver = "Tactical SMG",              Stock = NoStock,                                Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 2  Tactical SMG
        BurstfireSMG : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = 140,  Muzzle = 9,            Receiver = "Burstfire SMG",             Stock = NoStock,                                Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 3  Burstfire SMG
        AntiMaterielRifle : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = -1,   Muzzle = NoMuzzle,   Receiver = "Anti-Materiel Rifle",       Stock = DefaultStock,                           Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 4  Anti-Materiel Rifle
        BullpupFullAuto : new() { Barrel = "No Grip",                              Grip = "",             Magazine = 149,  Muzzle = 9,            Receiver = "Bullpup Full Auto",         Stock = DefaultBullPupStock,                         Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 5  Bullpup Full Auto
        AK470Rifle : new() { Barrel = DefaultBarrel,                          Grip = "",             Magazine = 195,  Muzzle = 9,            Receiver = "AK470 Rifle",               Stock = DefaultStock,                           Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 6  AK470 Rifle
        CompoundBow : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = 205,  Muzzle = 0,            Receiver = "Compound Bow",              Stock = NoStock,                                Scope = "No Optic Mod"    ,           Tag = 0, Camo = 0},             // 7  Compound Bow
        M4XRifle : new() { Barrel = DefaultBarrel,                          Grip = "",             Magazine = 185,  Muzzle = 9,            Receiver = "M4X Rifle",                 Stock = DefaultStock,                           Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 8  M4X Rifle
        TacticalAssaultRifle : new() { Barrel = DefaultBarrel,                          Grip = "",             Magazine = 212,  Muzzle = 9,            Receiver = "Tactical Assault Rifle",    Stock = DefaultStock,                           Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 9  Tactical Assault Rifle
        AssaultRifle : new() { Barrel = DefaultBarrel,                          Grip = "",             Magazine = 9,    Muzzle = 9,            Receiver = "Assault Rifle",             Stock = DefaultStock,                           Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 10 Assault Rifle
        BoltActionRifle : new() { Barrel = DefaultBarrel,                          Grip = "",             Magazine = 90,   Muzzle = 9,            Receiver = "Bolt-Action Rifle",         Stock = DefaultStock,                           Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 11 Bolt-Action Rifle
        LightMachineGun : new() { Barrel = DefaultBarrel,                          Grip = "",             Magazine = 39,   Muzzle = 9,            Receiver = "Light Machine Gun",         Stock = DefaultStock,                           Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 12 Light Machine Gun
        BurstfireRifle : new() { Barrel = DefaultBarrel,                          Grip = "",             Magazine = 81,   Muzzle = 9,            Receiver = "Burstfire Rifle",           Stock = DefaultBullPupStock,                         Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 13 Burstfire Rifle
        PrestigeAssaultRifle : new() { Barrel = "Prestige Frontier Standard Barrel",    Grip = "",             Magazine = 226,  Muzzle = 18,           Receiver = "Prestige Assault Rifle",    Stock = "Prestige Silverwood Standard Stock",   Scope = "Prestige Titan Rail Sight",  Tag = 0, Camo = 0},    // 14 Prestige Assault Rifle
        SubmachineGun : new() { Barrel = DefaultBarrel,                          Grip = "",             Magazine = 0,    Muzzle = 9,            Receiver = "Submachine Gun",            Stock = DefaultStock,                           Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 15 Submachine Gun
        CombatRifle : new() { Barrel = DefaultBarrel,                          Grip = "",             Magazine = 20,   Muzzle = 9,            Receiver = "Combat Rifle",              Stock = DefaultStock,                           Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 16 Combat Rifle
        LightReconRifle : new() { Barrel = DefaultBarrel,                          Grip = "",             Magazine = 221,  Muzzle = 9,            Receiver = "Light Recon Rifle",         Stock = DefaultStock,                           Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 17 Light Recon Rifle
        Shotgun_ARk : new() { Barrel = "Overmatch A-12 Blast",                 Grip = "",             Magazine = 124,  Muzzle = NoMuzzle,   Receiver = "Shotgun AR-k",              Stock = DefaultStock,                           Scope = "Titan Rail Sight",           Tag = 0, Camo = 0},             // 18 Shotgun AR-k
        BreechLoadedPistol : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = 134,  Muzzle = NoMuzzle,   Receiver = "Breech Loaded Pistol",      Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},                 // 19 Breech Loaded Pistol
        Snub260 : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = 177,  Muzzle = NoMuzzle,   Receiver = "Snub 260",                  Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},                 // 20 Snub 260
        HeavyPistol : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = 45,   Muzzle = NoMuzzle,   Receiver = "Heavy Pistol",              Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},                 // 21 Heavy Pistol
        LightPistol : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = 54,   Muzzle = NoMuzzle,   Receiver = "Light Pistol",              Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},                 // 22 Light Pistol
        BurstfirePistol : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = 72,   Muzzle = NoMuzzle,   Receiver = "Burstfire Pistol",          Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},                 // 23 Burstfire Pistol
        PrestigeLightPistol : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = 227,  Muzzle = NoMuzzle,   Receiver = "Prestige Light Pistol",     Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},                 // 24 Prestige Light Pistol
        MachinePistol : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = 63,   Muzzle = NoMuzzle,   Receiver = "Machine Pistol",            Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},                 // 25 Machine Pistol
        Revolver : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = 99,   Muzzle = NoMuzzle,   Receiver = "Revolver",                  Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},                 // 26 Revolver
        Shotgun : new() { Barrel = "Titan FFB",                            Grip = "Briar BrGR1",  Magazine = 29,   Muzzle = NoMuzzle,   Receiver = "Shotgun",                   Stock = DefaultStock,                           Scope = "No Optic Mod",               Tag = 0, Camo = 0},                 // 27 Shotgun
        HardsuitHRVDecoy : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = -1,   Muzzle = NoMuzzle,   Receiver = "Hardsuit HRV Decoy",        Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},
        RocketStinger : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = -1,   Muzzle = NoMuzzle,   Receiver = "Rocket Stinger",            Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},
        RocketSwarm : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = -1,   Muzzle = NoMuzzle,   Receiver = "Rocket Swarm",              Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},
        Railgun : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = -1,   Muzzle = NoMuzzle,   Receiver = "Railgun",                   Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},
        Minigun : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = -1,   Muzzle = NoMuzzle,   Receiver = "Minigun",                   Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},
        Turret : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = -1,   Muzzle = NoMuzzle,   Receiver = "Turret",                    Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},
        RhinoHardsuit : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = -1,   Muzzle = NoMuzzle,   Receiver = "Rhino Hardsuit",            Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},
        GrenadeLauncher : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = -1,   Muzzle = NoMuzzle,   Receiver = "Grenade Launcher",          Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},
        Flamethrower : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = -1,   Muzzle = NoMuzzle,   Receiver = "Flamethrower",              Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},
        Katana : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = -1,   Muzzle = NoMuzzle,   Receiver = "Katana",                    Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},
        Airstrike : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = -1,   Muzzle = NoMuzzle,   Receiver = "Airstrike",                 Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},
        GunmanHardsuit : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = -1,   Muzzle = NoMuzzle,   Receiver = "Gunman Hardsuit",           Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0},
        MK1AssaultAI : new() { Barrel = NoBarrel,                               Grip = "",             Magazine = -1,   Muzzle = NoMuzzle,   Receiver = "MK1 Assault AI",            Stock = NoStock,                                Scope = "No Optic Mod",               Tag = 0, Camo = 0}
    );

    public const int    NoMuzzle = 0;
    public const int    NoMagazine = -1;
    public const int    NoTag = 0;
    public const int    NoCamo = 0;
    public const string NoBarrel = "No Barrel Mod";
    public const string NoGrip = "";
    public const string NoStock = "No Stock";
    public const string NoScope = "No Optic Mod";
    

    public const string DefaultBarrel = "Frontier Standard Barrel";
    public const string DefaultStock = "Silverwood Standard Stock";
    public const string DefaultBullPupStock = "MMRS BP-SR Tactical";
}