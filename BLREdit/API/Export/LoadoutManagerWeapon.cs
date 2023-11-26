using BLREdit.Import;
using BLREdit.UI.Views;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BLREdit.Export;

public sealed class LoadoutManagerWeapon
{
    /// <summary>
    /// Contains the Index of the Reciever
    /// </summary>
    public int Receiver { get; set; } = 1;

    /// <summary>
    /// Contains the Index of the Barrel
    /// </summary>
    public int Barrel { get; set; } = 0;

    /// <summary>
    /// Contains the Index of the Scope
    /// </summary>
    public int Scope { get; set; } = 0;

    /// <summary>
    /// Contains the Index of the Grip
    /// </summary>
    public int Grip { get; set; } = 0;

    /// <summary>
    /// Contains the Index of the Stock
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

    private static Dictionary<string, PropertyInfo> Properties { get; } = GetAllProperties();
    private static Dictionary<string, PropertyInfo> GetAllProperties()
    {
        var props = new Dictionary<string, PropertyInfo>();
        var properties = typeof(LoadoutManagerWeapon).GetProperties().ToArray();
        foreach (var prop in properties)
        { 
            props.Add(prop.Name, prop);
        }
        return props;
    }
    public LoadoutManagerWeapon() { }

    /// <summary>
    /// Creates a Loadout-Manager readable Weapon
    /// </summary>
    /// <param name="weapon"></param>
    public LoadoutManagerWeapon(BLRWeapon weapon)
    {
        foreach (var part in BLRWeapon.WeaponPartInfo)
        {
            switch (part.Name)
            {
                case "Tag":
                    Properties["Hanger"].SetValue(this, BLRItem.GetLMID((BLRItem)part.GetValue(weapon)));
                    break;
                case "Camo":
                    Properties["CamoIndex"].SetValue(this, BLRItem.GetLMID((BLRItem)part.GetValue(weapon)));
                    break;
                case "Reciever":
                    Properties["Receiver"].SetValue(this, BLRItem.GetLMID((BLRItem)part.GetValue(weapon)));
                    break;
                default:
                    Properties[part.Name].SetValue(this, BLRItem.GetLMID((BLRItem)part.GetValue(weapon)));
                    break;
            }
        }
    }

    public BLRWeapon GetWeapon(bool isPrimary)
    {
        return new BLRWeapon(isPrimary)
        {
            Reciever = ImportSystem.GetItemByLMIDAndType(isPrimary ? ImportSystem.PRIMARY_CATEGORY : ImportSystem.SECONDARY_CATEGORY, Receiver),
            Barrel = ImportSystem.GetItemByLMIDAndType(ImportSystem.BARRELS_CATEGORY, Barrel),
            Muzzle = ImportSystem.GetItemByLMIDAndType(ImportSystem.MUZZELS_CATEGORY, Muzzle),
            Grip = ImportSystem.GetItemByLMIDAndType(ImportSystem.GRIPS_CATEGORY, Grip),
            Magazine = ImportSystem.GetItemByLMIDAndType(ImportSystem.MAGAZINES_CATEGORY, Magazine),
            Ammo = ImportSystem.GetItemByLMIDAndType(ImportSystem.AMMO_CATEGORY, Ammo),
            Tag = ImportSystem.GetItemByLMIDAndType(ImportSystem.HANGERS_CATEGORY, Hanger),
            Stock = ImportSystem.GetItemByLMIDAndType(ImportSystem.STOCKS_CATEGORY, Stock),
            Scope = ImportSystem.GetItemByLMIDAndType(ImportSystem.SCOPES_CATEGORY, Scope),
            Camo = ImportSystem.GetItemByLMIDAndType(ImportSystem.CAMOS_WEAPONS_CATEGORY, CamoIndex),
            Skin = ImportSystem.GetItemByLMIDAndType(ImportSystem.PRIMARY_SKIN_CATEGORY, Skin)
        };
    }
}
