namespace BLREdit
{
    public class MagiCowsWeapon
    {
        public string Receiver { get; set; } = "Assault Rifle";
        public int Muzzle { get; set; } = 8;
        public string Stock { get; set; } = "Silverwood Standard Stock";
        public string Barrel { get; set; } = "Frontier Standard Barrel";
        public int Magazine { get; set; } = 9;
        public string Scope { get; set; } = "No Optic Mod";
        public string Grip { get; set; } = "";
        public int Tag { get; set; } = 0;
        public int Camo { get; set; } = 0;

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }

        public MagiCowsWeapon Copy()
        {
            return new MagiCowsWeapon()
            {
                Barrel = Barrel,
                Grip = Grip,
                Magazine = Magazine,
                Muzzle = Muzzle,
                Receiver = Receiver,
                Scope = Scope,
                Stock = Stock,
                Tag = Tag,
                Camo = Camo
            };
        }



        public ImportItem GetReciever()
        {
            return GetReciever(this);
        }
        public static ImportItem GetReciever(MagiCowsWeapon weapon)
        {
            foreach (ImportItem primary in ImportSystem.Weapons.primary)
            {
                if (primary.name == weapon.Receiver)
                {
                    return primary;
                }
            }
            foreach (ImportItem secondary in ImportSystem.Weapons.secondary)
            {
                if (secondary.name == weapon.Receiver)
                {
                    return secondary;
                }
            }
            return null;
        }

        public ImportItem GetCamo()
        {
            return ImportSystem.GetItemByID(this.Camo, ImportSystem.Mods.camosBody);
        }
        public ImportItem GetTag()
        {
            return ImportSystem.GetItemByID(this.Tag, ImportSystem.Gear.hangers);
        }
        public ImportItem GetMagazine()
        {
            return ImportSystem.GetItemByID(this.Magazine, ImportSystem.Mods.magazines);
        }
        public ImportItem GetMuzzle()
        {
            return ImportSystem.GetItemByID(this.Muzzle, ImportSystem.Mods.muzzles);
        }
        public ImportItem GetStock()
        {
            return ImportSystem.GetItemByName(this.Stock, ImportSystem.Mods.stocks);
        }
        public ImportItem GetBarrel()
        {
            return ImportSystem.GetItemByName(this.Barrel, ImportSystem.Mods.barrels);
        }
        public ImportItem GetScope()
        {
            return ImportSystem.GetItemByName(this.Scope, ImportSystem.Mods.scopes);
        }
        public ImportItem GetGrip()
        {
            return ImportSystem.GetItemByName(this.Grip, ImportSystem.Mods.grips);
        }

        public static MagiCowsWeapon GetDefaultSetupOfReciever(ImportItem item)
        {
            foreach (MagiCowsWeapon weapon in DefaultWeapons)
            {
                if (weapon.Receiver == item.name)
                {
                    return weapon.Copy();
                }
            }
            return null;
        }

