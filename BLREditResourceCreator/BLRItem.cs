namespace BLREditResourceCreator;

public sealed class BLRItem
{
#pragma warning disable IDE1006 // Naming Styles
    public string? _class { get; set; }
    public string? icon { get; set; }
    public string? name { get; set; }
    public string? tooltip { get; set; }
    public int uid { get; set; }
    public int type { get; set; } = -1;
    public int[]? validFor { get; set; }
    public string[]? supportedMods { get; set; }
    public Weaponmodifiers? weaponModifiers { get; set; }
    public Pawnmodifiers? pawnModifiers { get; set; }
    public WeaponStats? stats { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}

public sealed class Pawnmodifiers
{
    public int BodyDamageReduction { get; set; }
    public int ElectroProtection { get; set; }
    public int ExplosiveProtection { get; set; }
    public int GearSlots { get; set; }
    public int HRVDuration { get; set; }
    public int HRVRechargeRate { get; set; }
    public int Health { get; set; }
    public int HealthRecharge { get; set; }
    public int HelmetDamageReduction { get; set; }
    public int IncendiaryProtection { get; set; }
    public int InfraredProtection { get; set; }
    public int LegsDamageReduction { get; set; }
    public int MeleeProtection { get; set; }
    public int MeleeRange { get; set; }
    public int MovementSpeed { get; set; }
    public int PermanentHealthProtection { get; set; }
    public float SprintMultiplier { get; set; }
    public int Stamina { get; set; }
    public int SwitchWeaponSpeed { get; set; }
    public int ToxicProtection { get; set; }
}

public sealed class Weaponmodifiers
{
#pragma warning disable IDE1006 // Naming Styles
    public int accuracy { get; set; }
    public int ammo { get; set; }
    public int damage { get; set; }
    public int movementSpeed { get; set; }
    public int range { get; set; }
    public int rateOfFire { get; set; }
    public int rating { get; set; }
    public int recoil { get; set; }
    public int reloadSpeed { get; set; }
    public int switchWeaponSpeed { get; set; }
    public int weaponWeight { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}

public sealed class WeaponStats
{
#pragma warning disable IDE1006 // Naming Styles
    public float accuracy { get; set; }
    public float damage { get; set; }
    public float movementSpeed { get; set; }
    public float range { get; set; }
    public int rateOfFire { get; set; }
    public float recoil { get; set; }
    public float reloadSpeed { get; set; }
    public float weaponWeight { get; set; }
#pragma warning restore IDE1006 // Naming Styles
}