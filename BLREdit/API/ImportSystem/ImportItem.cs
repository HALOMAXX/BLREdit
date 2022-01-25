using System;
using System.Text;
using System.Windows.Media.Imaging;

namespace BLREdit
{
    public class ImportItem
    {
        public string Category { get; set; }
        public string _class { get; set; }
        public string icon { get; set; }
        public BitmapSource WideImage { get; private set; }
        public BitmapSource LargeSquareImage { get; private set; }
        public BitmapSource SmallSquareImage { get; private set; }
        public BitmapSource Crosshair { get; private set; }
        public BitmapSource MiniCrosshair { get { return GetBitmapCrosshair(name); } }
        public string name { get; set; }
        public string descriptorName { get; set; } = "";
        public string DescriptorName { get { return GetDescriptorName(); } }
        public PawnModifiers pawnModifiers { get; set; }
        public string tooltip { get; set; }
        public int uid { get; set; }
        public int[] validFor { get; set; }
        public WeaponModifiers weaponModifiers { get; set; } = new WeaponModifiers();
        public string[] supportedMods { get; set; }
        public ImportStats stats { get; set; }
        public WikiStats WikiStats { get; set; }
        public IniStats IniStats { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[Class={0}, Icon={1}, Name={2}, PawnModifiers={3}, Tooltip={4}, UID={5}, ValidFor={6}, WeaponModifiers={7}, SupportedMods={8}, Stats={9}]", _class, icon, name, pawnModifiers, tooltip, uid, PrintIntArray(validFor), weaponModifiers, PrintStringArray(supportedMods), stats);
            return sb.ToString();
        }

        public string GetDescriptorName() 
        {
            if (!string.IsNullOrEmpty(descriptorName))
            { return descriptorName + ":" + weaponModifiers.rating; }
            string desc = "";
            int i = 0;
            foreach (StatDecriptor st in IniStats.StatDecriptors)
            {
                if (i <= 0)
                {
                    desc += st.Name;
                    i++;
                }
                else
                {
                    desc += "/" + st.Name;
                }
            }
            return desc;
        }
        public string GetDescriptorName(int points)
        {
            string currentbest = "";
            foreach (StatDecriptor st in IniStats.StatDecriptors)
            {
                if (points >= st.Points)
                { 
                    currentbest = st.Name;
                }
            }
            return currentbest;
        }

        public bool IsValidFor(ImportItem item)
        {
            if (validFor == null || item == null) { return true; }
            foreach (int id in validFor)
            {
                if (id == item.uid)
                { return true; }
            }
            return false;
        }

        public void LoadImage()
        {
            if (!string.IsNullOrEmpty(icon))
            {
                foreach (FoxIcon foxicon in ImportSystem.Icons)
                {
                    if (foxicon.Name == icon)
                    {
                        WideImage = foxicon.GetWideImage();
                        LargeSquareImage = foxicon.GetLargeSquareImage();
                        SmallSquareImage = foxicon.GetSmallSquareImage();
                        return;
                    }
                }
            }
            WideImage = FoxIcon.CreateEmptyBitmap(256, 128);
            LargeSquareImage = FoxIcon.CreateEmptyBitmap(128, 128);
            SmallSquareImage = FoxIcon.CreateEmptyBitmap(64, 64);
        }

        public void LoadCrosshair()
        {
            Crosshair = GetBitmapCrosshair(name);
        }

        public void RemoveCrosshair()
        {
            Crosshair = null;
        }

        public static BitmapSource GetBitmapCrosshair(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                foreach (FoxIcon icon in ImportSystem.Crosshairs)
                {
                    if (icon.Name.Equals(name))
                    {
                        return new BitmapImage(icon.Icon);
                    }
                }
            }
            return FoxIcon.CreateEmptyBitmap(1, 1);
        }

        private static string  PrintIntArray(int[] ints)
        {
            string array = "[";
            if (ints != null)
            {
                foreach (int i in ints)
                {
                    array += ' ' + i + ',';
                }
            }
            return array + ']';
        }

        private static string PrintStringArray(string[] strings)
        {
            string array = "[";
            if (strings != null)
            {
                foreach (string s in strings)
                {
                    array += ' ' + s + ',';
                }
            }
            return array + ']';
        }

        internal void PrepareImages()
        {
            WideImage = WideImage.Clone();
            LargeSquareImage = LargeSquareImage.Clone();
            SmallSquareImage = SmallSquareImage.Clone();
        }
    }
}
