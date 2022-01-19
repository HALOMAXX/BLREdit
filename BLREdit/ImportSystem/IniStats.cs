using System.Numerics;

namespace BLREdit
{
    public class IniStats
    {
        public int ItemID { get; set; } = 0;
        public string ItemName { get; set; } = "Nope";
        public float ROF { get; set; } = 0;
        public float Burst { get; set; } = 0;
        public float ApplyTime { get; set; } = 0;
        public float RecoilSize { get; set; } = 0;
        public Vector3 RecoilVector { get; set; } = Vector3.Zero;
        public Vector3 RecoilVectorMultiplier { get; set; } = Vector3.Zero;
        public float RecoilAccumulation { get; set; } = 0;
        public float RecoilAccumulationMultiplier { get; set; } = 0.95f;
        public float BaseSpread { get; set; } = 0.04f;
        public float TABaseSpread { get; set; } = 0;
        public float CrouchSpreadMultiplier { get; set; } = 0.5f;
        public float JumpSpreadMultiplier { get; set; } = 4.0f;
        public float MovementSpreadMultiplier { get; set; } = 2.5f;
        public float ZoomSpreadMultiplier { get; set; } = 0.4f;
        public bool UseTABaseSpread { get; set; } = false;
        public Vector3 ModificationRangeDamage { get; set; } = Vector3.Zero;
        public Vector3 ModificationRangeBaseSpread { get; set; } = Vector3.Zero;
        public Vector3 ModificationRangeTABaseSpread { get; set; } = Vector3.Zero;
        public Vector3 ModificationRangeRecoil { get; set; } = Vector3.Zero;
    }
}
