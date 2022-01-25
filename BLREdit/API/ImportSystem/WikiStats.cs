namespace BLREdit
{
    public class WikiStats
    {
        public int itemID { get; set; }
        public string itemName { get; set; }
        public float damage { get; set; }
        public float firerate { get; set; }
        public float ammoMag { get; set; }
        public float ammoReserve { get; set; }
        public float reload { get; set; }
        public float swaprate { get; set; }
        public float aimSpread { get; set; }
        public float hipSpread { get; set; }
        public float moveSpread { get; set; }
        public float recoil { get; set; }
        public float zoom { get; set; }
        public float scopeInTime { get; set; }
        public float rangeClose { get; set; }
        public float rangeFar { get; set; }
        public float run { get; set; }

        public override string ToString()
        {
            return LoggingSystem.ObjectToTextWall(this);
        }
    }
}
