using System.ComponentModel;

namespace BLREdit
{
    public enum ImportWeaponSortingType
    {
        None = 0,
        Name = 1,
        Damage = 2,
        Spread = 3,
        Recoil = 4,
        Range = 5,
        Run = 6,
        Zoom = 7,
        ScopeInTime = 8,
    }

    public enum ImportGearSortingType
    {
        None = 0,
        Name = 1,
        ElectroArmor=2,
        ToxicArmor=3,
        IncendiaryArmor=4,
        MeleeArmor=5,
        InfraredArmor=6,
    }

    public enum ImportTacticalSortingType
    {
        None = 0,
        Name = 1,
    }

    public enum ImportHelmetSortingType
    {
        None = 0,
        Name = 1,
        Health = 2,
        Head_Protection = 3,
        HRV_Duration = 4,
        HRV_Recharge = 5,
        Run = 6,
    }

    public enum ImportArmorSortingType
    {
        None = 0,
        Name = 1,
        Health = 2,
        GearSlots = 3,
        Run = 4,
    }
}
