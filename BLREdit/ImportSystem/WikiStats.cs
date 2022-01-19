using System.Text;

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
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[ ID={0}, Name={1}, Damage={2}, ROF={3}, Ammo={4}/{5}, Reload={6}, SwapRate={7}, Aim={8}, Hip={9}, Move={10}, Recoil={11}, Zoom={12}, Scope In={13}, Range={14}/{15}, Run={16}]",
                itemID, itemName, damage, firerate, ammoMag, ammoReserve, reload, swaprate, aimSpread, hipSpread, moveSpread, recoil, zoom, scopeInTime, rangeClose, rangeFar, run);
            return sb.ToString();
        }
    }
}
