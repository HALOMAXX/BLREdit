using System;
using System.Collections.Generic;
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
        public List<int> validFor { get; set; }
        public WeaponModifiers weaponModifiers { get; set; }
        public List<string> supportedMods { get; set; }
        public ImportStats stats { get; set; }
        public WikiStats WikiStats { get; set; }
        public IniStats IniStats { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new();
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
        public string GetDescriptorName(double points)
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


        public bool IsValidForItemIDS(params int[] uids)
        {
            foreach (int valid in validFor)
            {
                foreach (int uid in uids)
                {
                    if (valid == uid)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsValidFor(ImportItem item)
        {
            if (item.tooltip == "Depot Item!" && (Category != "secondary" && Category != "tactical" && Category != "attachments" && Category != "helmets" && Category != "lowerBodies" && Category != "upperBodies")) { return false; }
            if (validFor == null) { return true; }

            foreach (int id in validFor)
            {
                if (id == item.uid)
                { return true; }
            }
            return false;
        }


        internal bool IsValidModType(string modType)
        {
            if (modType == "helmets" || modType == "lowerBodies" || modType == "upperBodies" || modType == "tactical" || modType == "depot") { return true; }
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

        private static string PrintIntArray(List<int> ints)
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

        private static string PrintStringArray(List<string> strings)
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
