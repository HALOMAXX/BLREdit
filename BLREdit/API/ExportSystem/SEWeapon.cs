namespace BLREdit
{
    public class SEWeapon
    {
        /// <summary>
        /// Contains the UnlockID(UID) of the Reciever
        /// </summary>
        public int Receiver { get; set; } = 40022;

        /// <summary>
        /// Contains the UnlockID(UID) of the Barrel
        /// </summary>
        public int Barrel { get; set; } = 41031;

        /// <summary>
        /// Contains the UnlockID(UID) of the Scope
        /// </summary>
        public int Scope { get; set; } = 45019;

        /// <summary>
        /// Contains the UnlockID(UID) of the Grip
        /// </summary>
        public int Grip { get; set; } = 62000;

        /// <summary>
        /// Contains the UnlockID(UID) of the Stock
        /// </summary>
        public int Stock { get; set; } = 42005;



        /// <summary>
        /// Contains the Index of the Ammo
        /// </summary>
        public int Ammo { get; set; } = 0;

        /// <summary>
        /// Contains the Index of the Muzzle
        /// </summary>
        public int Muzzle { get; set; } = 0;

        /// <summary>
        /// Contains the Index of the Magazine
        /// </summary>
        public int Magazine { get; set; } = 0;

        /// <summary>
        /// Contains the Index of the Skin
        /// </summary>
        public int Skin { get; set; } = 0;

        /// <summary>
        /// Contains the Index of the CamoIndex
        /// </summary>
        public int CamoIndex { get; set; } = 0;

        /// <summary>
        /// Contains the Index of the Hanger
        /// </summary>
        public int Hanger { get; set; } = 0;

        public static SEWeapon CreateFromMagiCowsWeapon(MagiCowsWeapon weapon)
        {
            return new SEWeapon { 
                Receiver = weapon.GetReciever().uid,
                Barrel = weapon.GetBarrel().uid,
                Scope = weapon.GetScope().uid,
                Grip = weapon.GetGrip()?.uid ?? 62000,
                Muzzle = weapon.Muzzle,
                Magazine = weapon.Magazine,
                CamoIndex = weapon.Camo,
                Hanger = weapon.Tag
            };
        }
    }
}
