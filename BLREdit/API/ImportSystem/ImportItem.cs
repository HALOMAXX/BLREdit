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
        public BitmapSource WideImage { get { return GetWideImage(); } }
        public BitmapSource LargeSquareImage { get { return GetLargeSquareImage(); } }
        public BitmapSource SmallSquareImage { get { return GetSmallSquareImage(); } }

        public BitmapSource wideImageMale = null;
        public BitmapSource largeSquareImageMale = null;
        public BitmapSource smallSquareImageMale = null;

        public BitmapSource wideImageFemale = null;
        public BitmapSource largeSquareImageFemale = null;
        public BitmapSource smallSquareImageFemale = null;

        public BitmapSource Crosshair { get; private set; }
        public BitmapSource MiniCrosshair { get { return GetBitmapCrosshair(name); } }
        public string name { get; set; }
        public string descriptorName { get; set; } = "";
        public string DescriptorName { get { return GetDescriptorName(); } }
        public PawnModifiers pawnModifiers { get; set; }
        public string tooltip { get; set; }
        public int uid { get; set; }
        public int[] validFor { get; set; }
        public WeaponModifiers weaponModifiers { get; set; }
        public string[] supportedMods { get; set; }
        public ImportStats stats { get; set; }
        public WikiStats WikiStats { get; set; }
        public IniStats IniStats { get; set; }

        public string StatDisplay
        {
            get
            {
                if (stats != null)
                {
                    double[] damage = UI.MainWindow.CalculateDamage(this, 0);
                    double[] spread = UI.MainWindow.CalculateSpread(this, 0, 0);
                    double recoil = UI.MainWindow.CalculateRecoil(this, 0);
                    double[] range = UI.MainWindow.CalculateRange(this, 0);
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Damage: {0}/{1}\tAim: {2}\tHip: {3}\nMove: {4}\tRecoil: {5}\tRange:{6}/{7}", damage[0].ToString("0"), damage[1].ToString("0"), spread[0].ToString("0.00"), spread[1].ToString("0.00"), spread[2].ToString("0.00"), recoil.ToString("0.00"), range[0].ToString("0"), range[1].ToString("0"));
                    return sb.ToString();
                }
                else
                {
                    ImportItem reciever;
                    if (UI.MainWindow.LastSelectedImage.Name.Contains("Primary"))
                    {
                        reciever = UI.MainWindow.PrimaryReciever;
                    }
                    else
                    {
                        reciever = UI.MainWindow.SecondaryReciever;
                    }
                    if (reciever != null && (weaponModifiers != null && weaponModifiers.IsNotZero))
                    {
                        double[] damageBase = UI.MainWindow.CalculateDamage(reciever, 0);
                        double[] spreadBase = UI.MainWindow.CalculateSpread(reciever, 0, 0);
                        double recoilBase = UI.MainWindow.CalculateRecoil(reciever, 0);
                        double[] rangeBase = UI.MainWindow.CalculateRange(reciever, 0);

                        double[] damageNew = UI.MainWindow.CalculateDamage(reciever, weaponModifiers.damage / 100.0);
                        double[] spreadNew = UI.MainWindow.CalculateSpread(reciever, weaponModifiers.accuracy / 100.0, weaponModifiers.movementSpeed / 100.0);
                        double recoilNew = UI.MainWindow.CalculateRecoil(reciever, weaponModifiers.recoil / 100.0);
                        double[] rangeNew = UI.MainWindow.CalculateRange(reciever, weaponModifiers.range / 100.0);

                        double[] damage = new double[] { damageNew[0] - damageBase[0], damageNew[1] - damageBase[1] };
                        double[] spread = new double[] { spreadNew[0] - spreadBase[0], spreadNew[1] - spreadBase[1], spreadNew[2] - spreadBase[2] };
                        double recoil = recoilNew - recoilBase;
                        double[] range = new double[] { rangeNew[0] - rangeBase[0], rangeNew[1] - rangeBase[1] };

                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("Damage: {0}/{1}\tAim: {2}\tHip: {3}\nMove: {4}\tRecoil: {5}\tRange:{6}/{7}", damage[0].ToString("0"), damage[1].ToString("0"), spread[0].ToString("0.00"), spread[1].ToString("0.00"), spread[2].ToString("0.00"), recoil.ToString("0.00"), range[0].ToString("0"), range[1].ToString("0"));
                        return sb.ToString();
                    }
                    else
                    {
                        return "";
                    }
                }
            }
        }

        public double Damage { get { if (stats != null) { return stats?.damage ?? 0; } else { return weaponModifiers?.damage ?? 0; } } }
        public double Spread
        {
            get
            {
                if (stats != null)
                { return UI.MainWindow.CalculateSpread(this, 0, 0)[1]; }
                else { return weaponModifiers?.accuracy ?? 0; }
            }
        }
        public double Recoil
        {
            get
            {
                if (stats != null)
                { return UI.MainWindow.CalculateRecoil(this, 0); }
                else { return weaponModifiers?.recoil ?? 0; }
            }
        }

        public double Range
        {
            get
            {
                if (stats != null)
                { return UI.MainWindow.CalculateRange(this, 0)[0]; }
                else { return weaponModifiers?.range ?? 0; }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[Class={0}, Icon={1}, Name={2}, PawnModifiers={3}, Tooltip={4}, UID={5}, ValidFor={6}, WeaponModifiers={7}, SupportedMods={8}, Stats={9}]", _class, icon, name, pawnModifiers, tooltip, uid, PrintIntArray(validFor), weaponModifiers, PrintStringArray(supportedMods), stats);
            return sb.ToString();
        }

        public BitmapSource GetWideImage()
        {
            return GetImage(wideImageMale, wideImageFemale);
        }
        public BitmapSource GetLargeSquareImage()
        {
            return GetImage(largeSquareImageMale, largeSquareImageFemale);
        }
        public BitmapSource GetSmallSquareImage()
        {
            return GetImage(smallSquareImageMale, smallSquareImageFemale);
        }

        public static BitmapSource GetImage(BitmapSource male, BitmapSource female)
        {
            if (UI.MainWindow.ActiveLoadout.IsFemale)
            {
                if (female == null)
                { return male; }
                return female;
            }
            else
            {
                return male;
            }
        }

        public string GetDescriptorName()
        {
            if (!string.IsNullOrEmpty(descriptorName))
            { return descriptorName + ":" + weaponModifiers.rating; }
            if (IniStats == null)
            { return ""; }

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


        internal bool IsValidModType(string modType)
        {
            if (modType == "helmet" || modType == "lowerBody" || modType == "upperBody" || modType == "tactical" || modType == "depot") { return true; }
            foreach (string supportedModType in supportedMods)
            {
                if (modType == supportedModType)
                {
                    return true;
                }
            }
            return false;
        }

        public void LoadImage()
        {
            bool male = false;
            if (!string.IsNullOrEmpty(icon))
            {
                foreach (FoxIcon foxicon in ImportSystem.Icons)
                {
                    if (foxicon.Name == icon)
                    {
                        wideImageMale = foxicon.GetWideImage();
                        largeSquareImageMale = foxicon.GetLargeSquareImage();
                        smallSquareImageMale = foxicon.GetSmallSquareImage();
                        male = true;
                    }
                    if (foxicon.Name == GetFemaleIconName())
                    {
                        wideImageFemale = foxicon.GetWideImage();
                        largeSquareImageFemale = foxicon.GetLargeSquareImage();
                        smallSquareImageFemale = foxicon.GetSmallSquareImage();
                    }
                }
            }
            if (!male)
            {
                wideImageMale = FoxIcon.CreateEmptyBitmap(256, 128);
                largeSquareImageMale = FoxIcon.CreateEmptyBitmap(128, 128);
                smallSquareImageMale = FoxIcon.CreateEmptyBitmap(64, 64);
            }
        }

        private string GetFemaleIconName()
        {
            string[] parts = icon.Split('_');
            string female = "";
            for (int i = 0; i < parts.Length; i++)
            {
                if (i == parts.Length - 1)
                {
                    female += "_Female";
                }
                if (i == 0)
                {
                    female += parts[i];
                }
                else
                {
                    female += "_" + parts[i];
                }
            }
            return female;
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

        private static string PrintIntArray(int[] ints)
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
            wideImageMale = wideImageMale?.Clone();
            wideImageFemale = wideImageFemale?.Clone();

            largeSquareImageMale = largeSquareImageMale?.Clone();
            largeSquareImageFemale = largeSquareImageFemale?.Clone();

            smallSquareImageMale = smallSquareImageMale?.Clone();
            smallSquareImageFemale = smallSquareImageFemale?.Clone();
        }
    }
}
