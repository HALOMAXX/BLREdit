using BLREdit.Import;
using BLREdit.UI;
using BLREdit.UI.Views;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace BLREdit.API.Export;

public sealed class ShareableWeapon : IBLRWeapon
{
    [JsonIgnore] public ShareableLoadout? Loadout { get; set; } = null;
    [JsonPropertyName("A1")] public int Ammo { get; set; } = 0;
    [JsonPropertyName("B1")] public int Barrel { get; set; } = 0;
    [JsonPropertyName("C1")] public int Camo { get; set; } = 0;
    [JsonPropertyName("G1")] public int Grip { get; set; } = 0;
    [JsonPropertyName("M1")] public int Muzzle { get; set; } = 0;
    [JsonPropertyName("M2")] public int Magazine { get; set; } = 0;
    [JsonPropertyName("R1")] public int Receiver { get; set; } = 1;
    [JsonPropertyName("S1")] public int Scope { get; set; } = 0;
    [JsonPropertyName("S2")] public int Stock { get; set; } = 0;
    [JsonPropertyName("S3")] public int Skin { get; set; } = -1;
    [JsonPropertyName("T1")] public int Tag { get; set; } = 0;

    private static Dictionary<string, PropertyInfo> Properties { get; } = GetAllProperties();

    public event EventHandler? WasWrittenTo;

    private static Dictionary<string, PropertyInfo> GetAllProperties()
    {
        var props = new Dictionary<string, PropertyInfo>();
        var properties = typeof(ShareableWeapon).GetProperties().ToArray();
        foreach (var prop in properties)
        {
            props.Add(prop.Name, prop);
        }
        return props;
    }
    public ShareableWeapon() { }

    /// <summary>
    /// Creates a Loadout-Manager readable Weapon
    /// </summary>
    /// <param name="weapon"></param>
    public ShareableWeapon(BLRWeapon weapon)
    {
        foreach (var part in BLRWeapon.WeaponPartInfo)
        {
            if (Properties.TryGetValue(part.Name, out PropertyInfo info))
            {
                info.SetValue(this, BLRItem.GetMagicCowsID((BLRItem)part.GetValue(weapon)));
            }
        }
    }

    public BLRWeapon ToBLRWeapon(bool isPrimary, BLRLoadout? loadout = null) 
    {
        return new BLRWeapon(isPrimary, loadout, this, true);
    }

    public ShareableWeapon Clone()
    {
        return new()
        { 
            Ammo = Ammo,
            Barrel = Barrel,
            Camo = Camo,
            Grip = Grip,
            Magazine = Magazine,
            Muzzle = Muzzle,
            Receiver = Receiver,
            Scope = Scope,
            Skin = Skin,
            Stock = Stock,
            Tag = Tag
        };
    }

    public void Read(BLRWeapon weapon)
    {
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.ReadWeapon)) return;
        UndoRedoSystem.CurrentlyBlockedEvents.Value = BlockEvents.All;
        weapon.Receiver = ImportSystem.GetItemByIDAndType(weapon.IsPrimary ? ImportSystem.PRIMARY_CATEGORY : ImportSystem.SECONDARY_CATEGORY, Receiver);
        weapon.Barrel = ImportSystem.GetItemByIDAndType(ImportSystem.BARRELS_CATEGORY, Barrel);
        weapon.Muzzle = ImportSystem.GetItemByIDAndType(ImportSystem.MUZZELS_CATEGORY, Muzzle);
        weapon.Magazine = ImportSystem.GetItemByIDAndType(ImportSystem.MAGAZINES_CATEGORY, Magazine);
        weapon.Stock = ImportSystem.GetItemByIDAndType(ImportSystem.STOCKS_CATEGORY, Stock);
        weapon.Scope = ImportSystem.GetItemByIDAndType(ImportSystem.SCOPES_CATEGORY, Scope);
        weapon.Grip = ImportSystem.GetItemByIDAndType(ImportSystem.GRIPS_CATEGORY, Grip);
        weapon.Ammo = ImportSystem.GetItemByIDAndType(ImportSystem.AMMO_CATEGORY, Ammo);
        weapon.Tag = ImportSystem.GetItemByIDAndType(ImportSystem.HANGERS_CATEGORY, Tag);
        weapon.Camo = ImportSystem.GetItemByIDAndType(ImportSystem.CAMOS_WEAPONS_CATEGORY, Camo);
        weapon.Skin = ImportSystem.GetItemByIDAndType(ImportSystem.PRIMARY_SKIN_CATEGORY, Skin);
        UndoRedoSystem.RestoreBlockedEvents();
    }

    public void Write(BLRWeapon weapon)
    {
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteWeapon)) return;
        if (Loadout is not null) { Loadout.LastModified = DateTime.Now; }
        Receiver = BLRItem.GetMagicCowsID(weapon.Receiver, -1);
        Barrel = BLRItem.GetMagicCowsID(weapon.Barrel, -1);
        Muzzle = BLRItem.GetMagicCowsID(weapon.Muzzle, -1);
        Magazine = BLRItem.GetMagicCowsID(weapon.Magazine, -1);
        Stock = BLRItem.GetMagicCowsID(weapon.Stock, -1);
        Scope = BLRItem.GetMagicCowsID(weapon.Scope, -1);
        Grip = BLRItem.GetMagicCowsID(weapon.Grip, -1);
        Ammo = BLRItem.GetMagicCowsID(weapon.Ammo, -1);
        Tag = BLRItem.GetMagicCowsID(weapon.Tag, -1);
        Camo = BLRItem.GetMagicCowsID(weapon.Camo, -1);
        Skin = BLRItem.GetMagicCowsID(weapon.Skin, -1);
        if (WasWrittenTo is not null && !UndoRedoSystem.UndoRedoSystemWorking) { WasWrittenTo(weapon, EventArgs.Empty); }
    }
}