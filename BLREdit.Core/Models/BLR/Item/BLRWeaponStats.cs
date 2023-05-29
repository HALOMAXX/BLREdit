using BLREdit.Core.Utils;

using System.Collections.ObjectModel;
using System.Numerics;

namespace BLREdit.Core.Models.BLR.Item;

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
    public double ApplyTime { get; set; }
    public double RecoveryTime { get; set; }
    public double BaseSpread { get; set; } = 0.04d;
    public double Burst { get; set; }
    public double FragmentsPerShell { get; set; } = 1;
    public double ZoomRateOfFire { get; set; }
    public double CrouchSpreadMultiplier { get; set; } = 0.5d;
    public double InitialMagazines { get; set; } = 4;
    public double IdealDistance { get; set; } = 8000;
    public double JumpSpreadMultiplier { get; set; } = 4.0d;
    public double SpreadCenterWeight { get; set; } = 0.2d;
    public double SpreadCenter { get; set; } = 0.4d;
    public double MagSize { get; set; } = 30;
    public double MaxDistance { get; set; } = 16384;
    public double MaxRangeDamageMultiplier { get; set; } = 0.1d;
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
    public double MovementSpreadConstant { get; set; }
    public double MovementSpreadMultiplier { get; set; } = 2.5d;
    public double RecoilAccumulation { get; set; }
    public double RecoilAccumulationMultiplier { get; set; } = 0.95d;
    public double RecoilSize { get; set; }
    public Vector3 RecoilVector { get; set; } = Vector3.Zero;
    public Vector3 RecoilVectorMultiplier { get; set; } = Vector3.Zero;
    public double RecoilZoomMultiplier { get; set; } = 0.5d;
    public double ReloadShortMultiplier { get; set; } = 1.0d; // not actually a thing, but this is currently the easiest way with how we do the reload numbers
    public double ROF { get; set; }
    public RangeObservableCollection<BLRWeaponStatDecriptor> StatDecriptors { get; set; } = new();
    public double TABaseSpread { get; set; }
    public double TightAimTime { get; set; }
    public bool UseTABaseSpread { get; set; }
    public double Weight { get; set; } = 150.0d;
    public double ZoomSpreadMultiplier { get; set; } = 0.4d;

    public override bool Equals(object? obj)
    {
        if (obj is BLRWeaponStats stats)
        {
            return
                Accuracy.Equals(stats.Accuracy) &&
                Damage.Equals(stats.Damage) &&
                MovementSpeed.Equals(stats.MovementSpeed) &&
                Range.Equals(stats.Range) &&
                RateOfFire.Equals(stats.RateOfFire) &&
                Recoil.Equals(stats.Recoil) &&
                ReloadSpeed.Equals(stats.ReloadSpeed) &&
                WeaponWeight.Equals(stats.WeaponWeight) &&
                ApplyTime.Equals(stats.ApplyTime) &&
                RecoveryTime.Equals(stats.RecoveryTime) &&
                BaseSpread.Equals(stats.BaseSpread) &&
                Burst.Equals(stats.Burst) &&
                FragmentsPerShell.Equals(stats.FragmentsPerShell) &&
                ZoomRateOfFire.Equals(stats.ZoomRateOfFire) &&
                CrouchSpreadMultiplier.Equals(stats.CrouchSpreadMultiplier) &&
                InitialMagazines.Equals(stats.InitialMagazines) &&
                IdealDistance.Equals(stats.IdealDistance) &&
                JumpSpreadMultiplier.Equals(stats.JumpSpreadMultiplier) &&
                SpreadCenterWeight.Equals(stats.SpreadCenterWeight) &&
                SpreadCenter.Equals(stats.SpreadCenter) &&
                MagSize.Equals(stats.MagSize) &&
                MaxDistance.Equals(stats.MaxDistance) &&
                MaxRangeDamageMultiplier.Equals(stats.MaxRangeDamageMultiplier) &&
                MaxTraceDistance.Equals(stats.MaxTraceDistance) &&
                ModificationRangeBaseSpread.Equals(stats.ModificationRangeBaseSpread) &&
                ModificationRangeCockRate.Equals(stats.ModificationRangeCockRate) &&
                ModificationRangeDamage.Equals(stats.ModificationRangeDamage) &&
                ModificationRangeIdealDistance.Equals(stats.ModificationRangeIdealDistance) &&
                ModificationRangeMaxDistance.Equals(stats.ModificationRangeMaxDistance) &&
                ModificationRangeMoveSpeed.Equals(stats.ModificationRangeMoveSpeed) &&
                ModificationRangeRecoil.Equals(stats.ModificationRangeRecoil) &&
                ModificationRangeReloadRate.Equals(stats.ModificationRangeReloadRate) &&
                ModificationRangeRecoilReloadRate.Equals(stats.ModificationRangeRecoilReloadRate) &&
                ModificationRangeTABaseSpread.Equals(stats.ModificationRangeTABaseSpread) &&
                ModificationRangeWeightMultiplier.Equals(stats.ModificationRangeWeightMultiplier) &&
                MovementSpreadConstant.Equals(stats.MovementSpreadConstant) &&
                MovementSpreadMultiplier.Equals(stats.MovementSpreadMultiplier) &&
                RecoilAccumulation.Equals(stats.RecoilAccumulation) &&
                RecoilAccumulationMultiplier.Equals(stats.RecoilAccumulationMultiplier) &&
                RecoilSize.Equals(stats.RecoilSize) &&
                RecoilVector.Equals(stats.RecoilVector) &&
                RecoilVectorMultiplier.Equals(stats.RecoilVectorMultiplier) &&
                RecoilZoomMultiplier.Equals(stats.RecoilZoomMultiplier) &&
                ReloadShortMultiplier.Equals(stats.ReloadShortMultiplier) &&
                ROF.Equals(stats.ROF) &&
                TABaseSpread.Equals(stats.TABaseSpread) &&
                TightAimTime.Equals(stats.TightAimTime) &&
                UseTABaseSpread.Equals(stats.UseTABaseSpread) &&
                Weight.Equals(stats.Weight) &&
                ZoomSpreadMultiplier.Equals(stats.ZoomSpreadMultiplier) &&
                StatDecriptors.Count == stats.StatDecriptors.Count;
        }
        return false;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Accuracy);
        hash.Add(Damage);
        hash.Add(MovementSpeed);
        hash.Add(Range);
        hash.Add(RateOfFire);
        hash.Add(Recoil);
        hash.Add(ReloadSpeed);
        hash.Add(WeaponWeight);
        hash.Add(ApplyTime);
        hash.Add(RecoveryTime);
        hash.Add(BaseSpread);
        hash.Add(Burst);
        hash.Add(FragmentsPerShell);
        hash.Add(ZoomRateOfFire);
        hash.Add(CrouchSpreadMultiplier);
        hash.Add(InitialMagazines);
        hash.Add(IdealDistance);
        hash.Add(JumpSpreadMultiplier);
        hash.Add(SpreadCenterWeight);
        hash.Add(SpreadCenter);
        hash.Add(MagSize);
        hash.Add(MaxDistance);
        hash.Add(MaxRangeDamageMultiplier);
        hash.Add(MaxTraceDistance);
        hash.Add(ModificationRangeBaseSpread);
        hash.Add(ModificationRangeCockRate);
        hash.Add(ModificationRangeDamage);
        hash.Add(ModificationRangeIdealDistance);
        hash.Add(ModificationRangeMaxDistance);
        hash.Add(ModificationRangeMoveSpeed);
        hash.Add(ModificationRangeRecoil);


        hash.Add(ModificationRangeRecoilReloadRate);
        hash.Add(ModificationRangeReloadRate);

        hash.Add(ModificationRangeTABaseSpread);
        hash.Add(ModificationRangeWeightMultiplier);
        hash.Add(MovementSpreadConstant);
        hash.Add(MovementSpreadMultiplier);
        hash.Add(RecoilAccumulation);
        hash.Add(RecoilAccumulationMultiplier);
        hash.Add(RecoilSize);
        hash.Add(RecoilVector);
        hash.Add(RecoilVectorMultiplier);
        hash.Add(RecoilZoomMultiplier);
        hash.Add(ReloadShortMultiplier);
        hash.Add(ROF);
        hash.Add(TABaseSpread);
        hash.Add(TightAimTime);
        hash.Add(UseTABaseSpread);
        hash.Add(Weight);
        hash.Add(ZoomSpreadMultiplier);

        foreach(var itm in StatDecriptors)
            hash.Add(itm);

        return hash.ToHashCode();
    }
}

public class JsonBLRWeaponStatsConverter : JsonGenericConverter<BLRWeaponStats>
{
    static JsonBLRWeaponStatsConverter()
    {
        Default = new();
        IOResources.JSOSerialization.Converters.Add(new JsonBLRWeaponStatsConverter());
        IOResources.JSOSerializationCompact.Converters.Add(new JsonBLRWeaponStatsConverter());
    }
}