        private static readonly MagiCowsWeapon[] DefaultWeapons = new MagiCowsWeapon[]
        {
            new MagiCowsWeapon() { Barrel = DefaultBarrel,                          Grip = "", Magazine = 115,  Muzzle = 9,     Receiver = "Heavy Assault Rifle",       Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 0  Heavy Assault Rifle
            new MagiCowsWeapon() { Barrel = DefaultBarrel,                          Grip = "", Magazine = 104,  Muzzle = 9,     Receiver = "LMG Recon",                 Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 1  LMG Recon
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = 107,  Muzzle = 9,     Receiver = "Tactical SMG",              Stock = NoStock,                                Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 2  Tactical SMG
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = 140,  Muzzle = 9,     Receiver = "Burstfire SMG",             Stock = NoStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 3  Burstfire SMG
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = -1,   Muzzle = 0,     Receiver = "Anti-Materiel Rifle",       Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 4  Anti-Materiel Rifle
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = 149,  Muzzle = 9,     Receiver = "Bullpup Full Auto",         Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 5  Bullpup Full Auto
            new MagiCowsWeapon() { Barrel = DefaultBarrel,                          Grip = "", Magazine = 195,  Muzzle = 9,     Receiver = "AK470 Rifle",               Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 6  AK470 Rifle
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = 205,  Muzzle = 0,     Receiver = "Compound Bow",              Stock = NoStock,                                Scope="No Optic Mod"    , Tag=0, Camo=0 },             // 7  Compound Bow
            new MagiCowsWeapon() { Barrel = DefaultBarrel,                          Grip = "", Magazine = 185,  Muzzle = 9,     Receiver = "M4X Rifle",                 Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 8  M4X Rifle
            new MagiCowsWeapon() { Barrel = DefaultBarrel,                          Grip = "", Magazine = 212,  Muzzle = 9,     Receiver = "Tactical Assault Rifle",    Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 9  Tactical Assault Rifle
            new MagiCowsWeapon() { Barrel = DefaultBarrel,                          Grip = "", Magazine = 9,    Muzzle = 9,     Receiver = "Assault Rifle",             Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 10 Assault Rifle
            new MagiCowsWeapon() { Barrel = DefaultBarrel,                          Grip = "", Magazine = 90,   Muzzle = 9,     Receiver = "Bolt-Action Rifle",         Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 11 Bolt-Action Rifle
            new MagiCowsWeapon() { Barrel = DefaultBarrel,                          Grip = "", Magazine = 39,   Muzzle = 9,     Receiver = "Light Machine Gun",         Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 12 Light Machine Gun
            new MagiCowsWeapon() { Barrel = DefaultBarrel,                          Grip = "", Magazine = 81,   Muzzle = 9,     Receiver = "Burstfire Rifle",           Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 13 Burstfire Rifle
            new MagiCowsWeapon() { Barrel = "Prestige Frontier Standard Barrel",    Grip = "", Magazine = 226,  Muzzle = 18,    Receiver = "Prestige Assault Rifle",    Stock = "Prestige Silverwood Standard Stock",   Scope="Prestige Titan Rail Sight", Tag=0, Camo=0 },    // 14 Prestige Assault Rifle
            new MagiCowsWeapon() { Barrel = DefaultBarrel,                          Grip = "", Magazine = 0,    Muzzle = 9,     Receiver = "Submachine Gun",            Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 15 Submachine Gun
            new MagiCowsWeapon() { Barrel = DefaultBarrel,                          Grip = "", Magazine = 20,   Muzzle = 9,     Receiver = "Combat Rifle",              Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 16 Combat Rifle
            new MagiCowsWeapon() { Barrel = DefaultBarrel,                          Grip = "", Magazine = 221,  Muzzle = 9,     Receiver = "Light Recon Rifle",         Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 17 Light Recon Rifle
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = 124,  Muzzle = 0,     Receiver = "Shotgun AR-k",              Stock = DefaultStock,                           Scope="Titan Rail Sight", Tag=0, Camo=0 },             // 18 Shotgun AR-k
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = 134,  Muzzle = 0,     Receiver = "Breech Loaded Pistol",      Stock = NoStock,                                Scope="No Optic Mod", Tag=0, Camo=0 },                 // 19 Breech Loaded Pistol
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = 177,  Muzzle = 0,     Receiver = "Snub 260",                  Stock = NoStock,                                Scope="No Optic Mod", Tag=0, Camo=0 },                 // 20 Snub 260
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = 45,   Muzzle = 0,     Receiver = "Heavy Pistol",              Stock = NoStock,                                Scope="No Optic Mod", Tag=0, Camo=0 },                 // 21 Heavy Pistol
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = 54,   Muzzle = 0,     Receiver = "Light Pistol",              Stock = NoStock,                                Scope="No Optic Mod", Tag=0, Camo=0 },                 // 22 Light Pistol
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = 72,   Muzzle = 0,     Receiver = "Burstfire Pistol",          Stock = NoStock,                                Scope="No Optic Mod", Tag=0, Camo=0 },                 // 23 Burstfire Pistol
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = 227,  Muzzle = 0,     Receiver = "Prestige Light Pistol",     Stock = NoStock,                                Scope="No Optic Mod", Tag=0, Camo=0 },                 // 24 Prestige Light Pistol
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = 63,   Muzzle = 0,     Receiver = "Machine Pistol",            Stock = NoStock,                                Scope="No Optic Mod", Tag=0, Camo=0 },                 // 25 Machine Pistol
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = 99,   Muzzle = 0,     Receiver = "Revolver",                  Stock = NoStock,                                Scope="No Optic Mod", Tag=0, Camo=0 },                 // 26 Revolver
            new MagiCowsWeapon() { Barrel = NoBarrel,                               Grip = "", Magazine = 29,   Muzzle = 0,     Receiver = "Shotgun",                   Stock = DefaultStock,                           Scope="No Optic Mod", Tag=0, Camo=0 },                 // 27 Shotgun
        };

        public const string NoMuzzle = "No Muzzle Mod";
        public const int NoMuzzleID = 0;
        public const string NoBarrel = "No Barrel Mod";
        public const string NoGrip = "";
        public const string NoStock = "No Stock";
        public const string NoScope = "No Optic Mod";

        public const string DefaultBarrel = "Frontier Standard Barrel";
        public const string DefaultStock = "Silverwood Standard Stock";

        public static MagiCowsWeapon DefaultAssaultRifle { get { return DefaultWeapons[10].Copy(); } }
        public static MagiCowsWeapon DefaultPrestigeAssaultRifle { get { return DefaultWeapons[14].Copy(); } }
        public static MagiCowsWeapon DefaultSubmachineGun { get { return DefaultWeapons[15].Copy(); } }
        public static MagiCowsWeapon DefaultBAR { get { return DefaultWeapons[11].Copy(); } }
        public static MagiCowsWeapon DefaultLightPistol { get { return DefaultWeapons[22].Copy(); } }
    }
}