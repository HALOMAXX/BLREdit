using System;
using System.Collections.ObjectModel;
using System.Numerics;

namespace BLREdit.Models;

public sealed class BLRWeaponStats : ModelBase
{
    public double Accuracy { get; set; }
    public double Damage { get; set; }
    public double MovementSpeed { get; set; }
    public double Range { get; set; }
    public double RateOfFire { get; set; }
    public double Recoil { get; set; }
    public double ReloadSpeed { get; set; }
    public double WeaponWeight { get; set; }

    public double ApplyTime { get; set; } = 0;
    public double RecoveryTime { get; set; } = 0;
    public double BaseSpread { get; set; } = 0.04f;
    public double Burst { get; set; } = 0;
    public double FragmentsPerShell { get; set; } = 1;
    public double ZoomRateOfFire { get; set; } = 0;
    public double CrouchSpreadMultiplier { get; set; } = 0.5f;
    public double InitialMagazines { get; set; } = 4;
    public double IdealDistance { get; set; } = 8000;
    public double JumpSpreadMultiplier { get; set; } = 4.0f;
    public double SpreadCenterWeight { get; set; } = 0.2f;
    public double SpreadCenter { get; set; } = 0.4f;
    public double MagSize { get; set; } = 30;
    public double MaxDistance { get; set; } = 16384;
    public double MaxRangeDamageMultiplier { get; set; } = 0.1f;
    public double MaxTraceDistance { get; set; } = 15000;
    public Vector3 ModificationRangeBaseSpread { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeCockRate { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeDamage { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeIdealDistance { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeMaxDistance { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeMoveSpeed { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeRecoil { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeReloadRate { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeRecoilReloadRate { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeTABaseSpread { get; set; } = Vector3.Zero;
    public Vector3 ModificationRangeWeightMultiplier { get; set; } = Vector3.Zero;
    public double MovementSpreadConstant { get; set; } = 0.0f;
    public double MovementSpreadMultiplier { get; set; } = 2.5f;
    public double RecoilAccumulation { get; set; } = 0;
    public double RecoilAccumulationMultiplier { get; set; } = 0.95f;
    public double RecoilSize { get; set; } = 0;
    public Vector3 RecoilVector { get; set; } = Vector3.Zero;
    public Vector3 RecoilVectorMultiplier { get; set; } = Vector3.Zero;
    public double RecoilZoomMultiplier { get; set; } = 0.5f;
    public double ReloadShortMultiplier { get; set; } = 1.0f; // not actually a thing, but this is currently the easiest way with how we do the reload numbers
    public double ROF { get; set; } = 0;
    public RangeObservableCollection<BLRWeaponStatDecriptor> StatDecriptors { get; set; } = new();
    public double TABaseSpread { get; set; } = 0;
    public double TightAimTime { get; set; } = 0.0f;
    public bool UseTABaseSpread { get; set; } = false;
    public double Weight { get; set; } = 150.0f;
    public double ZoomSpreadMultiplier { get; set; } = 0.4f;
}
