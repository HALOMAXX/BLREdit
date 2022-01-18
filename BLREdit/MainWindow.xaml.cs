using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BLREdit
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ImportItem FilterWeapon = null;
        private Loadout ActiveLoadout = null;

        public MainWindow()
        {
            profilechanging = true;
            textchnaging = true;


            ImportSystem.Initialize();
            InitializeComponent();
            ItemList.Items.Filter += new Predicate<object>(o => ((ImportItem)o).IsValidFor(FilterWeapon));


            PlayerNameTextBox.Text = ExportSystem.ActiveProfile.PlayerName;
            ProfileComboBox.ItemsSource = ExportSystem.Profiles;
            ProfileComboBox.SelectedIndex = 0;
            SetLoadout(ExportSystem.ActiveProfile.Loadout1);


            profilechanging = false;
            textchnaging = false;
        }

        private void UpdatePrimaryStats()
        {
            UpdateStats(
                PrimaryRecieverImage.DataContext as ImportItem,
                PrimaryBarrelImage.DataContext as ImportItem,
                PrimaryMagazineImage.DataContext as ImportItem,
                PrimaryMuzzleImage.DataContext as ImportItem,
                PrimaryScopeImage.DataContext as ImportItem,
                PrimaryStockImage.DataContext as ImportItem,
                PrimaryDamageLabel,
                PrimaryRateOfFireLabel,
                PrimaryAmmoLabel,
                PrimaryReloadLabel,
                PrimarySwapLabel,
                PrimaryAimLabel,
                PrimaryHipLabel,
                PrimaryMoveLabel,
                PrimaryRecoilLabel,
                PrimaryZoomLabel,
                PrimaryScopeInLabel,
                PrimaryRangeLabel,
                PrimaryRunLabel
                );
        }

        private void UpdateSecondaryStats()
        {
            UpdateStats(
                SecondaryRecieverImage.DataContext as ImportItem,
                SecondaryBarrelImage.DataContext as ImportItem,
                SecondaryMagazineImage.DataContext as ImportItem,
                SecondaryMuzzleImage.DataContext as ImportItem,
                SecondaryScopeImage.DataContext as ImportItem,
                SecondaryStockImage.DataContext as ImportItem,
                SecondaryDamageLabel,
                SecondaryRateOfFireLabel,
                SecondaryAmmoLabel,
                SecondaryReloadLabel,
                SecondarySwapLabel,
                SecondaryAimLabel,
                SecondaryHipLabel,
                SecondaryMoveLabel,
                SecondaryRecoilLabel,
                SecondaryZoomLabel,
                SecondaryScopeInLabel,
                SecondaryRangeLabel,
                SecondaryRunLabel
        );
        }

        private float Lerp(float start, float target, float time)
        {
            return start * (1 - time) + target * time;
        }

        private void UpdateStats(ImportItem Reciever, ImportItem Barrel, ImportItem Magazine, ImportItem Muzzle, ImportItem Scope, ImportItem Stock, Label DamageLabel, Label ROFLabel, Label AmmoLabel, Label ReloadLabel, Label SwapLabel, Label AimLabel, Label HipLabel, Label MoveLabel, Label RecoilLabel, Label ZoomLabel, Label ScopeInLabel, Label RangeLabel, Label RunLabel)
        {
            LoggingSystem.LogInfo("Updating Stats");

            float Damage = 0, ROF = 0, AmmoMag = 0, AmmoRes = 0, Reload = 0, Swap = 0, Aim = 0, Hip = 0, Move = 0, Recoil = 0, Zoom = 0, ScopeIn = 0, RangeClose = 0, RangeFar = 0, Run = 0;

            Damage += Reciever?.WikiStats.damage ?? 0;
            ROF += Reciever?.WikiStats.firerate ?? 0;
            AmmoMag += Reciever?.WikiStats.ammoMag ?? 0;
            AmmoRes += Reciever?.WikiStats.ammoReserve ?? 0;
            Reload += Reciever?.WikiStats.reload ?? 0;
            Swap += Reciever?.WikiStats.swaprate ?? 0;
            Aim += Reciever?.WikiStats.aimSpread ?? 0;
            Hip += Reciever?.WikiStats.hipSpread ?? 0;
            Move += Reciever?.WikiStats.moveSpread ?? 0;
            
            Zoom += Reciever?.WikiStats.zoom ?? 0;
            ScopeIn += Reciever?.WikiStats.scopeInTime ?? 0;
            RangeClose += Reciever?.WikiStats.rangeClose ?? 0;
            RangeFar += Reciever?.WikiStats.rangeFar ?? 0;
            Run += Reciever?.WikiStats.run ?? 0;

            Damage += Barrel?.WikiStats.damage ?? 0;
            ROF += Barrel?.WikiStats.firerate ?? 0;
            AmmoMag += Barrel?.WikiStats.ammoMag ?? 0;
            AmmoRes += Barrel?.WikiStats.ammoReserve ?? 0;
            Reload += Barrel?.WikiStats.reload ?? 0;
            Swap += Barrel?.WikiStats.swaprate ?? 0;
            Aim += Barrel?.WikiStats.aimSpread ?? 0;
            Hip += Barrel?.WikiStats.hipSpread ?? 0;
            Move += Barrel?.WikiStats.moveSpread ?? 0;
            
            Zoom += Barrel?.WikiStats.zoom ?? 0;
            ScopeIn += Barrel?.WikiStats.scopeInTime ?? 0;
            RangeClose += Barrel?.WikiStats.rangeClose ?? 0;
            RangeFar += Barrel?.WikiStats.rangeFar ?? 0;
            Run += Barrel?.WikiStats.run ?? 0;

            Damage += Muzzle?.WikiStats.damage ?? 0;
            ROF += Muzzle?.WikiStats.firerate ?? 0;
            AmmoMag += Muzzle?.WikiStats.ammoMag ?? 0;
            AmmoRes += Muzzle?.WikiStats.ammoReserve ?? 0;
            Reload += Muzzle?.WikiStats.reload ?? 0;
            Swap += Muzzle?.WikiStats.swaprate ?? 0;
            Aim += Muzzle?.WikiStats.aimSpread ?? 0;
            Hip += Muzzle?.WikiStats.hipSpread ?? 0;
            Move += Muzzle?.WikiStats.moveSpread ?? 0;
            
            Zoom += Muzzle?.WikiStats.zoom ?? 0;
            ScopeIn += Muzzle?.WikiStats.scopeInTime ?? 0;
            RangeClose += Muzzle?.WikiStats.rangeClose ?? 0;
            RangeFar += Muzzle?.WikiStats.rangeFar ?? 0;
            Run += Muzzle?.WikiStats.run ?? 0;

            Damage += Stock?.WikiStats.damage ?? 0;
            ROF += Stock?.WikiStats.firerate ?? 0;
            AmmoMag += Stock?.WikiStats.ammoMag ?? 0;
            AmmoRes += Stock?.WikiStats.ammoReserve ?? 0;
            Reload += Stock?.WikiStats.reload ?? 0;
            Swap += Stock?.WikiStats.swaprate ?? 0;
            Aim += Stock?.WikiStats.aimSpread ?? 0;
            Hip += Stock?.WikiStats.hipSpread ?? 0;
            Move += Stock?.WikiStats.moveSpread ?? 0;
            
            Zoom += Stock?.WikiStats.zoom ?? 0;
            ScopeIn += Stock?.WikiStats.scopeInTime ?? 0;
            RangeClose += Stock?.WikiStats.rangeClose ?? 0;
            RangeFar += Stock?.WikiStats.rangeFar ?? 0;
            Run += Stock?.WikiStats.run ?? 0;

            Damage += Scope?.WikiStats.damage ?? 0;
            ROF += Scope?.WikiStats.firerate ?? 0;
            AmmoMag += Scope?.WikiStats.ammoMag ?? 0;
            AmmoRes += Scope?.WikiStats.ammoReserve ?? 0;
            Reload += Scope?.WikiStats.reload ?? 0;
            Swap += Scope?.WikiStats.swaprate ?? 0;
            Aim += Scope?.WikiStats.aimSpread ?? 0;
            Hip += Scope?.WikiStats.hipSpread ?? 0;
            Move += Scope?.WikiStats.moveSpread ?? 0;
            
            Zoom += Scope?.WikiStats.zoom ?? 0;
            ScopeIn += Scope?.WikiStats.scopeInTime ?? 0;
            RangeClose += Scope?.WikiStats.rangeClose ?? 0;
            RangeFar += Scope?.WikiStats.rangeFar ?? 0;
            Run += Scope?.WikiStats.run ?? 0;

            Damage += Magazine?.WikiStats.damage ?? 0;
            ROF += Magazine?.WikiStats.firerate ?? 0;
            AmmoMag += Magazine?.WikiStats.ammoMag ?? 0;
            AmmoRes += Magazine?.WikiStats.ammoReserve ?? 0;
            Reload += Magazine?.WikiStats.reload ?? 0;
            Swap += Magazine?.WikiStats.swaprate ?? 0;
            Aim += Magazine?.WikiStats.aimSpread ?? 0;
            Hip += Magazine?.WikiStats.hipSpread ?? 0;
            Move += Magazine?.WikiStats.moveSpread ?? 0;
            
            Zoom += Magazine?.WikiStats.zoom ?? 0;
            ScopeIn += Magazine?.WikiStats.scopeInTime ?? 0;
            RangeClose += Magazine?.WikiStats.rangeClose ?? 0;
            RangeFar += Magazine?.WikiStats.rangeFar ?? 0;
            Run += Magazine?.WikiStats.run ?? 0;

            if (Reciever?.IniStats.RecoilAccumulation == 0)
            {
                Recoil += Reciever?.WikiStats.recoil ?? 0;
                Recoil += Barrel?.WikiStats.recoil ?? 0;
                Recoil += Muzzle?.WikiStats.recoil ?? 0;
                Recoil += Stock?.WikiStats.recoil ?? 0;
                Recoil += Scope?.WikiStats.recoil ?? 0;
                Recoil += Magazine?.WikiStats.recoil ?? 0;
            }
            else
            {
                if (Reciever != null && Reciever.WikiStats != null && Reciever.IniStats != null)
                {
                    float allRecoil = Barrel?.weaponModifiers?.recoil ?? 0;
                    allRecoil += Muzzle?.weaponModifiers?.recoil ?? 0;
                    allRecoil += Stock?.weaponModifiers?.recoil ?? 0;
                    allRecoil += Scope?.weaponModifiers?.recoil ?? 0;
                    allRecoil += Magazine?.weaponModifiers?.recoil ?? 0;
                    allRecoil /= 100.0f;
                    float recoilModifier = 1.0f;
                    if (allRecoil > 0)
                    {
                        recoilModifier = Lerp(Reciever?.IniStats?.ModificationRangeRecoil.Z ?? 0, Reciever?.IniStats?.ModificationRangeRecoil.Y ?? 0, allRecoil);
                    }
                    else
                    {
                        recoilModifier = Lerp(Reciever?.IniStats?.ModificationRangeRecoil.Z ?? 0, Reciever?.IniStats?.ModificationRangeRecoil.Y ?? 0, allRecoil);
                    }
                    //recoilModifier = 1.0f;
                    if (Reciever?.WikiStats.ammoMag > 0)
                    {
                        float averageShotCount = Math.Min(Reciever?.WikiStats.ammoMag ?? 0, 15.0f);
                        Vector3 averageRecoil = new Vector3(0, 0, 0);

                        for (int shot = 1; shot <= averageShotCount; shot++)
                        {
                            Vector3 newRecoil = new Vector3(0, 0, 0);
                            newRecoil.X = (Reciever.IniStats.RecoilVector.X * Reciever.IniStats.RecoilVectorMultiplier.X) / 8.0f;
                            newRecoil.Y = (Reciever.IniStats.RecoilVector.Y * Reciever.IniStats.RecoilVectorMultiplier.Y) / 2.0f;

                            float previousMultiplier = Reciever.IniStats.RecoilSize * (float)Math.Pow(shot/Reciever.IniStats.Burst, (Reciever.IniStats.RecoilAccumulation * Reciever.IniStats.RecoilAccumulationMultiplier));
                            float currentMultiplier = Reciever.IniStats.RecoilSize * (float)Math.Pow(shot / Reciever.IniStats.Burst+1.0f, (Reciever.IniStats.RecoilAccumulation * Reciever.IniStats.RecoilAccumulationMultiplier));
                            float multiplier = currentMultiplier - previousMultiplier;
                            newRecoil *= multiplier;
                            averageRecoil += newRecoil;
                        }

                        if (averageShotCount > 0)
                        { 
                            averageRecoil /= averageShotCount;
                        }
                        if (Reciever.IniStats.ROF > 0 && Reciever.IniStats.ApplyTime > 60 / Reciever.IniStats.ROF)
                        {
                            averageRecoil *= (60 / (Reciever.IniStats.ROF * Reciever.IniStats.ApplyTime));
                        }
                        Recoil = averageRecoil.Length() * recoilModifier;
                        Recoil = Recoil * (float)(180 / Math.PI); 
                    }
                    else
                    {
                        Recoil = 0;
                    }
                }
                else
                {
                    Recoil = 0;
                }
            }

            DamageLabel.Content = Damage.ToString("0");
            ROFLabel.Content = ROF.ToString("0");
            AmmoLabel.Content = AmmoMag.ToString("0") + "/" + AmmoRes.ToString("0");
            ReloadLabel.Content = Reload.ToString("0.00") + "s";
            SwapLabel.Content = Swap.ToString("0.00");
            AimLabel.Content = Aim.ToString("0.00") + "°";
            HipLabel.Content = Hip.ToString("0.00") + "°";
            MoveLabel.Content = Move.ToString("0.00") + "°";
            RecoilLabel.Content = Recoil.ToString("0.00") + "°";
            ZoomLabel.Content = Zoom.ToString("0.00");
            ScopeInLabel.Content = ScopeIn.ToString("0.00") + "s";
            RangeLabel.Content = RangeClose.ToString("0") + "/" + RangeFar.ToString("0");
            RunLabel.Content = Run.ToString("0.00");
            LoggingSystem.LogInfo("Finished Updating Stats");
        }

        private void CheckPrimaryModsForValidity(ImportItem primary)
        {
            CheckValidity(PrimaryBarrelImage, primary);

            CheckValidity(PrimaryMagazineImage, primary);

            CheckValidity(PrimaryMuzzleImage, primary);
            if (primary.name.Contains("Bow"))
            { PrimaryMuzzleImage.DataContext = null; }

            CheckValidity(PrimaryScopeImage, primary);

            CheckValidity(PrimaryStockImage, primary);
        }

        private void CheckSecondaryModsForValidity(ImportItem secondary)
        {
            CheckValidity(SecondaryBarrelImage, secondary);

            CheckValidity(SecondaryMagazineImage, secondary);

            CheckValidity(SecondaryMuzzleImage, secondary);

            CheckValidity(SecondaryScopeImage, secondary);

            CheckValidity(SecondaryStockImage, secondary);
        }

        private void CheckValidity(Image image, ImportItem item)
        {
            ImportItem importItem = image.DataContext as ImportItem;
            if (importItem != null && !importItem.IsValidFor(item))
            { LoggingSystem.LogInfo(importItem.name + " was not a Valid Mod for " + item.name); image.DataContext = null; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ItemList.ItemsSource = ImportSystem.Weapons.primary;
        }

        private void ItemList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pt = e.GetPosition(ItemList);
            var result = VisualTreeHelper.HitTest(ItemList, pt);
            if (result.VisualHit is Image image)
            {
                if (image.DataContext is ImportItem item)
                {
                    LoggingSystem.LogInfo("Sending:" + item.name);
                    DragDrop.DoDragDrop(image, item, DragDropEffects.Copy);
                }
            }
        }

        private void Image_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Serializable))
            { e.Effects = DragDropEffects.Copy; }
            else
            { e.Effects = DragDropEffects.None; }
        }

        private void Image_Drop(object sender, DragEventArgs e)
        {
            if (sender is Border border)
            {
                if (e.Data.GetData(typeof(ImportItem)) is ImportItem item)
                {
                    LoggingSystem.LogInfo("Recieving:" + item.name);
                    if (border.Child is Image image)
                    {

                        if (image.Name.Contains("Reciever"))
                        {
                            if (image.Name.Contains("Primary") && ImportSystem.Weapons.primary.Contains(item))
                            {
                                image.DataContext = item;
                                LoggingSystem.LogInfo("Primary Set!");
                                CheckPrimaryModsForValidity(item);
                                UpdatePrimaryStats();
                                return;
                            }
                            if (image.Name.Contains("Secondary") && ImportSystem.Weapons.secondary.Contains(item))
                            {
                                image.DataContext = item;
                                LoggingSystem.LogInfo("Secondary Set!");
                                CheckSecondaryModsForValidity(item);
                                UpdateSecondaryStats();
                                return;
                            }
                            LoggingSystem.LogInfo("Not a Valid Primary or Secondary!");
                        }
                        else
                        {
                            if (image.Name.Contains("Primary") && !item.IsValidFor(PrimaryRecieverImage.DataContext as ImportItem))
                            { LoggingSystem.LogInfo("Not a Valid Mod for " + (PrimaryRecieverImage.DataContext as ImportItem).name); return; }

                            if (image.Name.Contains("Secondary") && !item.IsValidFor(SecondaryRecieverImage.DataContext as ImportItem))
                            { LoggingSystem.LogInfo("Not a Valid Mod for " + (SecondaryRecieverImage.DataContext as ImportItem).name); return; }

                            if (image.Name.Contains("Muzzle") && ImportSystem.Mods.muzzles.Contains(item))
                            { image.DataContext = item; LoggingSystem.LogInfo("Muzzle with ID:" + ImportSystem.GetMuzzleID(item) + " Set!"); }
                            if (image.Name.Contains("Barrel") && ImportSystem.Mods.barrels.Contains(item))
                            { image.DataContext = item; LoggingSystem.LogInfo("Barrel Set!"); }
                            if (image.Name.Contains("Magazine") && ImportSystem.Mods.magazines.Contains(item))
                            { image.DataContext = item; LoggingSystem.LogInfo("Magazine with ID:" + ImportSystem.GetMagazineID(item) + " Set!"); }
                            if (image.Name.Contains("Scope") && ImportSystem.Mods.scopes.Contains(item))
                            { image.DataContext = item; LoggingSystem.LogInfo("Scope Set!"); }
                            if (image.Name.Contains("Stock") && ImportSystem.Mods.stocks.Contains(item))
                            { image.DataContext = item; LoggingSystem.LogInfo("Stock Set!"); }
                            if (image.Name.Contains("Gear") && ImportSystem.Gear.attachments.Contains(item))
                            { image.DataContext = item; LoggingSystem.LogInfo("Gear Set!"); }
                            if (image.Name.Contains("Tactical") && ImportSystem.Gear.tactical.Contains(item))
                            { image.DataContext = item; LoggingSystem.LogInfo("Tactical Set!"); }
                            UpdatePrimaryStats();
                            UpdateSecondaryStats();
                        }
                        UpdateActiveLoadout();
                    }
                }
            }
        }

        private void UpdateActiveLoadout()
        {
            UpdateLoadoutWeapon(ActiveLoadout.Primary, PrimaryRecieverImage.DataContext as ImportItem, PrimaryMuzzleImage.DataContext as ImportItem, PrimaryBarrelImage.DataContext as ImportItem, PrimaryMagazineImage.DataContext as ImportItem, PrimaryScopeImage.DataContext as ImportItem, PrimaryStockImage.DataContext as ImportItem);
            UpdateLoadoutWeapon(ActiveLoadout.Secondary, SecondaryRecieverImage.DataContext as ImportItem, SecondaryMuzzleImage.DataContext as ImportItem, SecondaryBarrelImage.DataContext as ImportItem, SecondaryMagazineImage.DataContext as ImportItem, SecondaryScopeImage.DataContext as ImportItem, SecondaryStockImage.DataContext as ImportItem);
            ActiveLoadout.Gear1 = ImportSystem.GetGearID(GearImage1.DataContext as ImportItem);
            ActiveLoadout.Gear2 = ImportSystem.GetGearID(GearImage2.DataContext as ImportItem);
            ActiveLoadout.Gear3 = ImportSystem.GetGearID(GearImage3.DataContext as ImportItem);
            ActiveLoadout.Gear4 = ImportSystem.GetGearID(GearImage4.DataContext as ImportItem);
            ActiveLoadout.Tactical = ImportSystem.GetTacticalID(TacticalImage.DataContext as ImportItem);
        }

        private void UpdateLoadoutWeapon(Weapon weapon, ImportItem reciever, ImportItem muzzle, ImportItem barrel, ImportItem magazine, ImportItem scope, ImportItem stock)
        {
            weapon.Receiver = reciever.name;
            weapon.Muzzle = ImportSystem.GetMuzzleID(muzzle);
            weapon.Barrel = barrel?.name ?? "No Barrel Mod";
            weapon.Magazine = ImportSystem.GetMagazineID(magazine);
            weapon.Scope = scope?.name ?? "No Optic Mod";
            weapon.Stock = stock?.name ?? "No Stock";
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                if (border.Child is Image image)
                {
                    ItemList.ItemsSource = null;
                    if (image.Name.Contains("Primary"))
                    {
                        FilterWeapon = PrimaryRecieverImage.DataContext as ImportItem;
                        LoggingSystem.LogInfo("ItemList Filter set Primary:" + (FilterWeapon?.name ?? "None"));
                        if (image.Name.Contains("Reciever"))
                        {
                            ItemList.ItemsSource = ImportSystem.Weapons.primary;
                            LoggingSystem.LogInfo("ItemList Set for Primary Reciever");
                            return;
                        }
                    }
                    if (image.Name.Contains("Secondary"))
                    {
                        FilterWeapon = SecondaryRecieverImage.DataContext as ImportItem;
                        LoggingSystem.LogInfo("ItemList Filter set Secondary:" + (FilterWeapon?.name ?? "None"));
                        if (image.Name.Contains("Reciever"))
                        {
                            ItemList.ItemsSource = ImportSystem.Weapons.secondary;
                            LoggingSystem.LogInfo("ItemList Set for Secondary Reciever");
                            return;
                        }
                    }
                    if (image.Name.Contains("Muzzle"))
                    {
                        if (!((PrimaryRecieverImage?.DataContext as ImportItem)?.name == "Compound Bow"))
                        {
                            ItemList.ItemsSource = ImportSystem.Mods.muzzles;
                            LoggingSystem.LogInfo("ItemList Set for Muzzles");
                        }
                        else
                        {
                            LoggingSystem.LogInfo("Muzzles are not Valid for " + (PrimaryRecieverImage.DataContext as ImportItem).name + "!");
                        }
                        return;
                    }
                    if (image.Name.Contains("Magazine"))
                    {
                        ItemList.ItemsSource = ImportSystem.Mods.magazines;
                        LoggingSystem.LogInfo("ItemList Set for Magazines");
                        return;
                    }
                    if (image.Name.Contains("Stock"))
                    {
                        ItemList.ItemsSource = ImportSystem.Mods.stocks;
                        LoggingSystem.LogInfo("ItemList Set for Stocks");
                        return;
                    }
                    if (image.Name.Contains("Scope"))
                    {
                        ItemList.ItemsSource = ImportSystem.Mods.scopes;
                        LoggingSystem.LogInfo("ItemList Set for Scopes");
                        return;
                    }
                    if (image.Name.Contains("Barrel"))
                    {
                        ItemList.ItemsSource = ImportSystem.Mods.barrels;
                        LoggingSystem.LogInfo("ItemList Set for Barrels");
                        return;
                    }
                    if (image.Name.Contains("Grip"))
                    {
                        ItemList.ItemsSource = ImportSystem.Mods.grips;
                        LoggingSystem.LogInfo("ItemList Set for Grips");
                        return;
                    }
                    if (image.Name.Contains("Gear"))
                    {
                        ItemList.ItemsSource = ImportSystem.Gear.attachments;
                        LoggingSystem.LogInfo("ItemList Set for Gear");
                        return;
                    }
                    if (image.Name.Contains("Tactical"))
                    {
                        ItemList.ItemsSource = ImportSystem.Gear.tactical;
                        LoggingSystem.LogInfo("ItemList Set for Tactical");
                        return;
                    }
                    LoggingSystem.LogInfo("ItemList Dind't get set");
                }
            }
        }

        public void SetLoadout(Loadout loadout)
        {
            ActiveLoadout = loadout;
            GearImage1.DataContext = loadout.GetGear(loadout.Gear1);
            GearImage2.DataContext = loadout.GetGear(loadout.Gear2);
            GearImage3.DataContext = loadout.GetGear(loadout.Gear3);
            GearImage4.DataContext = loadout.GetGear(loadout.Gear4);
            TacticalImage.DataContext = loadout.GetTactical();
            SetPrimary(loadout.Primary);
            SetSecondary(loadout.Secondary);

        }

        public void SetPrimary(Weapon primary)
        {
            PrimaryRecieverImage.DataContext = primary.GetReciever();
            PrimaryMuzzleImage.DataContext = primary.GetMuzzle();
            PrimaryStockImage.DataContext = primary.GetStock();
            PrimaryBarrelImage.DataContext = primary.GetBarrel();
            PrimaryMagazineImage.DataContext = primary.GetMagazine();
            PrimaryScopeImage.DataContext = primary.GetScope();
            UpdatePrimaryStats();
        }

        public void SetSecondary(Weapon secondary)
        {
            SecondaryRecieverImage.DataContext = secondary.GetReciever();
            SecondaryMuzzleImage.DataContext = secondary.GetMuzzle();
            SecondaryStockImage.DataContext = secondary.GetStock();
            SecondaryBarrelImage.DataContext = secondary.GetBarrel();
            SecondaryMagazineImage.DataContext = secondary.GetMagazine();
            SecondaryScopeImage.DataContext = secondary.GetScope();
            UpdateSecondaryStats();
        }
        bool textchnaging = false;
        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!textchnaging)
            {
                profilechanging = true;
                ExportSystem.ActiveProfile = ProfileComboBox.SelectedValue as Profile;
                PlayerNameTextBox.Text = ExportSystem.ActiveProfile.PlayerName;
                SetLoadout(ExportSystem.ActiveProfile.Loadout1);
                profilechanging = false;
            }
        }

        private void Loadout1Button_Click(object sender, RoutedEventArgs e)
        {
            SetLoadout(ExportSystem.ActiveProfile.Loadout1);
        }

        private void Loadout2Button_Click(object sender, RoutedEventArgs e)
        {
            SetLoadout(ExportSystem.ActiveProfile.Loadout2);
        }

        private void Loadout3Button_Click(object sender, RoutedEventArgs e)
        {
            SetLoadout(ExportSystem.ActiveProfile.Loadout3);
        }

        private void CopyToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            ExportSystem.CopyToClipBoard(ExportSystem.ActiveProfile);
        }

        private void AddProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ExportSystem.AddProfile();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ExportSystem.SaveProfiles();
        }
        bool profilechanging = false;
        private void PlayerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!profilechanging)
            {
                textchnaging = true;
                int index = ProfileComboBox.SelectedIndex;
                ProfileComboBox.ItemsSource = null;
                ProfileComboBox.Items.Clear();
                ExportSystem.RemoveActiveProfileFromDisk();
                ExportSystem.ActiveProfile.PlayerName = PlayerNameTextBox.Text;
                ProfileComboBox.ItemsSource = ExportSystem.Profiles;
                ProfileComboBox.SelectedIndex = index;
                textchnaging = false;
            }
        }
    }
}