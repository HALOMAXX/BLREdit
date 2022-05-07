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

        public static ImportItem GetGear(int GearID)
        {
            return ImportSystem.GetItemByIDAndType("attachments", GearID);
        }
        public ImportItem GetTactical()
        {
            return ImportSystem.GetItemByIDAndType("tactical", Tactical);
        }
        public ImportItem GetHelmet()
        {
            return ImportSystem.GetItemByIDAndType("helmets", Helmet);
        }

        public ImportItem GetUpperBody()
        {
            return ImportSystem.GetItemByIDAndType("upperBodies", UpperBody);
        }

        public ImportItem GetLowerBody()
        {
            return ImportSystem.GetItemByIDAndType("lowerBodies", LowerBody);
        }
        public ImportItem GetCamo()
        {
            return ImportSystem.GetItemByIDAndType("camosBody", Camo);
        }
        public ImportItem GetSkin()
        {
            return ImportSystem.GetItemByIDAndType("avatars", Skin);
        }
    }
}