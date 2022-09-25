using BLREdit.Import;
using BLREdit.UI.Views;

namespace BLREdit.Export;

public sealed class SEWeapon
{
    /// <summary>
    /// Contains the UnlockID(UID) of the Reciever
    /// </summary>
    public int Receiver { get; set; } = 1;

    /// <summary>
    /// Contains the UnlockID(UID) of the Barrel
    /// </summary>
    public int Barrel { get; set; } = 0;

    /// <summary>
    /// Contains the UnlockID(UID) of the Scope
    /// </summary>
    public int Scope { get; set; } = 0;

    /// <summary>
    /// Contains the UnlockID(UID) of the Grip
    /// </summary>
    public int Grip { get; set; } = 0;

    /// <summary>
    /// Contains the UnlockID(UID) of the Stock
    /// </summary>
    public int Stock { get; set; } = 0;



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
    public int Skin { get; set; } = -1;

    /// <summary>
    /// Contains the Index of the CamoIndex
    /// </summary>
    public int CamoIndex { get; set; } = 0;

    /// <summary>
    /// Contains the Index of the Hanger
    /// </summary>
    public int Hanger { get; set; } = 0;

    public SEWeapon() { }

    public SEWeapon(BLRWeapon weapon)
    {
        Receiver = weapon?.Reciever?.LMID ?? -1;
        Barrel = weapon?.Barrel?.LMID ?? -1;
        Scope = weapon?.Scope?.LMID ?? -1;
        Grip = weapon?.Grip?.LMID ?? -1;
        Stock = weapon?.Stock?.LMID ?? -1;
        Muzzle = ImportSystem.GetIDOfItem(weapon.Muzzle);
        Magazine = ImportSystem.GetIDOfItem(weapon.Magazine);
        CamoIndex = ImportSystem.GetIDOfItem(weapon.Camo);
        Hanger = ImportSystem.GetIDOfItem(weapon.Tag);

        Ammo = weapon?.Ammo?.LMID ?? 0;

        //TODO (Weapon) Ammo Type according to magazine type (Arrows, Breech Loaded Pistol)
    }
}
