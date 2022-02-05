using System.Numerics;

namespace BLREdit
{
    public class IniStats
    {
        public int ItemID { get; set; } = 0;
        public string ItemName { get; set; } = "Nope";
        public double ROF { get; set; } = 0;
        public double Burst { get; set; } = 0;
        public double InitialMagazines { get; set; } = 4;
        public double MagSize { get; set; } = 30;
        public double ApplyTime { get; set; } = 0;
        public double RecoilSize { get; set; } = 0;
        public Vector3 RecoilVector { get; set; } = Vector3.Zero;
        public Vector3 RecoilVectorMultiplier { get; set; } = Vector3.Zero;
        public double RecoilAccumulation { get; set; } = 0;
        public double RecoilAccumulationMultiplier { get; set; } = 0.95f;
        public double RecoilZoomMultiplier { get; set; } = 0.5f;
        public double BaseSpread { get; set; } = 0.04f;
        public double TABaseSpread { get; set; } = 0;
        public double CrouchSpreadMultiplier { get; set; } = 0.5f;
        public double JumpSpreadMultiplier { get; set; } = 4.0f;
        public double MovementSpreadMultiplier { get; set; } = 2.5f;
        public double MovementSpreadConstant { get; set; } = 0.0f;
        public double Weight { get; set; } = 150.0f;
        public double ZoomSpreadMultiplier { get; set; } = 0.4f;
        public double IdealDistance { get; set; } = 8000;
        public double MaxDistance { get; set; } = 16384;
        public double MaxTraceDistance { get; set; } = 15000;
        public double MaxRangeDamageMultiplier { get; set; } = 0.1f;
        public bool UseTABaseSpread { get; set; } = false;
        public Vector3 ModificationRangeDamage { get; set; } = Vector3.Zero;
        public Vector3 ModificationRangeBaseSpread { get; set; } = Vector3.Zero;
        public Vector3 ModificationRangeTABaseSpread { get; set; } = Vector3.Zero;
        public Vector3 ModificationRangeRecoil { get; set; } = Vector3.Zero;
        public Vector3 ModificationRangeIdealDistance { get; set; } = Vector3.Zero;
        public Vector3 ModificationRangeMaxDistance { get; set; } = Vector3.Zero;
        public Vector3 ModificationRangeMoveSpeed { get; set; } = Vector3.Zero;
        public Vector3 ModificationRangeCockRate { get; set; } = Vector3.Zero;
        public Vector3 ModificationRangeRecoilReloadRate { get; set; } = Vector3.Zero;
        public StatDecriptor[] StatDecriptors { get; set; } = new StatDecriptor[] { new StatDecriptor() };

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }
    }

    public class StatDecriptor
    {
        public string Name { get; set; } = "Classic";
        public int Points { get; set; } = 0;
    }
}


