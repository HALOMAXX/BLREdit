using System;

namespace BLREdit
{
    public class MagiCowsLoadout : ICloneable
    {
        public MagiCowsWeapon Primary { get; set; } = (MagiCowsWeapon)MagiCowsWeapon.DefaultAssaultRifle.Clone();
        public MagiCowsWeapon Secondary { get; set; } = (MagiCowsWeapon)MagiCowsWeapon.DefaultLightPistol.Clone();
        public int Gear1 { get; set; } = 1;
        public int Gear2 { get; set; } = 2;
        public int Gear3 { get; set; } = 0;
        public int Gear4 { get; set; } = 0;
        public int Tactical { get; set; } = 0;
        public bool IsFemale { get; set; } = false;
        public int Helmet { get; set; } = 0;
        public int UpperBody { get; set; } = 0;
        public int LowerBody { get; set; } = 0;
        public int Camo { get; set; } = 0;
        public int Skin { get; set; } = 99;

        public int Trophy { get; set; } = 0;

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }

        public object Clone()
        {
            MagiCowsLoadout clone = (MagiCowsLoadout)this.MemberwiseClone();
            clone.Primary = (MagiCowsWeapon)this.Primary.Clone();
            clone.Secondary = (MagiCowsWeapon)this.Secondary.Clone();
            return clone;
        }

        public bool IsHealthOkAndRepair()
        {
            bool isHealthy = true;
            if (!Primary.IsHealthOkAndRepair())
            {
                isHealthy = false;
            }
            if (!Secondary.IsHealthOkAndRepair())
            {
                isHealthy = false;
            }
            return isHealthy;
        }

        public static MagiCowsLoadout DefaultLoadout1 { get; } = new MagiCowsLoadout() { Primary = MagiCowsWeapon.DefaultAssaultRifle, Secondary = MagiCowsWeapon.DefaultLightPistol };
        public static MagiCowsLoadout DefaultLoadout2 { get; } = new MagiCowsLoadout() { Primary = MagiCowsWeapon.DefaultSubmachineGun, Secondary = MagiCowsWeapon.DefaultLightPistol };
        public static MagiCowsLoadout DefaultLoadout3 { get; } = new MagiCowsLoadout() { Primary = MagiCowsWeapon.DefaultBAR, Secondary = MagiCowsWeapon.DefaultLightPistol };

        public static BLRItem GetGear(int GearID)
        {
            return ImportSystem.GetItemByIDAndType(ImportSystem.ATTACHMENTS_CATEGORY, GearID);
        }
        public BLRItem GetTactical()
        {
            return ImportSystem.GetItemByIDAndType(ImportSystem.TACTICAL_CATEGORY, Tactical);
        }
        public BLRItem GetHelmet()
        {
            return ImportSystem.GetItemByIDAndType(ImportSystem.HELMETS_CATEGORY, Helmet);
        }

        public BLRItem GetUpperBody()
        {
            return ImportSystem.GetItemByIDAndType(ImportSystem.UPPER_BODIES_CATEGORY, UpperBody);
        }

        public BLRItem GetLowerBody()
        {
            return ImportSystem.GetItemByIDAndType(ImportSystem.LOWER_BODIES_CATEGORY, LowerBody);
        }
        public BLRItem GetCamo()
        {
            return ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_BODIES_CATEGORY, Camo);
        }
        public BLRItem GetSkin()
        {
            return ImportSystem.GetItemByIDAndType(ImportSystem.AVATARS_CATEGORY, Skin);
        }

        public BLRItem GetTrophy()
        {
            return ImportSystem.GetItemByIDAndType(ImportSystem.BADGES_CATEGORY, Trophy);
        }
    }
}