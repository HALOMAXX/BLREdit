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

        public string DisplayStatDesc1
        {
            get 
            {
                if (Category == "primary" || Category == "secondary")
                {
                    return "Damage:";
                }
                else if(Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    return "Damage:";
                }
                else if (Category == "scope")
                {
                    return "Zoom:";
                }
                else if (Category == "magazine")
                {
                    return "Ammo:";
                }
                else if (Category == "helmet" || Category == "upperBody" || Category == "lowerBody")
                {
                    return "Health:";
                }
                return "";
            }
        }
        public string DisplayStat1
        {
            get
            {
                if (Category == "primary" || Category == "secondary")
                {
                    double[] damage = UI.MainWindow.CalculateDamage(this, 0);
                    return damage[0].ToString("0") + "/" + damage[1].ToString("0");
                }
                else if (Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    return weaponModifiers.damage + "%";
                }
                else if (Category == "scope")
                {
                    return (1.3 + (WikiStats?.zoom ?? 0)).ToString("0.00") + "x";
                }
                else if (Category == "magazine")
                {
                    return weaponModifiers.ammo.ToString("0");
                }
                else if (Category == "helmet" || Category == "upperBody" || Category == "lowerBody")
                {
                    return pawnModifiers.Health.ToString("0");
                }
                return "";
            }
        }
        public bool DisplayStat1Gray
        {
            get
            {
                if (Category == "primary" || Category == "secondary")
                {
                    double[] damage = UI.MainWindow.CalculateDamage(this, 0);
                    if (damage[0] == 0)
                    { return true; }
                }
                else if (Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    if (weaponModifiers.damage == 0)
                    { return true; }
                }
                else if (Category == "magazine")
                {
                    if (weaponModifiers.ammo == 0)
                    { return true; }
                }
                else if (Category == "helmet" || Category == "upperBody" || Category == "lowerBody")
                {
                    if (pawnModifiers.Health == 0)
                    { return true; }
                }
                return false;
            }
        }

        public string DisplayStatDesc2
        {
            get
            {
                if (Category == "primary" || Category == "secondary")
                {
                    return "Aim:";
                }
                else if (Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    return "Accuracy:";
                }
                else if (Category == "scope")
                {
                    return "Scope In:";
                }
                else if (Category == "magazine")
                {
                    return "Reload:";
                }
                else if (Category == "helmet")
                {
                    return "Head Armor:";
                }
                else if (Category == "upperBody" || Category == "lowerBody")
                {
                    return "Run:";
                }
                return "";
            }
        }
        public string DisplayStat2
        {
            get
            {
                if (Category == "primary" || Category == "secondary")
                {
                    double[] spread = UI.MainWindow.CalculateSpread(this, 0, 0);
                    return spread[0].ToString("0.00") + '°';
                }
                else if (Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    return weaponModifiers.accuracy + "%";
                }
                else if (Category == "scope")
                {
                    return (0.240 + (WikiStats?.scopeInTime ?? 0)).ToString("0.000") + "s";
                }
                else if (Category == "magazine")
                {
                    if (Contains(40021, 40002))
                    { 
                        return weaponModifiers.reloadSpeed.ToString("0") + "%";
                    }
                    else
                    {
                        return WikiStats.reload.ToString("0.00") + 's';
                    }
                }
                else if (Category == "helmet")
                {
                    return pawnModifiers.HelmetDamageReduction.ToString("0") + '%';
                }
                else if (Category == "upperBody" || Category == "lowerBody")
                {
                    return pawnModifiers.MovementSpeed.ToString("0.00");
                }
                return "";
            }
        }
        public bool DisplayStat2Gray
        {
            get 
            {
                if (Category == "primary" || Category == "secondary")
                {
                    double[] spread = UI.MainWindow.CalculateSpread(this, 0, 0);
                    if (spread[0] == 0)
                    { return true; }
                }
                else if (Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    if (weaponModifiers.accuracy == 0)
                    { return true; }
                }
                else if (Category == "magazine")
                {
                    if (Contains(40021, 40002))
                    {
                        if (weaponModifiers.reloadSpeed == 0)
                        { return true; }
                    }
                    else
                    {
                        if (WikiStats.reload == 0)
                        { return true; }
                    }
                }
                else if (Category == "helmet")
                {
                    if (pawnModifiers.HelmetDamageReduction == 0)
                    { return true; }
                }
                else if (Category == "upperBody" || Category == "lowerBody")
                {
                    if (pawnModifiers.MovementSpeed == 0)
                    { return true; }
                }
                return false;
            }
        }

        public string DisplayStatDesc3
        {
            get
            {
                if (Category == "primary" || Category == "secondary")
                {
                    return "Hip:";
                }
                else if (Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    return "Recoil:";
                }
                else if (Category == "helmet")
                {
                    return "Run:";
                }
                else if (Category == "magazine")
                {
                    return "Run:";
                }
                else if (Category == "upperBody" || Category == "lowerBody")
                {
                    return "Gear:";
                }
                return "";
            }
        }
        public string DisplayStat3
        {
            get
            {
                if (Category == "primary" || Category == "secondary")
                {
                    double[] spread = UI.MainWindow.CalculateSpread(this, 0, 0);
                    return spread[1].ToString("0.00") + '°';
                }
                else if (Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    return weaponModifiers.recoil + "%";
                }
                else if (Category == "helmet")
                { 
                    return pawnModifiers.MovementSpeed.ToString("0.00");
                }
                else if (Category == "magazine")
                {
                    return weaponModifiers.movementSpeed.ToString("0") + "%";
                }
                else if (Category == "upperBody" || Category == "lowerBody")
                {
                    return pawnModifiers.GearSlots.ToString("0");
                }
                return "";
            }
        }
        public bool DisplayStat3Gray
        {
            get 
            {
                if (Category == "primary" || Category == "secondary")
                {
                    double[] spread = UI.MainWindow.CalculateSpread(this, 0, 0);
                    if (spread[1] == 0)
                    { return true; }
                }
                else if (Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    if (weaponModifiers.recoil == 0)
                    { return true; }
                }
                else if (Category == "helmet")
                {
                    if (pawnModifiers.MovementSpeed == 0)
                    { return true; }
                }
                else if (Category == "magazine")
                {
                    if (weaponModifiers.movementSpeed == 0)
                    { return true; }
                }
                else if (Category == "upperBody" || Category == "lowerBody")
                {
                    if (pawnModifiers.GearSlots == 0)
                    { return true; }
                }
                return false;
            }
        }

        public string DisplayStatDesc4
        {
            get
            {
                if (Category == "primary" || Category == "secondary")
                {
                    return "Move:";
                }
                else if (Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    return "Range:";
                }
                else if (Category == "magazine")
                {
                    return "Damage:";
                }
                else if (Category == "helmet")
                {
                    return "HRV:";
                }
                return "";
            }
        }
        public string DisplayStat4
        {
            get
            {
                if (Category == "primary" || Category == "secondary")
                {
                    double[] spread = UI.MainWindow.CalculateSpread(this, 0, 0);
                    return spread[2].ToString("0.00") + '°';
                }
                else if (Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    return weaponModifiers.range + "%";
                }
                else if (Category == "magazine")
                {
                    return weaponModifiers.damage.ToString("0") + '%';
                }
                else if (Category == "helmet")
                {
                    return pawnModifiers.HRVDuration.ToString("0.0");
                }
                return "";
            }
        }
        public bool DisplayStat4Gray { 
            get 
            {
                if (Category == "primary" || Category == "secondary")
                {
                    double[] spread = UI.MainWindow.CalculateSpread(this, 0, 0);
                    if (spread[2] == 0)
                    { return true; }
                }
                else if (Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    if (weaponModifiers.range == 0)
                    { return true; }
                }
                else if (Category == "magazine")
                {
                    if (weaponModifiers.damage == 0)
                    { return true; }
                }
                else if (Category == "helmet")
                {
                    if (pawnModifiers.HRVDuration == 0)
                    { return true; }
                }
                return false;
            }
        }

        public string DisplayStatDesc5
        {
            get
            {
                if (Category == "primary" || Category == "secondary")
                {
                    return "Recoil:";
                }
                else if (Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    return "Run:";
                }
                else if (Category == "magazine")
                {
                    return "Recoil:";
                }
                else if (Category == "helmet")
                {
                    return "Recharge:";
                }
                return "";
            }
        }
        public string DisplayStat5
        {
            get
            {
                if (Category == "primary" || Category == "secondary")
                {
                    double recoil = UI.MainWindow.CalculateRecoil(this, 0);
                    return recoil.ToString("0.00") + '°';
                }
                else if (Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    return weaponModifiers.movementSpeed + "%";
                }
                else if (Category == "magazine")
                {
                    return weaponModifiers.recoil + "%"; ;
                }
                else if (Category == "helmet")
                {
                    return pawnModifiers.HRVDuration.ToString("0.0") + "u/s";
                }
                return "";
            }
        }
        public bool DisplayStat5Gray
        {
            get 
            {
                if (Category == "primary" || Category == "secondary")
                {
                    double recoil = UI.MainWindow.CalculateRecoil(this, 0);
                    if (recoil == 0)
                    { return true; }
                }
                else if (Category == "barrel" || Category == "muzzle" || Category == "stock")
                {
                    if (weaponModifiers.movementSpeed == 0)
                    { return true; }
                }
                else if (Category == "magazine")
                {
                    if (weaponModifiers.recoil == 0)
                    { return true; }
                }
                else if (Category == "helmet")
                {
                    if (pawnModifiers.HRVDuration == 0)
                    { return true; }
                }
                return false;
            }
        }

        public string DisplayStatDesc6
        {
            get
            {
                if (Category == "primary" || Category == "secondary")
                {
                    return "Range:";
                }
                else if (name == "Prex Chem/Hazmat Respirator-TOX")
                {
                    return "Toxic:";
                }
                else if (name == "Prex Chem/Hazmat Respirator-INC")
                {
                    return "Fire:";
                }
                else if (name == "Prex Chem/Hazmat Respirator-XPL")
                {
                    return "Explo:";
                }
                else if (Category == "magazine")
                {
                    if (Contains(40021, 40002))
                    {
                        return "Accuracy:";
                    }
                }
                return "";
            }
        }
        public string DisplayStat6
        {
            get
            {
                if (Category == "primary" || Category == "secondary")
                {
                    double[] range = UI.MainWindow.CalculateRange(this, 0);
                    return range[0].ToString("0") + '/' + range[1].ToString("0");
                }
                else if (name == "Prex Chem/Hazmat Respirator-TOX")
                {
                    return pawnModifiers.ToxicProtection.ToString("0") + '%';
                }
                else if (name == "Prex Chem/Hazmat Respirator-INC")
                {
                    return pawnModifiers.IncendiaryProtection.ToString("0") + '%';
                }
                else if (name == "Prex Chem/Hazmat Respirator-XPL")
                {
                    return pawnModifiers.ExplosiveProtection.ToString("0") + '%';
                }
                else if (Category == "magazine")
                {
                    if (Contains(40021, 40002))
                    {
                        return weaponModifiers.accuracy.ToString("0") + '%'; ;
                    }
                }
                return "";
            }
        }
        public bool DisplayStat6Gray
        {
            get 
            {
                if (Category == "primary" || Category == "secondary")
                {
                    double[] range = UI.MainWindow.CalculateRange(this, 0);
                    if (range[0] == 0)
                    { return true; }
                }
                else if (Category == "magazine")
                {
                    if (Contains(40021, 40002))
                    {
                        if (weaponModifiers.accuracy == 0)
                        { return true; }
                    }
                }
                return false;
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


        public bool Contains(params int[] args)
        {
            foreach (int valid in validFor)
            {
                foreach (int uid in args)
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
            if (item != null && item.tooltip == "Depot Item!" && (Category != "secondary" && Category != "tactical" && Category != "attachments" && Category != "helmet" && Category != "lowerBody" && Category != "upperBody")) { return false; }
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
