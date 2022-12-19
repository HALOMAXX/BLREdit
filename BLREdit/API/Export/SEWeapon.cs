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
        Receiver = GetLMID(weapon.Reciever);
        Barrel = GetLMID(weapon.Barrel);
        Scope = GetLMID(weapon.Scope);
        Grip = GetLMID(weapon.Grip);
        Stock = GetLMID(weapon.Stock);
        Muzzle = GetLMID(weapon.Muzzle);
        Magazine = GetLMID(weapon.Magazine);
        CamoIndex = GetLMID(weapon.Camo);
        Hanger = GetLMID(weapon.Tag);

        Ammo = GetLMID(weapon.Ammo);
        Skin = GetLMID(weapon.Skin);
    }

    public static int GetLMID(BLRItem item)
    {
        if (item is null) return -1;
        if (item.LMID != -1) return item.LMID;
        return ImportSystem.GetIDOfItem(item);
    }
}
