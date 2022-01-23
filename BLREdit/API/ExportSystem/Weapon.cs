namespace BLREdit
{
    public class Weapon
    {
        public string Receiver { get; set; } = "Assault Rifle";
        public int Muzzle { get; set; } = 8;
        public string Stock { get; set; } = "Silverwood Standard Stock";
        public string Barrel { get; set; } = "Frontier Standard Barrel";
        public int Magazine { get; set; } = 9;
        public string Scope { get; set; } = "No Optic Mod";
        public string Grip { get; set; } = "";

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }

        public Weapon Copy()
        {
            return new Weapon()
            {
                Barrel = this.Barrel,
                Grip = this.Grip,
                Magazine = this.Magazine,
                Muzzle = this.Muzzle,
                Receiver = this.Receiver,
                Scope = this.Scope,
                Stock = this.Stock
            };
        }
      
        public ImportItem GetReciever()
        {
            return GetReciever(this);
        }
        public static ImportItem GetReciever(Weapon weapon)
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

        public ImportItem GetMuzzle()
        {
            return GetMuzzle(this);
        }
        public ImportItem GetMuzzle(Weapon weapon)
        {
            if (Muzzle < 0)
            { return null; }
            return ImportSystem.Mods.muzzles[weapon.Muzzle];
        }

        public ImportItem GetStock()
        {
            return GetStock(this);
        }
        public static ImportItem GetStock(Weapon weapon)
        {
            ImportItem item = null;
            foreach (ImportItem stock in ImportSystem.Mods.stocks)
            {
                if (stock.name == weapon.Stock)
                {
                    item = stock;
                }
            }
            return item;
        }

        public ImportItem GetBarrel()
        {
            return GetBarrel(this);
        }
        public static ImportItem GetBarrel(Weapon weapon)
        {
            ImportItem item = null;
            foreach (ImportItem barrel in ImportSystem.Mods.barrels)
            {
                if (barrel.name == weapon.Barrel)
                {
                    item = barrel;
                }
            }
            return item;
        }

        public ImportItem GetMagazine()
        {
            return GetMagazine(this);
        }
        public static ImportItem GetMagazine(Weapon weapon)
        {
            if (weapon.Magazine < 0)
            { return null; }
            return ImportSystem.Mods.magazines[weapon.Magazine];
        }

        public ImportItem GetScope()
        {
            return GetScope(this);
        }
        public static ImportItem GetScope(Weapon weapon)
        {
            ImportItem item = null;
            foreach (ImportItem scope in ImportSystem.Mods.scopes)
            {
                if (scope.name == weapon.Scope)
                {
                    item = scope;
                }
            }
            return item;
        }

        public ImportItem GetGrip()
        {
            return GetGrip(this);
        }
        public static ImportItem GetGrip(Weapon weapon)
        {
            ImportItem item = null;
            foreach (ImportItem grip in ImportSystem.Mods.grips)
            {
                if (grip.name == weapon.Grip)
                {
                    item = grip;
                }
            }
            return item;
        }

        public static Weapon GetDefaultSetupOfReciever(ImportItem item)
        {
            foreach (Weapon weapon in DefaultWeapons)
            {
                if (weapon.Receiver == item.name)
                {
                    return weapon.Copy();
                }
            }
            return null;
        }

        private static readonly Weapon[] DefaultWeapons = new Weapon[]
        {
            new Weapon() { Barrel = "Frontier Standard Barrel",             Grip = "", Magazine = 115,  Muzzle = 9,     Receiver = "Heavy Assault Rifle",       Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 0  Heavy Assault Rifle
            new Weapon() { Barrel = "Frontier Standard Barrel",             Grip = "", Magazine = 104,  Muzzle = 9,     Receiver = "LMG Recon",                 Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 1  LMG Recon
            new Weapon() { Barrel = "Frontier Standard Barrel",             Grip = "", Magazine = 107,  Muzzle = 9,     Receiver = "Tactical SMG",              Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 2  Tactical SMG
            new Weapon() { Barrel = "Frontier Standard Barrel",             Grip = "", Magazine = 140,  Muzzle = 9,     Receiver = "Burstfire SMG",             Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 3  Burstfire SMG
            new Weapon() { Barrel = "No Barrel Mod",                        Grip = "", Magazine = -1,   Muzzle = 0,     Receiver = "Anti-Materiel Rifle",       Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 4  Anti-Materiel Rifle
            new Weapon() { Barrel = "No Barrel Mod",                        Grip = "", Magazine = 149,  Muzzle = 9,     Receiver = "Bullpup Full Auto",         Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 5  Bullpup Full Auto
            new Weapon() { Barrel = "Frontier Standard Barrel",             Grip = "", Magazine = 195,  Muzzle = 9,     Receiver = "AK470 Rifle",               Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 6  AK470 Rifle
            new Weapon() { Barrel = "No Barrel Mod",                        Grip = "", Magazine = 205,  Muzzle = 0,     Receiver = "Compound Bow",              Stock = "No Stock",                             Scope="No Optic Mod" },                 // 7  Compound Bow
            new Weapon() { Barrel = "Frontier Standard Barrel",             Grip = "", Magazine = 185,  Muzzle = 9,     Receiver = "M4X Rifle",                 Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 8  M4X Rifle
            new Weapon() { Barrel = "Frontier Standard Barrel",             Grip = "", Magazine = 212,  Muzzle = 9,     Receiver = "Tactical Assault Rifle",    Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 9  Tactical Assault Rifle
            new Weapon() { Barrel = "Frontier Standard Barrel",             Grip = "", Magazine = 9,    Muzzle = 9,     Receiver = "Assault Rifle",             Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 10 Assault Rifle
            new Weapon() { Barrel = "Frontier Standard Barrel",             Grip = "", Magazine = 90,   Muzzle = 9,     Receiver = "Bolt-Action Rifle",         Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 11 Bolt-Action Rifle
            new Weapon() { Barrel = "Frontier Standard Barrel",             Grip = "", Magazine = 39,   Muzzle = 9,     Receiver = "Light Machine Gun",         Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 12 Light Machine Gun
            new Weapon() { Barrel = "Frontier Standard Barrel",             Grip = "", Magazine = 81,   Muzzle = 9,     Receiver = "Burstfire Rifle",           Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 13 Burstfire Rifle
            new Weapon() { Barrel = "Prestige Frontier Standard Barrel",    Grip = "", Magazine = 226,  Muzzle = 18,    Receiver = "Prestige Assault Rifle",    Stock = "Prestige Silverwood Standard Stock",   Scope="Prestige Titan Rail Sight" },    // 14 Prestige Assault Rifle
            new Weapon() { Barrel = "Frontier Standard Barrel",             Grip = "", Magazine = 0,    Muzzle = 9,     Receiver = "Submachine Gun",            Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 15 Submachine Gun
            new Weapon() { Barrel = "Frontier Standard Barrel",             Grip = "", Magazine = 20,   Muzzle = 9,     Receiver = "Combat Rifle",              Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 16 Combat Rifle
            new Weapon() { Barrel = "Frontier Standard Barrel",             Grip = "", Magazine = 221,  Muzzle = 9,     Receiver = "Light Recon Rifle",         Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 17 Light Recon Rifle
            new Weapon() { Barrel = "No Barrel Mod",                        Grip = "", Magazine = 124,  Muzzle = 0,     Receiver = "Shotgun AR-k",              Stock = "Silverwood Standard Stock",            Scope="Titan Rail Sight" },             // 18 Shotgun AR-k
            new Weapon() { Barrel = "No Barrel Mod",                        Grip = "", Magazine = 134,  Muzzle = 0,     Receiver = "Breech Loaded Pistol",      Stock = "No Stock",                             Scope="No Optic Mod" },                 // 19 Breech Loaded Pistol
            new Weapon() { Barrel = "No Barrel Mod",                        Grip = "", Magazine = 177,  Muzzle = 0,     Receiver = "Snub 260",                  Stock = "No Stock",                             Scope="No Optic Mod" },                 // 20 Snub 260
            new Weapon() { Barrel = "No Barrel Mod",                        Grip = "", Magazine = 45,   Muzzle = 0,     Receiver = "Heavy Pistol",              Stock = "No Stock",                             Scope="No Optic Mod" },                 // 21 Heavy Pistol
            new Weapon() { Barrel = "No Barrel Mod",                        Grip = "", Magazine = 54,   Muzzle = 0,     Receiver = "Light Pistol",              Stock = "No Stock",                             Scope="No Optic Mod" },                 // 22 Light Pistol
            new Weapon() { Barrel = "No Barrel Mod",                        Grip = "", Magazine = 72,   Muzzle = 0,     Receiver = "Burstfire Pistol",          Stock = "No Stock",                             Scope="No Optic Mod" },                 // 23 Burstfire Pistol
            new Weapon() { Barrel = "No Barrel Mod",                        Grip = "", Magazine = 227,  Muzzle = 0,     Receiver = "Prestige Light Pistol",     Stock = "No Stock",                             Scope="No Optic Mod" },                 // 24 Prestige Light Pistol
            new Weapon() { Barrel = "No Barrel Mod",                        Grip = "", Magazine = 63,   Muzzle = 0,     Receiver = "Machine Pistol",            Stock = "No Stock",                             Scope="No Optic Mod" },                 // 25 Machine Pistol
            new Weapon() { Barrel = "No Barrel Mod",                        Grip = "", Magazine = 99,   Muzzle = 0,     Receiver = "Revolver",                  Stock = "No Stock",                             Scope="No Optic Mod" },                 // 26 Revolver
            new Weapon() { Barrel = "No Barrel Mod",                        Grip = "", Magazine = 29,   Muzzle = 0,     Receiver = "Shotgun",                   Stock = "Silverwood Standard Stock",            Scope="No Optic Mod" },                 // 27 Shotgun
        };

        public const string NoBarrel = "No Barrel Mod";
        public const string NoGrip = "";
        public const string NoStock = "No Stock";
        public const string NoScope = "No Optic Mod";

        public static Weapon DefaultAssaultRifle { get { return DefaultWeapons[10].Copy(); } }
        public static Weapon DefaultPrestigeAssaultRifle { get { return DefaultWeapons[14].Copy(); } }
        public static Weapon DefaultSubmachineGun { get { return DefaultWeapons[15].Copy(); } }
        public static Weapon DefaultBAR { get { return DefaultWeapons[11].Copy(); } }
        public static Weapon DefaultLightPistol { get { return DefaultWeapons[22].Copy(); } }
    }
}