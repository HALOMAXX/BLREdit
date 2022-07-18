using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;

namespace BLREdit.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static readonly Random rng = new();

    /// <summary>
    /// Contains the last selected Image for setting the ItemList
    /// </summary>
    public static Image LastSelectedImage { get; private set; } = null;

    /// <summary>
    /// Contains the weapon to filter out Items From the ItemList
    /// </summary>
    private BLRItem FilterWeapon = null;

    /// <summary>
    /// Contains the current active loadout
    /// </summary>
    public static MagiCowsLoadout ActiveLoadout { get; set; } = null;
    public static BLRLoadoutSetup Loadout { get; } = new();
    /// <summary>
    /// Contains the Sorting Direction for the ItemList
    /// </summary>
    public ListSortDirection SortDirection { get; set; } = ListSortDirection.Descending;

    public Type CurrentSortingEnumType { get; private set; }
    public string CurrentSortingPropertyName { get; private set; } = "None";

    /// <summary>
    /// Prevents Profile Changes
    /// </summary>
    public bool IsPlayerNameChanging { get; private set; } = false;
    /// <summary>
    /// Prevents Profile Name Changes
    /// </summary>
    public bool IsPlayerProfileChanging { get; private set; } = false;

    public static MainWindow Self { get; private set; } = null;

    public MainWindow()
    {
        Self = this;
        IsPlayerProfileChanging = true;
        IsPlayerNameChanging = true;

        InitializeComponent();
        //CreateTags();

        ItemListButton_Click(ItemListButton, new RoutedEventArgs());

        ItemList.Items.Filter += new Predicate<object>(o =>
        {
            if (o != null && FilterWeapon != null)
            {
                return ((BLRItem)o).IsValidFor(FilterWeapon);
            }
            else
            {
                return false;
            }
        });

        PlayerNameTextBox.Text = ExportSystem.ActiveProfile.PlayerName;
        ProfileComboBox.ItemsSource = ExportSystem.Profiles;
        ProfileComboBox.SelectedIndex = 0;

        SetLoadout(ExportSystem.ActiveProfile.Loadout1);

        Loadout1Button.IsEnabled = false;

        IsPlayerProfileChanging = false;
        IsPlayerNameChanging = false;

        LastSelectedImage = PrimaryRecieverImage;
        FilterWeapon = Loadout.Primary.Reciever;
        SetItemList(ImportSystem.PRIMARY_CATEGORY);

        LoadoutGrid.DataContext = Loadout;
    }

    private void UpdateStats2()
    {
        #region PrimaryStats
        PrimaryDamageLabel.Content = Loadout.Primary.DamageClose.ToString("0.0") + " / " + Loadout.Primary.DamageFar.ToString("0.0");
        PrimaryRateOfFireLabel.Content = Loadout.Primary.ModifiedRateOfFire.ToString("0");
        PrimaryAmmoLabel.Content = Loadout.Primary.ModifiedAmmoMagazine.ToString("0") + " / " + Loadout.Primary.ModifiedAmmoReserve.ToString("0");
        PrimaryReloadLabel.Content = Loadout.Primary.ModifiedReloadSpeed.ToString("0.00") + 's';
        PrimarySwapLabel.Content = Loadout.Primary.RawSwapRate.ToString("0");
        PrimaryAimLabel.Content = Loadout.Primary.SpreadWhileADS.ToString("0.00") + '°';
        PrimaryHipLabel.Content = Loadout.Primary.SpreadWhileStanding.ToString("0.00") + '°';
        PrimaryMoveLabel.Content = Loadout.Primary.SpreadWhileMoving.ToString("0.00") + '°';
        PrimaryRecoilLabel.Content = Loadout.Primary.RecoilHip.ToString("0.00") + '°';
        PrimaryZoomRecoilLabel.Content = Loadout.Primary.RecoilZoom.ToString("0.00") + '°';
        PrimaryZoomLabel.Content = Loadout.Primary.ZoomMagnification.ToString("0.00");
        PrimaryScopeInLabel.Content = Loadout.Primary.ModifiedScopeInTime.ToString("0.000") + 's';
        PrimaryRangeLabel.Content = Loadout.Primary.RangeClose.ToString("0.0") + " / " + Loadout.Primary.RangeFar.ToString("0.0") + " / " + Loadout.Primary.RangeTracer.ToString("0");
        PrimaryRunLabel.Content = Loadout.Primary.ModifiedRunSpeed.ToString("0.00");
        PrimaryDescriptorLabel.Content = Loadout.Primary.WeaponDescriptor;

        PrimaryRecoilVerticalRatioLabel.Content = Loadout.Primary.VerticalRecoilRatio.ToString("0.00");
        PrimarySpreadCenterWeightLabel.Content = Loadout.Primary.SpreadCenterWeight.ToString("0.00");
        PrimarySpreadCenterLabel.Content = Loadout.Primary.SpreadCenter.ToString("0.00");
        PrimaryFragmentsPerShellLabel.Content = Loadout.Primary.FragmentsPerShell.ToString("0");
        PrimaryZoomFirerateLabel.Content = Loadout.Primary.ZoomRateOfFire.ToString("0");
        PrimarySpreadCrouchMultiplierLabel.Content = Loadout.Primary.SpreadCrouchMultiplier.ToString("0.00");
        PrimarySpreadJumpMultiplierLabel.Content = Loadout.Primary.SpreadJumpMultiplier.ToString("0.00");
        PrimaryRecoilRecoveryTimeLabel.Content = Loadout.Primary.RecoilRecoveryTime.ToString("0.00");
        PrimaryModReloadLabel.Content = Loadout.Primary.ModifiedReloadSpeed.ToString("0.00") + 's';

        PrimaryModAccuracyLabel.Content = Loadout.Primary.AccuracyPercentage.ToString("0") + '%';
        PrimaryModDamageLabel.Content = Loadout.Primary.DamagePercentage.ToString("0") + '%';
        PrimaryModRangeLabel.Content = Loadout.Primary.RangePercentage.ToString("0") + '%';
        PrimaryModRecoilLabel.Content = Loadout.Primary.RecoilPercentage.ToString("0") + '%';
        PrimaryModReloadLabel.Content = Loadout.Primary.ReloadSpeedPercentage.ToString("0") + '%';
        PrimaryModRunLabel.Content = Loadout.Primary.MovementSpeedPercentage.ToString("0") + '%';
        #endregion PrimaryStats

        #region SecondaryStats
        SecondaryDamageLabel.Content = Loadout.Secondary.DamageClose.ToString("0.0") + " / " + Loadout.Secondary.DamageFar.ToString("0.0");
        SecondaryRateOfFireLabel.Content = Loadout.Secondary.ModifiedRateOfFire.ToString("0");
        SecondaryAmmoLabel.Content = Loadout.Secondary.ModifiedAmmoMagazine.ToString("0") + " / " + Loadout.Secondary.ModifiedAmmoReserve.ToString("0");
        SecondaryReloadLabel.Content = Loadout.Secondary.ModifiedReloadSpeed.ToString("0.00") + 's';
        SecondarySwapLabel.Content = Loadout.Secondary.RawSwapRate.ToString("0");
        SecondaryAimLabel.Content = Loadout.Secondary.SpreadWhileADS.ToString("0.00") + '°';
        SecondaryHipLabel.Content = Loadout.Secondary.SpreadWhileStanding.ToString("0.00") + '°';
        SecondaryMoveLabel.Content = Loadout.Secondary.SpreadWhileMoving.ToString("0.00") + '°';
        SecondaryRecoilLabel.Content = Loadout.Secondary.RecoilHip.ToString("0.00") + '°';
        SecondaryZoomRecoilLabel.Content = Loadout.Secondary.RecoilZoom.ToString("0.00") + '°';
        SecondaryZoomLabel.Content = Loadout.Secondary.ZoomMagnification.ToString("0.00");
        SecondaryScopeInLabel.Content = Loadout.Secondary.ModifiedScopeInTime.ToString("0.000") + 's';
        SecondaryRangeLabel.Content = Loadout.Secondary.RangeClose.ToString("0.0") + " / " + Loadout.Secondary.RangeFar.ToString("0.0") + " / " + Loadout.Secondary.RangeTracer.ToString("0");
        SecondaryRunLabel.Content = Loadout.Secondary.ModifiedRunSpeed.ToString("0.00");
        SecondaryDescriptorLabel.Content = Loadout.Secondary.WeaponDescriptor;

        SecondaryRecoilVerticalRatioLabel.Content = Loadout.Secondary.VerticalRecoilRatio.ToString("0.00");
        SecondarySpreadCenterWeightLabel.Content = Loadout.Secondary.SpreadCenterWeight.ToString("0.00");
        SecondarySpreadCenterLabel.Content = Loadout.Secondary.SpreadCenter.ToString("0.00");
        SecondaryFragmentsPerShellLabel.Content = Loadout.Secondary.FragmentsPerShell.ToString("0");
        SecondaryZoomFirerateLabel.Content = Loadout.Secondary.ZoomRateOfFire.ToString("0");
        SecondarySpreadCrouchMultiplierLabel.Content = Loadout.Secondary.SpreadCrouchMultiplier.ToString("0.00");
        SecondarySpreadJumpMultiplierLabel.Content = Loadout.Secondary.SpreadJumpMultiplier.ToString("0.00");
        SecondaryRecoilRecoveryTimeLabel.Content = Loadout.Secondary.RecoilRecoveryTime.ToString("0.00");
        SecondaryModReloadLabel.Content = Loadout.Secondary.ModifiedReloadSpeed.ToString("0.00") + 's';

        SecondaryModAccuracyLabel.Content = Loadout.Secondary.AccuracyPercentage.ToString("0") + '%';
        SecondaryModDamageLabel.Content = Loadout.Secondary.DamagePercentage.ToString("0") + '%';
        SecondaryModRangeLabel.Content = Loadout.Secondary.RangePercentage.ToString("0") + '%';
        SecondaryModRecoilLabel.Content = Loadout.Secondary.RecoilPercentage.ToString("0") + '%';
        SecondaryModReloadLabel.Content = Loadout.Secondary.ReloadSpeedPercentage.ToString("0") + '%';
        SecondaryModRunLabel.Content = Loadout.Secondary.MovementSpeedPercentage.ToString("0") + '%';
        #endregion SecondaryStats



        GearImage1.IsEnabled = false;
        GearImage2.IsEnabled = false;
        GearImage3.IsEnabled = false;
        GearImage4.IsEnabled = false;

        Gear1Rect.Visibility = Visibility.Visible;
        Gear2Rect.Visibility = Visibility.Visible;
        Gear3Rect.Visibility = Visibility.Visible;
        Gear4Rect.Visibility = Visibility.Visible;

        if (Loadout.GearSlots > 0)
        {
            GearImage1.IsEnabled = true;
            Gear1Rect.Visibility = Visibility.Hidden;
        }
        if (Loadout.GearSlots > 1)
        {
            GearImage2.IsEnabled = true;
            Gear2Rect.Visibility = Visibility.Hidden;
        }
        if (Loadout.GearSlots > 2)
        {
            GearImage3.IsEnabled = true;
            Gear3Rect.Visibility = Visibility.Hidden;
        }
        if (Loadout.GearSlots > 3)
        {
            GearImage4.IsEnabled = true;
            Gear4Rect.Visibility = Visibility.Hidden;
        }
        ArmorGearLabel.Content = Loadout.GearSlots.ToString("0");
        GearSlotsGearModLabel.Content = (Loadout.GearSlots - 2).ToString("0");

        ArmorHeadProtectionLabel.Content = Loadout.HeadProtection.ToString("0.0") + '%';
        HeadArmorGearModLabel.Content = Loadout.HeadProtection.ToString("0.0") + '%';

        ArmorHealthLabel.Content = Loadout.Health;
        HealthGearModLabel.Content = Loadout.RawHealth.ToString("0") + '%';

        ArmorRunLabel.Content = (Loadout.Run / 100.0D).ToString("0.00");
        RunGearModLabel.Content = Loadout.RawMoveSpeed.ToString("0") + '%';

        ArmorHRVLabel.Content = Loadout.HRVDuration.ToString("0.0") + "u";
        HRVDurationGearModLabel.Content = (Loadout.HRVDuration - 70).ToString("0.0") + "u";

        ArmorHRVRechargeLabel.Content = Loadout.HRVRechargeRate.ToString("0.0") + "u/s";
        HRVRechargeGearModLabel.Content = (Loadout.HRVRechargeRate - 6.6).ToString("0.0") + "u/s";

        ArmorGearElectroProtectionLabel.Content = Loadout.ElectroProtection.ToString("0") + '%';
        ArmorGearExplosiveProtectionLabel.Content = Loadout.ExplosiveProtection.ToString("0") + '%';
        ArmorGearIncendiaryLabel.Content = Loadout.IncendiaryProtection.ToString("0") + '%';
        ArmorGearInfraredProtectionLabel.Content = Loadout.InfraredProtection.ToString("0") + '%';
        ArmorGearMeleeProtectionLabel.Content = Loadout.MeleeProtection.ToString("0") + '%';
        ArmorGearToxicProtectionLabel.Content = Loadout.ToxicProtection.ToString("0") + '%';
    }

    #region OldCalcs
    private static double Lerp(double start, double target, double time)
    {
        return start * (1.0d - time) + target * time;
    }

    private static bool CheckCalculationReady(BLRItem item)
    {
        return item != null && item.WeaponStats != null && item.WeaponStats != null;
    }

    public static void AccumulateStatsOfWeaponParts(BLRItem[] items, ref double ROF, ref double Reload, ref double Swap, ref double Zoom, ref double ScopeIn, ref double Run)
    {
        double DamagePercent = 0;
        double AccuracyPercent = 0;
        double RangePercent = 0;
        double ReloadPercent = 0;
        double RecoilPercent = 0;
        double RunPercent = 0;

        foreach (BLRItem item in items)
        {
            ROF += item?.WeaponStats?.ROF ?? 0;
            Reload += item?.WikiStats?.reload ?? 0;
            Swap += item?.WikiStats?.swaprate ?? 0;
            Zoom += item?.WikiStats?.zoom ?? 0;
            ScopeIn += item?.WikiStats?.scopeInTime ?? 0;
            Run += item?.WikiStats?.run ?? 0;

            DamagePercent += item?.WeaponModifiers?.damage ?? 0;
            AccuracyPercent += item?.WeaponModifiers?.accuracy ?? 0;
            RangePercent += item?.WeaponModifiers?.range ?? 0;
            ReloadPercent += item?.WeaponModifiers?.reloadSpeed ?? 0;
            RecoilPercent += item?.WeaponModifiers?.recoil ?? 0;
            RunPercent += item?.WeaponModifiers?.movementSpeed ?? 0;
        }
        DamagePercent = Math.Min(Math.Max(DamagePercent, -100), 100);
        AccuracyPercent = Math.Min(Math.Max(AccuracyPercent, -100), 100);
        RangePercent = Math.Min(Math.Max(RangePercent, -100), 100);
        RecoilPercent = Math.Min(Math.Max(RecoilPercent, -100), 100);
        RunPercent = Math.Min(Math.Max(RunPercent, -100), 100);

        if (items[0].Category == ImportSystem.PRIMARY_CATEGORY)
        {
            Self.PrimaryModDamageLabel.Content = DamagePercent.ToString("0") + '%';
            Self.PrimaryModAccuracyLabel.Content = AccuracyPercent.ToString("0") + '%';
            Self.PrimaryModRangeLabel.Content = RangePercent.ToString("0") + '%';
            Self.PrimaryModRecoilLabel.Content = RecoilPercent.ToString("0") + '%';
            Self.PrimaryModRunLabel.Content = RunPercent.ToString("0") + '%';
        }

        if (items[0].Category == ImportSystem.SECONDARY_CATEGORY)
        {
            Self.SecondaryModDamageLabel.Content = DamagePercent.ToString("0") + '%';
            Self.SecondaryModAccuracyLabel.Content = AccuracyPercent.ToString("0") + '%';
            Self.SecondaryModRangeLabel.Content = RangePercent.ToString("0") + '%';
            Self.SecondaryModRecoilLabel.Content = RecoilPercent.ToString("0") + '%';
            Self.SecondaryModRunLabel.Content = RunPercent.ToString("0") + '%';
        }
    }

    private static void UpdateStats(BLRItem Reciever, BLRItem Barrel, BLRItem Magazine, BLRItem Muzzle, BLRItem Scope, BLRItem Stock, Label DamageLabel, Label ROFLabel, Label AmmoLabel, Label ReloadLabel, Label SwapLabel, Label AimLabel, Label HipLabel, Label MoveLabel, Label RecoilLabel, Label ZoomRecoilLabel, Label ZoomLabel, Label ScopeInLabel, Label RangeLabel, Label RunLabel, Label Descriptor)
    {
        System.Diagnostics.Stopwatch watch = null;
        if (LoggingSystem.IsDebuggingEnabled) watch = LoggingSystem.LogInfo("Updating Stats", "");

        double Damage = 0, DamageFar = 0, ROF = 0, AmmoMag = 0, AmmoRes = 0, Reload = 0, Swap = 0, Aim = 0, Hip = 0, Move = 0, Recoil = 0, RecoilZoom = 0, Zoom = 0, ScopeIn = 0, RangeClose = 0, RangeFar = 0, RangeMax = 0, Run = 0, MoveSpeed = 0, CockRateMultiplier = 0, ReloadRateMultiplier = 0;
        double BaseScopeIn = 0;

        string barrelVSmag = "", stockVSmuzzle = "", weaponDescriptor = "";

        if (CheckCalculationReady(Reciever))
        {
            List<BLRItem> items = new();
            if (Reciever != null)
                items.Add(Reciever);
            if (Barrel != null)
                items.Add(Barrel);
            if (Magazine != null)
                items.Add(Magazine);
            if (Muzzle != null)
                items.Add(Muzzle);
            if (Scope != null)
                items.Add(Scope);
            if (Stock != null)
                items.Add(Stock);

            AccumulateStatsOfWeaponParts(items.ToArray(), ref ROF, ref Reload, ref Swap, ref Zoom, ref ScopeIn, ref Run);

            if (Reciever.UID == 40019)
            {
                AmmoMag = 1; // cheat because for some reason it isn't reading AMR's currently, might be due to lack of mag but am not sure
            }
            else
            {
                AmmoMag = Reciever.WeaponStats.MagSize + Magazine?.WeaponModifiers?.ammo ?? 0;
            }
            AmmoRes = AmmoMag * Reciever.WeaponStats.InitialMagazines;

            double allRecoil = Barrel?.WeaponModifiers?.recoil ?? 0;
            allRecoil += Muzzle?.WeaponModifiers?.recoil ?? 0;
            allRecoil += Stock?.WeaponModifiers?.recoil ?? 0;
            allRecoil += Scope?.WeaponModifiers?.recoil ?? 0;
            allRecoil += Magazine?.WeaponModifiers?.recoil ?? 0;
            allRecoil /= 100.0f;
            allRecoil = Math.Min(Math.Max(allRecoil, -1.0f), 1.0f);
            Recoil = CalculateRecoil(Reciever, allRecoil);
            RecoilZoom = Recoil * Reciever.WeaponStats.RecoilZoomMultiplier * 0.8;


            double allDamage = Barrel?.WeaponModifiers?.damage ?? 0;
            allDamage += Muzzle?.WeaponModifiers?.damage ?? 0;
            allDamage += Stock?.WeaponModifiers?.damage ?? 0;
            allDamage += Scope?.WeaponModifiers?.damage ?? 0;
            allDamage += Magazine?.WeaponModifiers?.damage ?? 0;
            allDamage /= 100.0f;
            allDamage = Math.Min(Math.Max(allDamage, -1.0f), 1.0f);
            var damages = CalculateDamage(Reciever, allDamage);
            Damage = damages[0];
            DamageFar = damages[1];

            double allRange = Barrel?.WeaponModifiers?.range ?? 0;
            allRange += Muzzle?.WeaponModifiers?.range ?? 0;
            allRange += Stock?.WeaponModifiers?.range ?? 0;
            allRange += Scope?.WeaponModifiers?.range ?? 0;
            allRange += Magazine?.WeaponModifiers?.range ?? 0;
            allRange /= 100.0f;
            allRange = Math.Min(Math.Max(allRange, -1.0f), 1.0f);
            var ranges = CalculateRange(Reciever, allRange);
            RangeClose = ranges[0];
            RangeFar = ranges[1];
            RangeMax = ranges[2];

            double allMovementSpread = Barrel?.WeaponModifiers?.movementSpeed ?? 0;  // For specifically my move spread change, hence no mag/scope added.
            allMovementSpread += Muzzle?.WeaponModifiers?.movementSpeed ?? 0; //muzzle is always zero
            allMovementSpread += Stock?.WeaponModifiers?.movementSpeed ?? 0;
            allMovementSpread /= 100.0f;
            allMovementSpread = Math.Min(Math.Max(allMovementSpread, -1.0f), 1.0f);

            double allMovementScopeIn = Barrel?.WeaponModifiers?.movementSpeed ?? 0;
            allMovementScopeIn += Stock?.WeaponModifiers?.movementSpeed ?? 0;
            allMovementScopeIn /= 80.0f;
            allMovementScopeIn = Math.Min(Math.Max(allMovementScopeIn, -1.0f), 1.0f);
            double WikiScopeIn = ScopeIn;
            BaseScopeIn = CalculateBaseScopeIn(Reciever, allMovementScopeIn, WikiScopeIn, Scope);

            double allAccuracy = Barrel?.WeaponModifiers?.accuracy ?? 0;
            allAccuracy += Muzzle?.WeaponModifiers?.accuracy ?? 0;
            allAccuracy += Stock?.WeaponModifiers?.accuracy ?? 0;
            allAccuracy += Scope?.WeaponModifiers?.accuracy ?? 0;
            allAccuracy += Magazine?.WeaponModifiers?.accuracy ?? 0;
            allAccuracy /= 100.0f;
            allAccuracy = Math.Min(Math.Max(allAccuracy, -1.0f), 1.0f);
            var spreads = CalculateSpread(Reciever, allAccuracy, allMovementSpread);
            Aim = spreads[0];
            Hip = spreads[1];
            Move = spreads[2];

            double allMovementSpeed = Barrel?.WeaponModifiers?.movementSpeed ?? 0;
            allMovementSpeed += Muzzle?.WeaponModifiers?.movementSpeed ?? 0;
            allMovementSpeed += Stock?.WeaponModifiers?.movementSpeed ?? 0;
            allMovementSpeed += Scope?.WeaponModifiers?.movementSpeed ?? 0;
            allMovementSpeed += Magazine?.WeaponModifiers?.movementSpeed ?? 0;
            allMovementSpeed /= 100.0f;
            allMovementSpeed = Math.Min(Math.Max(allMovementSpeed, -1.0f), 1.0f);
            MoveSpeed = CalculateSpeed(Reciever, allMovementSpeed);

            double allReloadSpeed = Magazine?.WeaponModifiers?.reloadSpeed ?? 0;
            allReloadSpeed /= 100.0f;
            allReloadSpeed = Math.Min(Math.Max(allReloadSpeed, -1.0f), 1.0f);

            CockRateMultiplier = CalculateCockRate(Reciever, allRecoil);
            ReloadRateMultiplier = CalculateReloadRate(Reciever, allRecoil, allReloadSpeed);

            CalculateZoomROF(Reciever, allRecoil);

            List<BLRItem> mods = new();
            if (Barrel != null)
                mods.Add(Barrel);
            if (Magazine != null)
                mods.Add(Magazine);
            if (Muzzle != null)
                mods.Add(Muzzle);
            if (Scope != null)
                mods.Add(Scope);
            if (Stock != null)
                mods.Add(Stock);
            if (Stock != null)
                mods.Add(Stock);

            barrelVSmag = CompareItemDescriptor1(Barrel, Magazine);
            stockVSmuzzle = CompareItemDescriptor2(Stock, Muzzle, Scope);
            weaponDescriptor = Reciever.GetDescriptorName(TotalPoints(mods));

        }

        DamageLabel.Content = Damage.ToString("0.0") + " / " + DamageFar.ToString("0.0");
        ROFLabel.Content = (ROF * CockRateMultiplier).ToString("0");
        AmmoLabel.Content = AmmoMag.ToString("0") + " / " + AmmoRes.ToString("0");
        ReloadLabel.Content = (Reload * ReloadRateMultiplier).ToString("0.00") + "s";
        SwapLabel.Content = Swap.ToString("0.00");
        AimLabel.Content = Aim.ToString("0.00") + "°";
        HipLabel.Content = Hip.ToString("0.00") + "°";
        MoveLabel.Content = Move.ToString("0.00") + "°";
        RecoilLabel.Content = Recoil.ToString("0.00") + "°";
        ZoomRecoilLabel.Content = RecoilZoom.ToString("0.00") + "°";
        ZoomLabel.Content = Zoom.ToString("0.00");
        ScopeInLabel.Content = BaseScopeIn.ToString("0.000") + "s";
        RangeLabel.Content = RangeClose.ToString("0.0") + " / " + RangeFar.ToString("0.0") + " / " + RangeMax.ToString("0");
        RunLabel.Content = MoveSpeed.ToString("0.00");
        Descriptor.Content = barrelVSmag + " " + stockVSmuzzle + " " + weaponDescriptor;
        if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfoAppend(watch);
    }

    private static void UpdateAdvancedStats(BLRItem Reciever, BLRItem Barrel, BLRItem Magazine, BLRItem Muzzle, BLRItem Stock, Label VerticalRatio, Label SpreadWeight, Label SpreadCenter, Label Fragments, Label ZoomROF, Label SpreadCrouch, Label SpreadJump, Label RecoilRecover, Label Reload)
    {
        if (CheckCalculationReady(Reciever))
        {
            double allRecoil = Barrel?.WeaponModifiers?.recoil ?? 0;
            allRecoil += Muzzle?.WeaponModifiers?.recoil ?? 0;
            allRecoil += Stock?.WeaponModifiers?.recoil ?? 0;
            allRecoil += Magazine?.WeaponModifiers?.recoil ?? 0;
            allRecoil /= 100.0f;
            allRecoil = Math.Min(Math.Max(allRecoil, -1.0f), 1.0f);

            VerticalRatio.Content = CalculateRecoilRatio(Reciever).ToString("0.00");
            SpreadWeight.Content = Reciever.WeaponStats.SpreadCenterWeight.ToString("0.00");
            SpreadCenter.Content = Reciever.WeaponStats.SpreadCenter.ToString("0.00");
            Fragments.Content = Reciever.WeaponStats.FragmentsPerShell.ToString("0");
            ZoomROF.Content = CalculateZoomROF(Reciever, allRecoil).ToString("0");
            SpreadCrouch.Content = Reciever.WeaponStats.CrouchSpreadMultiplier.ToString("0.00");
            SpreadJump.Content = Reciever.WeaponStats.JumpSpreadMultiplier.ToString("0.00");
            RecoilRecover.Content = CalculateRecoilRecovery(Reciever).ToString("0.00");

            // Reverted back to seconds for now because I found that elemental mags had reload percentages which means it likely doesn't do anything, in addition to LMG's quick mag previously having no percentage
            // I'll probably go back to percentages if I end up manually curating/fixing all of the mag's reloadSpeed values
            if (Magazine != null)
            {
                Reload.Content = Magazine.WikiStats.reload.ToString("0.00") + "s";
                //double allReload = Magazine?.WeaponModifiers?.reloadSpeed ?? 0;
                //Reload.Content = allReload.ToString("0") + "%";
            }
            else
            {
                Reload.Content = "0.00" + "s";
                //Reload.Content = "0" + "%";
            }
        }
    }

    private static double TotalPoints(IEnumerable<BLRItem> items)
    {
        double points = 0;
        foreach (BLRItem item in items)
        {
            points += item.WeaponModifiers.rating;
        }
        return points;
    }

    private static string CompareItemDescriptor1(BLRItem item1, BLRItem item2)
    {
        if (item1 == null && item2 != null)
        {
            return item2.DescriptorName;
        }
        else if (item1 != null && item2 == null)
        {
            return item1.DescriptorName;
        }
        else if (item1 == null && item2 == null)
        {
            return "Standard";
        }

        if (item1.WeaponModifiers.rating > item2.WeaponModifiers.rating)
        {
            return item1.DescriptorName;
        }
        else
        {
            return item2.DescriptorName;
        }
    }

    private static string CompareItemDescriptor2(BLRItem item1, BLRItem item2, BLRItem item3)
    {
        if (item1 == null && item2 == null && item3 == null)
        {
            return "Basic";
        }

        if (item1 == null && item2 != null)
        {
            return item2.DescriptorName;
        }
        else if (item1 != null && item2 == null)
        {
            return item1.DescriptorName;
        }
        else if (item1 == null && item2 == null && item3 != null)
        {
            return item3.DescriptorName;
        }

        if ((item1.WeaponModifiers.rating >= item2.WeaponModifiers.rating) && (item1.WeaponModifiers.rating >= item3.WeaponModifiers.rating))
        {
            if (item1.WeaponModifiers.rating > 0)
            {
                return item1.DescriptorName;
            }
            return "Basic";
        }
        else if ((item2.WeaponModifiers.rating >= item1.WeaponModifiers.rating) && (item2.WeaponModifiers.rating >= item3.WeaponModifiers.rating))
        {
            return item2.DescriptorName;
        }
        else if ((item3.WeaponModifiers.rating >= item1.WeaponModifiers.rating) && (item3.WeaponModifiers.rating >= item2.WeaponModifiers.rating))
        {
            return item3.DescriptorName;
        }

        return item1.DescriptorName;
    }

    public static double[] CalculateRange(BLRItem Reciever, double allRange)
    {
        if (Reciever != null && Reciever.WeaponStats != null)
        {
            double idealRange;
            double maxRange;
            double alpha = Math.Abs(allRange);
            if (allRange > 0)
            {
                idealRange = (int)Lerp(Reciever?.WeaponStats?.ModificationRangeIdealDistance.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeIdealDistance.Y ?? 0, alpha);
                maxRange = Lerp(Reciever?.WeaponStats?.ModificationRangeMaxDistance.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeMaxDistance.Y ?? 0, alpha);
            }
            else
            {
                idealRange = (int)Lerp(Reciever?.WeaponStats?.ModificationRangeIdealDistance.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeIdealDistance.X ?? 0, alpha);
                maxRange = Lerp(Reciever?.WeaponStats?.ModificationRangeMaxDistance.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeMaxDistance.X ?? 0, alpha);
            }

            return new double[] { idealRange / 100, maxRange / 100, (Reciever?.WeaponStats?.MaxTraceDistance ?? 0) / 100 };
        }
        else
        {
            return new double[] { 0, 0, 0 };
        }
    }

    /// <summary>
    /// Calculates Spread of reciever with precentage modifiers for Aim, Hip, Move Spread
    /// </summary>
    /// <param name="Reciever">The Reciever</param>
    /// <param name="allAccuracy">the percentages of all parts that modify the general accuracy</param>
    /// <param name="allMovementSpread">the percentages of all parts that modify the accuracy multiplier when moving</param>
    /// <returns>[0]Aim, [1]Hip, [2]Move</returns>
    public static double[] CalculateSpread(BLRItem Reciever, double allAccuracy, double allMovementSpread)
    {
        if (Reciever != null && Reciever.WeaponStats != null)
        {
            double accuracyBaseModifier;
            double accuracyTABaseModifier;
            double alpha = Math.Abs(allAccuracy);
            if (allAccuracy > 0)
            {
                accuracyBaseModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeBaseSpread.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeBaseSpread.Y ?? 0, alpha);
                accuracyTABaseModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeTABaseSpread.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeTABaseSpread.Y ?? 0, alpha);
            }
            else
            {
                accuracyBaseModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeBaseSpread.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeBaseSpread.X ?? 0, alpha);
                accuracyTABaseModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeTABaseSpread.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeTABaseSpread.X ?? 0, alpha);
            }

            double hip = accuracyBaseModifier * (180 / Math.PI);
            double aim = (accuracyBaseModifier * Reciever.WeaponStats.ZoomSpreadMultiplier) * (180 / Math.PI);
            if (Reciever.WeaponStats.UseTABaseSpread)
            {
                aim = accuracyTABaseModifier * (float)(180 / Math.PI);
            }

            double weight_alpha = Math.Abs(Reciever.WeaponStats.Weight / 80.0);
            double weight_clampalpha = Math.Min(Math.Max(weight_alpha, -1.0), 1.0); // Don't ask me why they clamp the absolute value with a negative, I have no idea.
            double weight_multiplier;
            if (Reciever.WeaponStats.Weight > 0)   // It was originally supposed to compare the total weight of equipped mods, but from what I can currently gather from the scripts, nothing modifies weapon weight so I'm just comparing base weight for now.
            {
                weight_multiplier = Lerp(Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.Y ?? 0, weight_clampalpha);  // Originally supposed to be a weapon specific range, but they all set the same values so it's not worth setting elsewhere.
            }
            else
            {
                weight_multiplier = Lerp(Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.X ?? 0, weight_clampalpha);
            }

            double move_alpha = Math.Abs(allMovementSpread); // Combied movement speed modifiers from only barrel and stock, divided by 100
            double move_multiplier; // Applying movement to it like this isn't how it's done to my current knowledge, but seems to be consistently closer to how it should be in most cases so far.
            if (allMovementSpread > 0)
            {
                move_multiplier = Lerp(Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.Y ?? 0, move_alpha);
            }
            else
            {
                move_multiplier = Lerp(Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeWeightMultiplier.X ?? 0, move_alpha);
            }

            double movemultiplier_current = 1.0 + ((Reciever.WeaponStats.MovementSpreadMultiplier - 1.0) * (weight_multiplier * move_multiplier));
            double moveconstant_current = Reciever.WeaponStats.MovementSpreadConstant * (weight_multiplier * move_multiplier);

            double move = ((accuracyBaseModifier + moveconstant_current) * (180 / Math.PI)) * movemultiplier_current;

            // Average spread over multiple shots to account for random center weight multiplier
            double[] averageSpread = { 0, 0, 0 };
            double magsize = Math.Min(Reciever?.WeaponStats.MagSize ?? 0, 15.0f);
            if (magsize <= 1)
            {
                magsize = Reciever.WeaponStats.InitialMagazines + 1.0;
            }
            if (magsize > 0)
            {
                double averageShotCount = Math.Max(magsize, 3.0f);
                for (int shot = 1; shot <= averageShotCount; shot++)
                {
                    if (shot > (averageShotCount - (averageShotCount * Reciever.WeaponStats.SpreadCenterWeight)))
                    {
                        averageSpread[0] += (aim * Reciever.WeaponStats.SpreadCenter);
                        averageSpread[1] += (hip * Reciever.WeaponStats.SpreadCenter);
                        averageSpread[2] += (move * Reciever.WeaponStats.SpreadCenter);
                    }
                    else
                    {
                        averageSpread[0] += aim;
                        averageSpread[1] += hip;
                        averageSpread[2] += move;
                    }
                }
                averageSpread[0] /= averageShotCount;
                averageSpread[1] /= averageShotCount;
                averageSpread[2] /= averageShotCount;
            }

            return averageSpread;
            //return new double[] { aim, hip, move };
        }
        else
        {
            return new double[] { 0, 0, 0 };
        }
    }

    public static double CalculateBaseScopeIn(BLRItem Reciever, double allMovementScopeIn, double WikiScopeIn, BLRItem Scope)
    {
        double TTTA_alpha = Math.Abs(allMovementScopeIn);
        double TightAimTime, ComboScopeMod, FourXAmmoCounterMod, ArmComInfraredMod, EMITechScopeMod, EMIInfraredMod, EMIInfraredMK2Mod, ArmComSniperMod, KraneSniperScopeMod, SilverwoodHeavyMod, FrontierSniperMod;

        // giant cheat incoming, please lord forgive me for what i am about to do
        if (allMovementScopeIn > 0)
        {
            TightAimTime = Lerp(0.225, 0.15, TTTA_alpha);
            ComboScopeMod = Lerp(0.0, 0.03, TTTA_alpha);
            FourXAmmoCounterMod = Lerp(WikiScopeIn, 0.16, TTTA_alpha);
            ArmComInfraredMod = Lerp(WikiScopeIn, 0.16, TTTA_alpha);
            EMITechScopeMod = Lerp(WikiScopeIn, 0.215, TTTA_alpha);
            EMIInfraredMod = Lerp(WikiScopeIn, 0.215, TTTA_alpha);
            EMIInfraredMK2Mod = Lerp(WikiScopeIn, 0.36, TTTA_alpha);
            ArmComSniperMod = Lerp(WikiScopeIn, 0.275, TTTA_alpha);
            KraneSniperScopeMod = Lerp(WikiScopeIn, 0.275, TTTA_alpha);
            SilverwoodHeavyMod = Lerp(WikiScopeIn, 0.275, TTTA_alpha);
            FrontierSniperMod = Lerp(WikiScopeIn, 0.315, TTTA_alpha);
        }
        else
        {
            TightAimTime = Lerp(0.225, 0.30, TTTA_alpha);
            ComboScopeMod = Lerp(0.0, -0.05, TTTA_alpha);
            FourXAmmoCounterMod = Lerp(WikiScopeIn, 0.10, TTTA_alpha);
            ArmComInfraredMod = Lerp(WikiScopeIn, 0.10, TTTA_alpha);
            EMITechScopeMod = Lerp(WikiScopeIn, 0.16, TTTA_alpha);
            EMIInfraredMod = Lerp(WikiScopeIn, 0.16, TTTA_alpha);
            EMIInfraredMK2Mod = Lerp(WikiScopeIn, 0.30, TTTA_alpha);
            ArmComSniperMod = Lerp(WikiScopeIn, 0.20, TTTA_alpha);
            KraneSniperScopeMod = Lerp(WikiScopeIn, 0.20, TTTA_alpha);
            SilverwoodHeavyMod = Lerp(WikiScopeIn, 0.20, TTTA_alpha);
            FrontierSniperMod = Lerp(WikiScopeIn, 0.235, TTTA_alpha);
        }

        if ((Reciever.WeaponStats?.TightAimTime ?? 0) > 0)
        {
            return Reciever.WeaponStats.TightAimTime;
        }
        else
        {
            if (TightAimTime > 0)
            {
                if (Scope.UID == 45005)
                {
                    return TightAimTime + ComboScopeMod + WikiScopeIn;
                }
                else if (Scope.UID == 45023)
                {
                    return TightAimTime + FourXAmmoCounterMod;
                }
                else if (Scope.UID == 45021)
                {
                    return TightAimTime + ArmComInfraredMod;
                }
                else if (Scope.UID == 45020)
                {
                    return TightAimTime + EMIInfraredMod;
                }
                else if (Scope.UID == 45019)
                {
                    return TightAimTime + EMIInfraredMK2Mod;
                }
                else if (Scope.UID == 45015)
                {
                    return TightAimTime + ArmComSniperMod;
                }
                else if (Scope.UID == 45008)
                {
                    return TightAimTime + SilverwoodHeavyMod;
                }
                else if (Scope.UID == 45007)
                {
                    return TightAimTime + KraneSniperScopeMod;
                }
                else if (Scope.UID == 45004)
                {
                    return TightAimTime + EMITechScopeMod;
                }
                else if (Scope.UID == 45001)
                {
                    return TightAimTime + FrontierSniperMod;
                }
                else
                {
                    return TightAimTime + WikiScopeIn;
                }
            }
        }
        return 0.225;
    }

    public static double CalculateSpeed(BLRItem Reciever, double allMovementSpeed)
    {
        if (Reciever != null && Reciever.WeaponStats != null)
        {
            double move_alpha = Math.Abs(allMovementSpeed);
            double move_modifier;
            if (allMovementSpeed > 0)
            {
                move_modifier = Lerp(Reciever.WeaponStats.ModificationRangeMoveSpeed.Z, Reciever.WeaponStats.ModificationRangeMoveSpeed.Y, move_alpha);
            }
            else
            {
                move_modifier = Lerp(Reciever.WeaponStats.ModificationRangeMoveSpeed.Z, Reciever.WeaponStats.ModificationRangeMoveSpeed.X, move_alpha);
            }
            double speed = (765 + (move_modifier * 0.9)) / 100.0f; // Apparently percent of movement from gear is applied to weapons, and not percent of movement from weapons
            return speed;
        }
        return 0;
    }

    public static double[] CalculateDamage(BLRItem Reciever, double allDamage)
    {
        if (Reciever != null && Reciever.WeaponStats != null)
        {
            double damageModifier;
            double alpha = Math.Abs(allDamage);
            if (allDamage > 0)
            {
                damageModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeDamage.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeDamage.Y ?? 0, alpha);
            }
            else
            {
                damageModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeDamage.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeDamage.X ?? 0, alpha);
            }

            return new double[] { damageModifier, damageModifier * (Reciever?.WeaponStats?.MaxRangeDamageMultiplier ?? 0.1d) };
        }
        else
        {
            return new double[] { 0, 0 };
        }
    }

    public static double CalculateRecoil(BLRItem Reciever, double allRecoil)
    {
        if (Reciever != null && Reciever.WeaponStats != null)
        {
            double recoilModifier;
            double alpha = Math.Abs(allRecoil);
            if (allRecoil > 0)
            {
                recoilModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeRecoil.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeRecoil.Y ?? 0, alpha);
            }
            else
            {
                recoilModifier = Lerp(Reciever?.WeaponStats?.ModificationRangeRecoil.Z ?? 0, Reciever?.WeaponStats?.ModificationRangeRecoil.X ?? 0, alpha);
            }
            if (Reciever?.WeaponStats.MagSize > 0)
            {
                double averageShotCount = Math.Min(Reciever?.WeaponStats.MagSize ?? 0, 15.0f);
                Vector3 averageRecoil = new(0, 0, 0);

                for (int shot = 1; shot <= averageShotCount; shot++)
                {
                    Vector3 newRecoil = new(0, 0, 0)
                    {
                        X = (Reciever.WeaponStats.RecoilVector.X * Reciever.WeaponStats.RecoilVectorMultiplier.X * 0.5f) / 2.0f, // in the recoil, recoil vector is actually a multiplier on a random X and Y value in the -0.5/0.5 and 0.0/0.3535 range respectively
                        Y = (Reciever.WeaponStats.RecoilVector.Y * Reciever.WeaponStats.RecoilVectorMultiplier.Y * 0.3535f)
                    };

                    double accumExponent = Reciever.WeaponStats.RecoilAccumulation;
                    if (accumExponent > 1)
                    {
                        accumExponent = ((accumExponent - 1.0) * Reciever.WeaponStats.RecoilAccumulationMultiplier) + 1.0; // Apparently this is how they apply the accumulation multiplier in the actual recoil
                    }

                    double previousMultiplier = Reciever.WeaponStats.RecoilSize * Math.Pow(shot / Reciever.WeaponStats.Burst, accumExponent);
                    double currentMultiplier = Reciever.WeaponStats.RecoilSize * Math.Pow(shot / Reciever.WeaponStats.Burst + 1.0f, accumExponent);
                    double multiplier = currentMultiplier - previousMultiplier;
                    newRecoil *= (float)multiplier;
                    averageRecoil += newRecoil;
                }

                if (averageShotCount > 0)
                {
                    averageRecoil /= (float)averageShotCount;
                }
                if (Reciever.WeaponStats.ROF > 0 && Reciever.WeaponStats.ApplyTime > 60 / Reciever.WeaponStats.ROF)
                {
                    averageRecoil *= (float)(60 / (Reciever.WeaponStats.ROF * Reciever.WeaponStats.ApplyTime));
                }
                double recoil = averageRecoil.Length() * recoilModifier;
                recoil *= (180 / Math.PI);
                return recoil;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return 0;
        }
    }

    // The ratio of vertical recoil
    public static double CalculateRecoilRatio(BLRItem Reciever)
    {
        if (Reciever != null && Reciever.WeaponStats != null)
        {
            double vertical = Reciever.WeaponStats.RecoilVector.Y * Reciever.WeaponStats.RecoilVectorMultiplier.Y * 0.3535;
            double horizontal = Reciever.WeaponStats.RecoilVector.X * Reciever.WeaponStats.RecoilVectorMultiplier.X * 0.5;
            if ((vertical + horizontal) != 0)
            {
                return vertical / (vertical + horizontal);
            }
            return 1;
        }
        return 1;
    }

    // Recoil recovery time
    public static double CalculateRecoilRecovery(BLRItem Reciever)
    {
        if (Reciever != null && Reciever.WeaponStats != null)
        {
            if (Reciever.WeaponStats.RecoveryTime > 0)
            {
                return Reciever.WeaponStats.RecoveryTime;
            }
            return 60 / Reciever.WeaponStats.ROF;
        }
        return 0;
    }

    // Rate of fire when zoomed
    public static double CalculateZoomROF(BLRItem Reciever, double allRecoil)
    {
        if (Reciever != null && Reciever.WeaponStats != null)
        {
            double cockrate = CalculateCockRate(Reciever, allRecoil);
            if (Reciever.WeaponStats.ZoomRateOfFire > 0)
            {
                return Reciever.WeaponStats.ZoomRateOfFire * cockrate;
            }
            return Reciever.WeaponStats.ROF * cockrate;
        }
        return 0;
    }

    public static double CalculateCockRate(BLRItem Reciever, double allRecoil)
    {
        double alpha = Math.Abs(allRecoil);
        double cockrate;
        if (Reciever.WeaponStats.ModificationRangeCockRate.Z != 0)
        {
            if (allRecoil > 0)
            {
                cockrate = Lerp(Reciever.WeaponStats.ModificationRangeCockRate.Z, Reciever.WeaponStats.ModificationRangeCockRate.Y, alpha);
            }
            else
            {
                cockrate = Lerp(Reciever.WeaponStats.ModificationRangeCockRate.Z, Reciever.WeaponStats.ModificationRangeCockRate.X, alpha);
            }
            if (cockrate > 0)
            {
                cockrate = 1.0 / cockrate;
            }
            return cockrate;
        }
        return 1.0;
    }

    public static double CalculateReloadRate(BLRItem Reciever, double allRecoil, double allReloadSpeed)
    {
        double WeaponReloadRate = 1.0;
        double rate_alpha;

        if (Reciever.WeaponStats.ModificationRangeReloadRate.Z > 0)
        {
            rate_alpha = Math.Abs(allReloadSpeed);
            if (allReloadSpeed > 0)
            {
                WeaponReloadRate = Lerp(Reciever.WeaponStats.ModificationRangeReloadRate.Z, Reciever.WeaponStats.ModificationRangeReloadRate.X, rate_alpha);
            }
            else
            {
                WeaponReloadRate = Lerp(Reciever.WeaponStats.ModificationRangeReloadRate.Z, Reciever.WeaponStats.ModificationRangeReloadRate.Y, rate_alpha);
            }
        }

        if (Reciever.WeaponStats.ModificationRangeRecoilReloadRate.Z == 1)
        {
            rate_alpha = Math.Abs(allRecoil);
            if (allRecoil > 0)
            {
                WeaponReloadRate = Lerp(Reciever.WeaponStats.ModificationRangeRecoilReloadRate.Z, Reciever.WeaponStats.ModificationRangeRecoilReloadRate.X, rate_alpha);
            }
            else
            {
                WeaponReloadRate = Lerp(Reciever.WeaponStats.ModificationRangeRecoilReloadRate.Z, Reciever.WeaponStats.ModificationRangeRecoilReloadRate.Y, rate_alpha);
            }
        }
        return WeaponReloadRate;
    }

    #endregion OldCalcs

    private static void CheckValidity(Image image, BLRItem item)
    {
        if (item.Tooltip == "Depot Item!") image.DataContext = null;
        if(image.DataContext is BLRItem BLRItem)
        {
            if (!BLRItem.IsValidFor(item)) //|| !item.IsValidModType(BLRItem.Category)
            { if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo(BLRItem.Name + " was not a Valid Mod for " + item.Name); image.DataContext = null; }
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SetItemList(ImportSystem.PRIMARY_CATEGORY);
        if (App.IsNewVersionAvailable && BLREditSettings.Settings.ShowUpdateNotice)
        {
            System.Diagnostics.Process.Start("https://github.com/" + App.CurrentOwner + "/" + App.CurrentRepo + "/releases");
        }
        if (BLREditSettings.Settings.DoRuntimeCheck || BLREditSettings.Settings.ForceRuntimeCheck)
        {
            if(App.IsBaseRuntimeMissing || App.IsUpdateRuntimeMissing || BLREditSettings.Settings.ForceRuntimeCheck)
            {
                var info = new InfoPopups.DownloadRuntimes();
                if (!App.IsUpdateRuntimeMissing)
                {
                    info.Link2012Update4.IsEnabled = false;
                    info.Link2012Updatet4Content.Text = "Microsoft Visual C++ 2012 Update 4(x86/32bit) is already installed!";
                }
                info.ShowDialog();
            }
        }
    }

    private void ItemList_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            if(element.DataContext is BLRItem item)
            {
                if (e.ClickCount >= 2)
                {
                    if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Double Clicking:" + item.Name);
                    SetItemToImage(LastSelectedImage, item);
                }
                else
                {
                    if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Dragging:" + item.Name);
                    DragDrop.DoDragDrop(element, item, DragDropEffects.Copy);
                }
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
            if (e.Data.GetData(typeof(BLRItem)) is BLRItem item)
            {
                if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Recieving:" + item.Name);
                if (border.Child is Grid grid)
                {
                    if (grid.Children[0] is Image img)
                    {
                        SetItemToImage(img, item);
                    }
                }
                if (border.Child is Image image)
                {
                    SetItemToImage(image, item);
                }
            }
        }
        
    }

    public void SetItemToImage(Image image, BLRItem item, bool updateLoadout = true)
    {
        switch (image.Name)
        {
            case nameof(PrimaryRecieverImage):
                Loadout.Primary.Reciever = item;
                break;
            case nameof(SecondaryRecieverImage):
                Loadout.Secondary.Reciever = item;
                break;

            case nameof(PrimaryBarrelImage):
                Loadout.Primary.Barrel = item;
                break;
            case nameof(SecondaryBarrelImage):
                Loadout.Secondary.Barrel = item;
                break;

            case nameof(PrimaryMuzzleImage):
                Loadout.Primary.Muzzle = item;
                break;
            case nameof(SecondaryMuzzleImage):
                Loadout.Secondary.Muzzle = item;
                break;

            case nameof(PrimaryMagazineImage):
                Loadout.Primary.Magazine = item;
                break;
            case nameof(SecondaryMagazineImage):
                Loadout.Secondary.Magazine = item;
                break;

            case nameof(PrimaryStockImage):
                Loadout.Primary.Stock = item;
                break;
            case nameof(SecondaryStockImage):
                Loadout.Secondary.Stock = item;
                break;

            case nameof(PrimaryScopeImage):
                Loadout.Primary.Scope = item;
                break;
            case nameof(SecondaryScopeImage):
                Loadout.Secondary.Scope = item;
                break;

            case nameof(SecondaryGripImage):
                Loadout.Secondary.Grip = item;
                break;

            case nameof(PrimaryTagImage):
                Loadout.Primary.Tag = item;
                break;
            case nameof(SecondaryTagImage):
                Loadout.Secondary.Tag = item;
                break;

            case nameof(PrimaryCamoWeaponImage):
                Loadout.Primary.Camo = item;
                break;
            case nameof(SecondaryCamoWeaponImage):
                Loadout.Secondary.Camo = item;
                break;


            case nameof(HelmetImage):
                Loadout.Helmet = item;
                break;
            case nameof(UpperBodyImage):
                Loadout.UpperBody = item;
                break;
            case nameof(LowerBodyImage):
                Loadout.LowerBody = item;
                break;
            case nameof(TacticalImage):
                Loadout.Tactical = item;
                break;
            case nameof(GearImage1):
                Loadout.Gear1 = item;
                break;
            case nameof(GearImage2):
                Loadout.Gear2 = item;
                break;
            case nameof(GearImage3):
                Loadout.Gear3 = item;
                break;
            case nameof(GearImage4):
                Loadout.Gear4 = item;
                break;
            case nameof(PlayerCamoBodyImage):
                Loadout.Camo = item;
                break;
            case nameof(AvatarImage):
                Loadout.Avatar = item;
                break;
            case nameof(TrophyImage):
                Loadout.Trophy = item;
                break;
        }

        if (updateLoadout)
        {
            UpdateActiveLoadout2();
        }


    }

    private void UpdateActiveLoadout2()
    {
        Loadout.UpdateMagicCowsLoadout(ActiveLoadout);
    }

    public void UpdateArmorStats()
    {
        var helmet = (HelmetImage.DataContext as BLRItem);
        var upperBody = (UpperBodyImage.DataContext as BLRItem);
        var lowerBody = (LowerBodyImage.DataContext as BLRItem);
        var tactical = (TacticalImage.DataContext as BLRItem);
        UpdateHealth(helmet, upperBody, lowerBody);
        UpdateHeadProtection(helmet);
        UpdateRun(helmet, upperBody, lowerBody);
        UpdateHRV(helmet, tactical);
        UpdateHRVRecharge(helmet, tactical);
        UpdateGearSlots(upperBody, lowerBody);
        UpdateProtections();
    }


    public void UpdateProtections()
    {
        var helmet = (HelmetImage.DataContext as BLRItem);
        var gear1 = (GearImage1.DataContext as BLRItem);
        var gear2 = (GearImage2.DataContext as BLRItem);
        var gear3 = (GearImage3.DataContext as BLRItem);
        var gear4 = (GearImage4.DataContext as BLRItem);

        double electroProt = helmet?.PawnModifiers?.ElectroProtection ?? 0;
        electroProt += gear1?.PawnModifiers?.ElectroProtection ?? 0;
        electroProt += gear2?.PawnModifiers?.ElectroProtection ?? 0;
        electroProt += gear3?.PawnModifiers?.ElectroProtection ?? 0;
        electroProt += gear4?.PawnModifiers?.ElectroProtection ?? 0;
        electroProt = Math.Min(electroProt, 100.0);

        double exploProt = helmet?.PawnModifiers?.ExplosiveProtection ?? 0;
        exploProt += gear1?.PawnModifiers?.ExplosiveProtection ?? 0;
        exploProt += gear2?.PawnModifiers?.ExplosiveProtection ?? 0;
        exploProt += gear3?.PawnModifiers?.ExplosiveProtection ?? 0;
        exploProt += gear4?.PawnModifiers?.ExplosiveProtection ?? 0;
        exploProt = Math.Min(exploProt, 100.0);

        double incendiaryProt = helmet?.PawnModifiers?.IncendiaryProtection ?? 0;
        incendiaryProt += gear1?.PawnModifiers?.IncendiaryProtection ?? 0;
        incendiaryProt += gear2?.PawnModifiers?.IncendiaryProtection ?? 0;
        incendiaryProt += gear3?.PawnModifiers?.IncendiaryProtection ?? 0;
        incendiaryProt += gear4?.PawnModifiers?.IncendiaryProtection ?? 0;
        incendiaryProt = Math.Min(incendiaryProt, 100.0);

        double infraProt = helmet?.PawnModifiers?.InfraredProtection ?? 0;
        infraProt += gear1?.PawnModifiers?.InfraredProtection ?? 0;
        infraProt += gear2?.PawnModifiers?.InfraredProtection ?? 0;
        infraProt += gear3?.PawnModifiers?.InfraredProtection ?? 0;
        infraProt += gear4?.PawnModifiers?.InfraredProtection ?? 0;
        infraProt = Math.Min(infraProt, 100.0);

        double meleeProt = helmet?.PawnModifiers?.MeleeProtection ?? 0;
        meleeProt += gear1?.PawnModifiers?.MeleeProtection ?? 0;
        meleeProt += gear2?.PawnModifiers?.MeleeProtection ?? 0;
        meleeProt += gear3?.PawnModifiers?.MeleeProtection ?? 0;
        meleeProt += gear4?.PawnModifiers?.MeleeProtection ?? 0;
        meleeProt = Math.Min(meleeProt, 100.0);

        double toxicProt = helmet?.PawnModifiers?.ToxicProtection ?? 0;
        toxicProt += gear1?.PawnModifiers?.ToxicProtection ?? 0;
        toxicProt += gear2?.PawnModifiers?.ToxicProtection ?? 0;
        toxicProt += gear3?.PawnModifiers?.ToxicProtection ?? 0;
        toxicProt += gear4?.PawnModifiers?.ToxicProtection ?? 0;
        toxicProt = Math.Min(toxicProt, 100.0);

        ArmorGearElectroProtectionLabel.Content = electroProt.ToString("0") + '%';
        ArmorGearExplosiveProtectionLabel.Content = exploProt.ToString("0") + '%';
        ArmorGearIncendiaryLabel.Content = incendiaryProt.ToString("0") + '%';
        ArmorGearInfraredProtectionLabel.Content = infraProt.ToString("0") + '%';
        ArmorGearMeleeProtectionLabel.Content = meleeProt.ToString("0") + '%';
        ArmorGearToxicProtectionLabel.Content = toxicProt.ToString("0") + '%';
    }


    public void UpdateHealth(BLRItem helmet, BLRItem upperBody, BLRItem lowerBody)
    {
        double allHealth = (helmet?.PawnModifiers.Health ?? 0);
        allHealth += (upperBody?.PawnModifiers.Health ?? 0);
        allHealth += (lowerBody?.PawnModifiers.Health ?? 0);
        allHealth = Math.Min(Math.Max((int)allHealth, -100), 100);

        HealthGearModLabel.Content = allHealth.ToString("0") + '%';

        double health_alpha = Math.Abs(allHealth) / 100;

        double basehealth = 200;
        double currentHealth;

        if (allHealth > 0)
        {
            currentHealth = Lerp(basehealth, 250, health_alpha);
        }
        else
        {
            currentHealth = Lerp(basehealth, 150, health_alpha);
        }

        ArmorHealthLabel.Content = currentHealth.ToString("0");
    }
    public void UpdateHeadProtection(BLRItem helmet)
    {
        double currentHSProt = (helmet?.PawnModifiers.HelmetDamageReduction ?? 0);
        ArmorHeadProtectionLabel.Content = currentHSProt.ToString("0.0") + '%';
        HeadArmorGearModLabel.Content = currentHSProt.ToString("0.0") + '%';

    }
    public void UpdateRun(BLRItem helmet, BLRItem upperBody, BLRItem lowerBody)
    {
        double finalRun = GetMoveSpeedArmor(helmet, upperBody, lowerBody) / 100.0;
        ArmorRunLabel.Content = finalRun.ToString("0.00");
    }

    public static double GetMoveSpeedArmor(BLRItem helmet, BLRItem upperBody, BLRItem lowerBody)
    {
        double allRun = (helmet?.PawnModifiers.MovementSpeed ?? 0);
        allRun += (upperBody?.PawnModifiers.MovementSpeed ?? 0);
        allRun += (lowerBody?.PawnModifiers.MovementSpeed ?? 0);
        allRun = Math.Min(Math.Max(allRun, -100), 100);

        Self.RunGearModLabel.Content = allRun.ToString("0") + '%';

        double run_alpha = Math.Abs(allRun) / 100;

        double baserun = 765;
        double currentRun;

        if (allRun > 0)
        {
            currentRun = Lerp(baserun, 900, run_alpha);
        }
        else
        {
            currentRun = Lerp(baserun, 630, run_alpha);
        }

        return currentRun;
    }
    public void UpdateHRV(BLRItem helmet, BLRItem tactical)
    {
        double allHRV = (helmet?.PawnModifiers.HRVDuration ?? 0) + (tactical?.PawnModifiers.HRVDuration ?? 0);
        double currentHRV = Math.Min(Math.Max(allHRV, 40.0), 100.0);
        ArmorHRVLabel.Content = currentHRV.ToString("0.0") + 'u';
        HRVDurationGearModLabel.Content = currentHRV.ToString("0.0") + 'u';
    }
    public void UpdateHRVRecharge(BLRItem helmet, BLRItem tactical)
    {
        double currentHRVRecharge = Math.Min(Math.Max((helmet?.PawnModifiers.HRVRechargeRate ?? 0) + (tactical?.PawnModifiers.HRVRechargeRate ?? 0), 5.0), 10.0);
        ArmorHRVRechargeLabel.Content = currentHRVRecharge.ToString("0.0") + "u/s";
        HRVRechargeGearModLabel.Content = currentHRVRecharge.ToString("0.0") + "u/s";
    }
    public void UpdateGearSlots(BLRItem upperBody, BLRItem lowerBody)
    { 
        double currentGearSlots = (upperBody?.PawnModifiers?.GearSlots ?? 0) + (lowerBody?.PawnModifiers?.GearSlots ?? 0);

        GearSlotsGearModLabel.Content = currentGearSlots.ToString("0");

        GearImage1.IsEnabled = false;
        GearImage2.IsEnabled = false;
        GearImage3.IsEnabled = false;
        GearImage4.IsEnabled = false;

        Gear1Rect.Visibility = Visibility.Visible;
        Gear2Rect.Visibility = Visibility.Visible;
        Gear3Rect.Visibility = Visibility.Visible;
        Gear4Rect.Visibility = Visibility.Visible;

        if (currentGearSlots > 0)
        { 
            GearImage1.IsEnabled = true;
            Gear1Rect.Visibility = Visibility.Hidden;
        }
        if (currentGearSlots > 1)
        {
            GearImage2.IsEnabled = true;
            Gear2Rect.Visibility = Visibility.Hidden;
        }
        if (currentGearSlots > 2)
        {
            GearImage3.IsEnabled = true;
            Gear3Rect.Visibility = Visibility.Hidden;
        }
        if (currentGearSlots > 3)
        {
            GearImage4.IsEnabled = true;
            Gear4Rect.Visibility = Visibility.Hidden;
        }
        ArmorGearLabel.Content = currentGearSlots.ToString("0");
    }

    private static bool CheckForPistolAndBarrel(BLRItem item)
    {
        return item.Name == "Light Pistol" || item.Name == "Heavy Pistol" || item.Name == "Prestige Light Pistol";
    }

    private void UpdatePrimaryImages(Image image)
    {
        FilterWeapon = PrimaryRecieverImage.Tag as BLRItem;

        if (image.Name.Contains("Reciever"))
        {
            SetItemList(ImportSystem.PRIMARY_CATEGORY);
            LastSelectedImage = PrimaryRecieverImage;
            return;
        }

        if (image.Name.Contains("Muzzle"))
        { 
            SetItemList(ImportSystem.MUZZELS_CATEGORY);
            LastSelectedImage = image;
            return;
        }
        if (image.Name.Contains("Crosshair"))
        {
            var item = (PrimaryScopeImage.Tag as BLRItem);
            item.LoadCrosshair(true);
            ItemList.ItemsSource = new BLRItem[] { item };
            LastSelectedImage = PrimaryScopeImage;
            return;
        }
        UpdateImages(image);
    }

    private void UpdateSecondaryImages(Image image)
    {
        FilterWeapon = SecondaryRecieverImage.Tag as BLRItem;
        
        if (image.Name.Contains("Reciever"))
        {
            SetItemList(ImportSystem.SECONDARY_CATEGORY);
            LastSelectedImage = SecondaryRecieverImage;
            return;
        }
        if (image.Name.Contains("Muzzle"))
        { 
            SetItemList(ImportSystem.MUZZELS_CATEGORY);
            LastSelectedImage = image;
            return;
        }
        if (image.Name.Contains("Grip"))
        {
            SetItemList(ImportSystem.GRIPS_CATEGORY);
            LastSelectedImage = image;
            return;
        }
        if (image.Name.Contains("Crosshair"))
        {
            var item = (SecondaryScopeImage.Tag as BLRItem);
            if (item != null)
            {
                item.LoadCrosshair(false);
                ItemList.ItemsSource = new BLRItem[] { item };
                LastSelectedImage = SecondaryScopeImage;
            }
            return;
        }
        UpdateImages(image);
    }

    private void UpdateImages(Image image)
    {
        LastSelectedImage = image;
        
        if (image.Name.Contains("Reciever"))
        {
            SetItemList(ImportSystem.PRIMARY_CATEGORY);
            
            return;
        }
        if (image.Name.Contains("Magazine"))
        {
            SetItemList(ImportSystem.MAGAZINES_CATEGORY);
            return;
        }
        if (image.Name.Contains("Stock"))
        {
            SetItemList(ImportSystem.STOCKS_CATEGORY);
            return;
        }
        if (image.Name.Contains("Scope"))
        {
            foreach (var item in ImportSystem.GetItemListOfType(ImportSystem.SCOPES_CATEGORY))
            {
                item.RemoveCrosshair();
            }
            SetItemList(ImportSystem.SCOPES_CATEGORY);
            return;
        }
        if (image.Name.Contains("Barrel"))
        {
            SetItemList(ImportSystem.BARRELS_CATEGORY);
            return;
        }
        if (image.Name.Contains("Tag"))
        {
            SetItemList(ImportSystem.HANGERS_CATEGORY);
            return;
        }
        if (image.Name.Contains("CamoWeapon"))
        {
            SetItemList(ImportSystem.CAMOS_WEAPONS_CATEGORY);
            return;
        }
    }

    public void SetItemList(string Type)
    {
        var list = ImportSystem.GetItemListOfType(Type);
        if (list.Count > 0)
        {
            int index = SortComboBox1.SelectedIndex;
            SortComboBox1.ItemsSource = null;
            SortComboBox1.Items.Clear();

            switch (list[0].Category)
            {
                case ImportSystem.HELMETS_CATEGORY:
                    SetSortingType(typeof(ImportHelmetSortingType));
                    break;

                case ImportSystem.UPPER_BODIES_CATEGORY:
                case ImportSystem.LOWER_BODIES_CATEGORY:
                    SetSortingType(typeof(ImportArmorSortingType));
                    break;

                case ImportSystem.ATTACHMENTS_CATEGORY:
                    SetSortingType(typeof(ImportGearSortingType));
                    break;

                case ImportSystem.SCOPES_CATEGORY:
                    SetSortingType(typeof(ImportScopeSortingType));
                    break;

                case ImportSystem.AVATARS_CATEGORY:
                case ImportSystem.CAMOS_BODIES_CATEGORY:
                case ImportSystem.CAMOS_WEAPONS_CATEGORY:
                case ImportSystem.HANGERS_CATEGORY:
                case ImportSystem.TACTICAL_CATEGORY:
                case ImportSystem.BADGES_CATEGORY:
                    SetSortingType(typeof(ImportNoStatsSortingType));
                    break;

                case ImportSystem.GRIPS_CATEGORY:
                    SetSortingType(typeof(ImportGripSortingType));
                    break;

                case ImportSystem.PRIMARY_CATEGORY:
                case ImportSystem.SECONDARY_CATEGORY:
                    SetSortingType(typeof(ImportWeaponSortingType));
                    break;

                default:
                    SetSortingType(typeof(ImportModificationSortingType));
                    break;
            }

            if (index > SortComboBox1.Items.Count)
            {
                index = SortComboBox1.Items.Count - 1;
            }
            if (index < 0)
            {
                index = 0;
            }
            SortComboBox1.SelectedIndex = index;

            ItemList.ItemsSource = list;
            ApplySorting();
        }
        if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("ItemList Set for " + Type);
    }

    public void ApplySorting()
    {
        CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ItemList.ItemsSource);
        if (view != null)
        {
            //Clear old sorting descriptions
            view.SortDescriptions.Clear();

            if (SortComboBox1.Items.Count > 0 && SortComboBox1.SelectedItem != null)
            {
                CurrentSortingPropertyName = Enum.GetName(CurrentSortingEnumType, Enum.GetValues(CurrentSortingEnumType).GetValue(SortComboBox1.SelectedIndex));
                view.SortDescriptions.Add(new SortDescription(CurrentSortingPropertyName, SortDirection));
            }
        }
    }

    private void SetSortingType(Type SortingEnumType)
    {
        CurrentSortingEnumType = SortingEnumType;
        SortComboBox1.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = API.ImportSystem.LanguageSet.GetWords(SortingEnumType) });
    }

    private void Image_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border)
        {
            if (border.Child is Grid grid)
            {
                if (grid.Children[0] is Image img)
                {
                    if (img.Name.Contains("Gear"))
                    {
                        SetItemList(ImportSystem.ATTACHMENTS_CATEGORY);
                        LastSelectedImage = img;
                        return;
                    }
                }
            }
            if (border.Child is Image image)
            {
                ItemList.ItemsSource = null;
                if (image.Name.Contains("Primary"))
                {
                    UpdatePrimaryImages(image);
                    return;
                }
                if (image.Name.Contains("Secondary"))
                {
                    UpdateSecondaryImages(image);
                    return;
                }
                if (image.Name.Contains("Tactical"))
                {
                    SetItemList(ImportSystem.TACTICAL_CATEGORY);
                    LastSelectedImage = image;
                    return;
                }
                if (image.Name.Contains("CamoBody"))
                {
                    SetItemList(ImportSystem.CAMOS_BODIES_CATEGORY);
                    LastSelectedImage = image;
                    return;
                }
                if (image.Name.Contains("Helmet"))
                {
                    SetItemList(ImportSystem.HELMETS_CATEGORY);
                    LastSelectedImage = image;
                    return;
                }
                if (image.Name.Contains("UpperBody"))
                {
                    SetItemList(ImportSystem.UPPER_BODIES_CATEGORY);
                    LastSelectedImage = image;
                    return;
                }

                if (image.Name.Contains("Avatar"))
                {
                    SetItemList(ImportSystem.AVATARS_CATEGORY);
                    LastSelectedImage = image;
                    return;
                }

                if (image.Name.Contains("LowerBody"))
                {
                    SetItemList(ImportSystem.LOWER_BODIES_CATEGORY);
                    LastSelectedImage = image;
                    return;
                }

                if (image.Name.Contains("Trophy"))
                {
                    SetItemList(ImportSystem.BADGES_CATEGORY);
                    LastSelectedImage = image;
                    return;
                }
                if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("ItemList Din't get set");
            }
        }
    }

    public void SetLoadout(MagiCowsLoadout loadout)
    {
        ActiveLoadout = loadout;
        Loadout.LoadMagicCowsLoadout(loadout);
        //UpdateStats2();
    }
    
    private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsPlayerNameChanging)
        {
            IsPlayerProfileChanging = true;
            if (ProfileComboBox.SelectedValue is ExportSystemProfile profile)
            {
                ExportSystem.ActiveProfile = profile;
                PlayerNameTextBox.Text = profile.PlayerName;
                SetLoadout(profile.Loadout1);
            }
            IsPlayerProfileChanging = false;
        }
    }
    private void PlayerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsPlayerProfileChanging)
        {
            IsPlayerNameChanging = true;
            int index = ProfileComboBox.SelectedIndex;
            ProfileComboBox.ItemsSource = null;
            //ProfileComboBox.Items.Clear();
            ExportSystem.RemoveActiveProfileFromDisk();
            ExportSystem.ActiveProfile.PlayerName = PlayerNameTextBox.Text;
            ProfileComboBox.ItemsSource = ExportSystem.Profiles;
            ProfileComboBox.SelectedIndex = index;
            IsPlayerNameChanging = false;
        }
    }
    private void AddProfileButton_Click(object sender, RoutedEventArgs e)
    {
        ExportSystem.AddProfile();
        this.ProfileComboBox.DataContext = null;
        this.ProfileComboBox.DataContext = ExportSystem.Profiles;
    }

    private void Loadout1Button_Click(object sender, RoutedEventArgs e)
    {
        SetLoadout(ExportSystem.ActiveProfile.Loadout1);
        Loadout1Button.IsEnabled = false;
        Loadout2Button.IsEnabled = true;
        Loadout3Button.IsEnabled = true;
    }

    private void Loadout2Button_Click(object sender, RoutedEventArgs e)
    {
        SetLoadout(ExportSystem.ActiveProfile.Loadout2);
        Loadout1Button.IsEnabled = true;
        Loadout2Button.IsEnabled = false;
        Loadout3Button.IsEnabled = true;
    }

    private void Loadout3Button_Click(object sender, RoutedEventArgs e)
    {
        SetLoadout(ExportSystem.ActiveProfile.Loadout3);
        Loadout1Button.IsEnabled = true;
        Loadout2Button.IsEnabled = true;
        Loadout3Button.IsEnabled = false;
    }

    private void CopyToClipboardButton_Click(object sender, RoutedEventArgs e)
    {
        ExportSystem.CopyToClipBoard(ExportSystem.ActiveProfile);
        //ExportSystem.CreateSEProfile(ExportSystem.ActiveProfile); //Not really in use currently only makes waste on the Hard Drive
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        ExportSystem.SaveProfiles();
    }

    private void IsFemaleCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        Loadout.IsFemale = true;
        UpdateActiveLoadout2();
        //reset item list
        var source = ItemList.ItemsSource;
        ItemList.ItemsSource = null;
        ItemList.ItemsSource = source;
    }

    private void IsFemaleCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        Loadout.IsFemale = false;
        UpdateActiveLoadout2();

        //reset item list
        var source = ItemList.ItemsSource;
        ItemList.ItemsSource = null;
        ItemList.ItemsSource = source;
    }

    private void SortComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplySorting();
    }

    private void ChangeSortingDirection(object sender, RoutedEventArgs e)
    {
        if (SortDirection == ListSortDirection.Ascending)
        {
            SortDirection = ListSortDirection.Descending;
            SortDirectionButton.Content = "Descending";
        }
        else
        {
            SortDirection = ListSortDirection.Ascending;
            SortDirectionButton.Content = "Ascending";
        }
        ApplySorting();
    }

    private void RandomLoadout_Click(object sender, RoutedEventArgs e)
    {
        ExportSystemProfile rngProfile = ExportSystem.AddProfile("Random");

        rngProfile.Loadout1 = RandomizeLoadout();
        rngProfile.Loadout2 = RandomizeLoadout();
        rngProfile.Loadout3 = RandomizeLoadout();

        IsPlayerProfileChanging = true;
        IsPlayerNameChanging = true;

        ExportSystem.ActiveProfile = rngProfile;

        PlayerNameTextBox.Text = ExportSystem.ActiveProfile.PlayerName;

        ProfileComboBox.SelectedIndex = ExportSystem.Profiles.IndexOf(ExportSystem.ActiveProfile);

        SetLoadout(ExportSystem.ActiveProfile.Loadout1);

        Loadout1Button.IsEnabled = false;
        Loadout2Button.IsEnabled = true;
        Loadout3Button.IsEnabled = true;

        IsPlayerProfileChanging = false;
        IsPlayerNameChanging = false;


        LastSelectedImage = PrimaryRecieverImage;
    }


    static readonly List<BLRItem> Primaries = ImportSystem.GetItemListOfType(ImportSystem.PRIMARY_CATEGORY);
    static readonly List<BLRItem> Secondaries = ImportSystem.GetItemListOfType(ImportSystem.SECONDARY_CATEGORY);

    static readonly List<BLRItem> Barrels = ImportSystem.GetItemListOfType(ImportSystem.BARRELS_CATEGORY);
    static readonly List<BLRItem> Scopes = ImportSystem.GetItemListOfType(ImportSystem.SCOPES_CATEGORY);
    static readonly List<BLRItem> Magazines = ImportSystem.GetItemListOfType(ImportSystem.MAGAZINES_CATEGORY);
    static readonly List<BLRItem> Muzzles = ImportSystem.GetItemListOfType(ImportSystem.MUZZELS_CATEGORY);
    static readonly List<BLRItem> Stocks = ImportSystem.GetItemListOfType(ImportSystem.STOCKS_CATEGORY);
    static readonly List<BLRItem> Hangers = ImportSystem.GetItemListOfType(ImportSystem.HANGERS_CATEGORY);
    static readonly List<BLRItem> CamosWeapon = ImportSystem.GetItemListOfType(ImportSystem.CAMOS_WEAPONS_CATEGORY);
    static readonly List<BLRItem> Grips = ImportSystem.GetItemListOfType(ImportSystem.GRIPS_CATEGORY);


    static readonly List<BLRItem> Tacticals = ImportSystem.GetItemListOfType(ImportSystem.TACTICAL_CATEGORY);
    static readonly List<BLRItem> Attachments = ImportSystem.GetItemListOfType(ImportSystem.ATTACHMENTS_CATEGORY);
    static readonly List<BLRItem> CamosBody = ImportSystem.GetItemListOfType(ImportSystem.CAMOS_BODIES_CATEGORY);
    static readonly List<BLRItem> Helmets = ImportSystem.GetItemListOfType(ImportSystem.HELMETS_CATEGORY);
    static readonly List<BLRItem> UpperBodies = ImportSystem.GetItemListOfType(ImportSystem.UPPER_BODIES_CATEGORY);
    static readonly List<BLRItem> LowerBodies = ImportSystem.GetItemListOfType(ImportSystem.LOWER_BODIES_CATEGORY);
    static readonly List<BLRItem> Avatars = ImportSystem.GetItemListOfType(ImportSystem.AVATARS_CATEGORY);
    static readonly List<BLRItem> Badges = ImportSystem.GetItemListOfType(ImportSystem.BADGES_CATEGORY);

    private static MagiCowsLoadout RandomizeLoadout()
    {
        MagiCowsLoadout loadout = new()
        {
            Primary = RandomizeWeapon(Primaries[rng.Next(0, Primaries.Count)]),
            Secondary = RandomizeWeapon(Secondaries[rng.Next(0, Secondaries.Count)], true),
            Tactical = rng.Next(0, Tacticals.Count),

            Gear1 = rng.Next(0, Attachments.Count),
            Gear2 = rng.Next(0, Attachments.Count),
            Gear3 = rng.Next(0, Attachments.Count),
            Gear4 = rng.Next(0, Attachments.Count),

            Camo = rng.Next(0, CamosBody.Count),

            Helmet = rng.Next(0, Helmets.Count),
            UpperBody = rng.Next(0, UpperBodies.Count),
            LowerBody = rng.Next(0, LowerBodies.Count),
            Skin = rng.Next(0, Avatars.Count),
            Trophy = rng.Next(0, Badges.Count),

            IsFemale = NextBoolean()
        };


        double gearSlots = 0;
        gearSlots += UpperBodies[loadout.UpperBody].PawnModifiers.GearSlots;
        gearSlots += LowerBodies[loadout.LowerBody].PawnModifiers.GearSlots;

        if (gearSlots < 4)
        {
            loadout.Gear4 = 0;
        }
        if (gearSlots < 3)
        {
            loadout.Gear3 = 0;
        }
        if (gearSlots < 2)
        {
            loadout.Gear2 = 0;
        }
        if (gearSlots < 1)
        {
            loadout.Gear1 = 0;
        }


        return loadout;
    }

    public static bool NextBoolean()
    {
        return rng.Next() > (Int32.MaxValue / 2);
        // Next() returns an int in the range [0..Int32.MaxValue]
    }

    private static MagiCowsWeapon RandomizeWeapon(BLRItem Weapon, bool IsSecondary = false)
    {
        MagiCowsWeapon weapon = MagiCowsWeapon.GetDefaultSetupOfReciever(Weapon);

        if (weapon == null)
        {
            return new()
            {
                Receiver = Weapon.Name
            };
        }

        var FilteredBarrels = Barrels.Where(o => o.IsValidFor(Weapon)).ToArray();
        var FilteredScopes = Scopes.Where(o => o.IsValidFor(Weapon)).ToArray();
        var FilteredMagazines = Magazines.Where(o => o.IsValidFor(Weapon)).ToArray();
        //Dependant of Barrel on secondarioes
        var FilteredMuzzles = Muzzles.Where(o => o.IsValidFor(Weapon)).ToArray();
        var FilteredStocks = Stocks.Where(o => o.IsValidFor(Weapon)).ToArray();
        var FilteredCamos = CamosWeapon.Where(o => o.IsValidFor(Weapon)).ToArray();
        var FilteredHangers = Hangers.Where(o => o.IsValidFor(Weapon)).ToArray();

        if (FilteredBarrels.Length > 0)
        {
            weapon.Barrel = FilteredBarrels[rng.Next(0, FilteredBarrels.Length)].Name;
        }

        if (FilteredScopes.Length > 0)
        {
            weapon.Scope = FilteredScopes[rng.Next(0, FilteredScopes.Length)].Name;
        }

        if (FilteredMagazines.Length > 0)
        {
            weapon.Magazine = ImportSystem.GetIDOfItem(FilteredMagazines[rng.Next(0, FilteredMagazines.Length)]);
        }

        if (CheckForPistolAndBarrel(Weapon) && weapon.Barrel != "" && weapon.Barrel != MagiCowsWeapon.NoBarrel || !IsSecondary)
        {
            if (FilteredStocks.Length > 0)
            { 
                weapon.Stock = FilteredStocks[rng.Next(0, FilteredStocks.Length)].Name;
            }
        }

        if (FilteredMuzzles.Length > 0)
        {
            weapon.Muzzle = ImportSystem.GetIDOfItem(FilteredMuzzles[rng.Next(0, FilteredMuzzles.Length)]);
        }

        if (FilteredHangers.Length > 0)
        { 
            weapon.Tag = ImportSystem.GetIDOfItem(FilteredHangers[rng.Next(0, FilteredHangers.Length)]);
        }

        if (FilteredCamos.Length > 0 && Weapon.Tooltip != "Depot Item!")
        {
            weapon.Camo = ImportSystem.GetIDOfItem(FilteredCamos[rng.Next(0, FilteredCamos.Length)]);
        }
        else
        {
            weapon.Camo = -1;
        }

        if (Weapon.Name == "Shotgun")
        {
            weapon.Grip = Grips[rng.Next(0, Grips.Count)].Name;
        }
        else
        {
            weapon.Grip = "";
        }

        weapon.IsHealthOkAndRepair();

        return weapon;
    }

    private void ItemListButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        { 
            ItemListButton.IsEnabled = true;
            AdvancedInfoButton.IsEnabled = true;
            button.IsEnabled = false;

            if (sender == ItemListButton)
            {
                ItemList.Visibility = Visibility.Visible;
                ItemList.IsEnabled = true;
            }
            else
            {
                ItemList.Visibility = Visibility.Collapsed;
                ItemList.IsEnabled = false;
            }

            if (sender == AdvancedInfoButton)
            {
                AdvancedInfo.Visibility = Visibility.Visible;
                AdvancedInfo.IsEnabled = true;
            }
            else
            {
                AdvancedInfo.Visibility = Visibility.Collapsed;
                AdvancedInfo.IsEnabled = false;
            }
        }
    }
}
