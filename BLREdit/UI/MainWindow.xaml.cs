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

namespace BLREdit.UI
{

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

        /// <summary>
        /// Contains the Sorting Direction for the ItemList
        /// </summary>
        public ListSortDirection SortDirection { get; set; } = ListSortDirection.Descending;

        public string CurrentSortingPropertyName { get; set; } = "None";

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
        }

        private void UpdatePrimaryStats()
        {
            UpdateStats(
                PrimaryRecieverImage.DataContext as BLRItem,
                PrimaryBarrelImage.DataContext as BLRItem,
                PrimaryMagazineImage.DataContext as BLRItem,
                PrimaryMuzzleImage.DataContext as BLRItem,
                PrimaryScopeImage.DataContext as BLRItem,
                PrimaryStockImage.DataContext as BLRItem,
                PrimaryDamageLabel,
                PrimaryRateOfFireLabel,
                PrimaryAmmoLabel,
                PrimaryReloadLabel,
                PrimarySwapLabel,
                PrimaryAimLabel,
                PrimaryHipLabel,
                PrimaryMoveLabel,
                PrimaryRecoilLabel,
                PrimaryZoomRecoilLabel,
                PrimaryZoomLabel,
                PrimaryScopeInLabel,
                PrimaryRangeLabel,
                PrimaryRunLabel,
                PrimaryDescriptorLabel
                );
        }

        private void UpdateSecondaryStats()
        {
            UpdateStats(
                SecondaryRecieverImage.DataContext as BLRItem,
                SecondaryBarrelImage.DataContext as BLRItem,
                SecondaryMagazineImage.DataContext as BLRItem,
                SecondaryMuzzleImage.DataContext as BLRItem,
                SecondaryScopeImage.DataContext as BLRItem,
                SecondaryStockImage.DataContext as BLRItem,
                SecondaryDamageLabel,
                SecondaryRateOfFireLabel,
                SecondaryAmmoLabel,
                SecondaryReloadLabel,
                SecondarySwapLabel,
                SecondaryAimLabel,
                SecondaryHipLabel,
                SecondaryMoveLabel,
                SecondaryRecoilLabel,
                SecondaryZoomRecoilLabel,
                SecondaryZoomLabel,
                SecondaryScopeInLabel,
                SecondaryRangeLabel,
                SecondaryRunLabel,
                SecondaryDescriptorLabel
        );
        }

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
            foreach (BLRItem item in items)
            {
                ROF += item?.WeaponStats?.ROF ?? 0;
                Reload += item?.WikiStats?.reload ?? 0;
                Swap += item?.WikiStats?.swaprate ?? 0;
                Zoom += item?.WikiStats?.zoom ?? 0;
                ScopeIn += item?.WikiStats?.scopeInTime ?? 0;
                Run += item?.WikiStats?.run ?? 0;
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

                AmmoMag = Reciever.WeaponStats.MagSize + Magazine?.WeaponModifiers?.ammo ?? 0; 
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
                allMovementSpread += Muzzle?.WeaponModifiers?.movementSpeed ?? 0;
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
                allAccuracy = Math.Min(Math.Max(allAccuracy,-1.0f),1.0f);
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

            if ( (item1.WeaponModifiers.rating >= item2.WeaponModifiers.rating) && (item1.WeaponModifiers.rating >= item3.WeaponModifiers.rating) )
            {
                if (item1.WeaponModifiers.rating > 0)
                {
                    return item1.DescriptorName;
                }
                return "Basic";
            }
            else if ( (item2.WeaponModifiers.rating >= item1.WeaponModifiers.rating) && (item2.WeaponModifiers.rating >= item3.WeaponModifiers.rating) )
            {
                return item2.DescriptorName;
            }
            else if ( (item3.WeaponModifiers.rating >= item1.WeaponModifiers.rating) && (item3.WeaponModifiers.rating >= item2.WeaponModifiers.rating) )
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

                return new double[] { aim, hip, move };
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

            if ((Reciever.WeaponStats?.TightAimTime ?? 0) > 0) {
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

        private void CheckPrimaryModsForValidity(BLRItem primary)
        {
            CheckValidity(PrimaryBarrelImage, primary);

            CheckValidity(PrimaryMagazineImage, primary);

            CheckValidity(PrimaryMuzzleImage, primary);

            CheckValidity(PrimaryScopeImage, primary);

            CheckValidity(PrimaryStockImage, primary);
        }

        private void CheckSecondaryModsForValidity(BLRItem secondary)
        {
            CheckValidity(SecondaryBarrelImage, secondary);

            CheckValidity(SecondaryMagazineImage, secondary);

            CheckValidity(SecondaryMuzzleImage, secondary);

            CheckValidity(SecondaryScopeImage, secondary);

            CheckValidity(SecondaryStockImage, secondary);

            CheckValidity(SecondaryTagImage, secondary);

            CheckValidity(SecondaryCamoWeaponImage, secondary);
        }

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
                App.RuntimeCheck(BLREditSettings.Settings.ForceRuntimeCheck);
            }
        }

        private void ItemList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("MouseDown in ListView/StackPanel");
            var pt = e.GetPosition(ItemList);
            var result = VisualTreeHelper.HitTest(ItemList, pt);
            if (result.VisualHit is Image image)
            {
                if (image.DataContext is BLRItem item)
                {
                    if (e.ClickCount >= 2)
                    {
                        SetItemToImage(LastSelectedImage, item);
                        if (item.Category == ImportSystem.SECONDARY_CATEGORY)
                        {
                            SetItemToImage(SecondaryScopeImage, (SecondaryScopeImage.DataContext as BLRItem), false);
                        }
                    }
                    else
                    {
                        if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Double Clicking:" + item.Name);
                        DragDrop.DoDragDrop(image, item, DragDropEffects.Copy);
                    }
                }
            }
            else if (result.VisualHit is StackPanel panel)
            {
                if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo(panel.DataContext.ToString());
                foreach (var child in panel.Children)
                {
                    if (child is Image imageChild)
                    {
                        if (imageChild.DataContext is BLRItem item)
                        {
                            if (e.ClickCount >= 2)
                            {
                                SetItemToImage(LastSelectedImage, item);
                                if (item.Category == ImportSystem.SECONDARY_CATEGORY)
                                {
                                    SetItemToImage(SecondaryScopeImage, (SecondaryScopeImage.DataContext as BLRItem), false);
                                }
                            }
                            else
                            {
                                if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Double Clicking2:" + item.Name);
                                DragDrop.DoDragDrop(imageChild, item, DragDropEffects.Copy);
                            }
                        }
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
            if (item == null)
            { image.DataContext = null; return; }
            if (image.Name.Contains("Primary"))
            {
                if (image.Name.Contains("Scope") || image.Name.Contains("Crosshair"))
                {
                    if (item.Category == ImportSystem.SCOPES_CATEGORY)
                    {
                        PrimaryScopeImage.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo(item.Name + " Set!");
                        PrimaryCrosshairImage.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo(item.Name + "Preview Set!");
                    }
                }
            }
            else
            {
                if (image.Name.Contains("Scope") || image.Name.Contains("Crosshair"))
                {
                    if (item.Category == ImportSystem.SCOPES_CATEGORY)
                    {
                        SecondaryScopeImage.DataContext = null;
                        SecondaryCrosshairImage.DataContext = null;
                        SecondaryScopeImage.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo(item.Name + " Set!");
                        SecondaryCrosshairImage.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo(item.Name + "Preview Set!");
                    }
                }
            }
            if (image.Name.Contains("Reciever"))
            {
                if (image.Name.Contains("Primary") && item.Category == ImportSystem.PRIMARY_CATEGORY)
                {
                    image.DataContext = item;
                    if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo(item.Name + " Set!");
                    CheckPrimaryModsForValidity(item);
                    FillEmptyPrimaryMods(item);
                    UpdatePrimaryStats();
                    if (updateLoadout)
                        UpdateActiveLoadout();
                    return;
                }
                if (image.Name.Contains("Secondary") && item.Category == ImportSystem.SECONDARY_CATEGORY)
                {
                    image.DataContext = item;
                    if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo(item.Name + " Set!");
                    CheckSecondaryModsForValidity(item);
                    FillEmptySecondaryMods(item);
                    UpdateSecondaryStats();
                    if (updateLoadout)
                        UpdateActiveLoadout();
                    return;
                }
                if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Not a Valid Primary or Secondary!");
            }
            else
            {
                if (image.Name.Contains("Primary") && !item.IsValidFor(PrimaryRecieverImage.DataContext as BLRItem))
                { if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo(item.Name + " wasn't a Valid Mod for " + (PrimaryRecieverImage.DataContext as BLRItem).Name); return; }

                if (image.Name.Contains("Secondary") && !item.IsValidFor(SecondaryRecieverImage.DataContext as BLRItem))
                { if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo(item.Name + " wasn't a Valid Mod for " + (SecondaryRecieverImage.DataContext as BLRItem).Name); return; }

                if (image.Name.Contains("Muzzle") && item.Category == ImportSystem.MUZZELS_CATEGORY)
                { image.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Muzzle:" + item.Name + " with ID:" + ImportSystem.GetIDOfItem(item) + " Set!"); }
                if (image.Name.Contains("Barrel") && item.Category == ImportSystem.BARRELS_CATEGORY)
                {
                    if (image.Name.Contains("Secondary"))
                    {
                        if (SecondaryRecieverImage.DataContext is BLRItem reciever)
                        {
                            if (CheckForPistolAndBarrel(reciever))
                            {
                                    if (item.Name == MagiCowsWeapon.NoBarrel)
                                    {
                                        SecondaryStockImage.DataContext = MagiCowsWeapon.GetDefaultSetupOfReciever(reciever).GetStock();
                                    }
                            }
                        }
                    }
                    image.DataContext = item;
                }
                if (image.Name.Contains("Magazine") && item.Category == ImportSystem.MAGAZINES_CATEGORY)
                { image.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Magazine:" + item.Name + " with ID:" + ImportSystem.GetIDOfItem(item) + " Set!"); }
                if (image.Name.Contains("Tag") && item.Category == ImportSystem.HANGERS_CATEGORY)
                { image.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Hanger:" + item.Name + " with ID:" + ImportSystem.GetIDOfItem(item) + " Set!"); }
                if (image.Name.Contains("CamoWeapon") && item.Category == ImportSystem.CAMOS_WEAPONS_CATEGORY)
                { image.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Weapon Camo:" + item.Name + " with ID:" + ImportSystem.GetIDOfItem(item) + " Set!"); }
                if (image.Name.Contains("CamoBody") && item.Category == ImportSystem.CAMOS_BODIES_CATEGORY)
                { image.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Body Camo:" + item.Name + " with ID:" + ImportSystem.GetIDOfItem(item) + " Set!"); }
                if (image.Name.Contains("Stock") && item.Category == ImportSystem.STOCKS_CATEGORY)
                {
                    if (image.Name.Contains("Primary"))
                    {
                        SetStock((PrimaryRecieverImage.DataContext as BLRItem), PrimaryBarrelImage, PrimaryStockImage, item);
                    }
                    else
                    {
                        SetStock((SecondaryRecieverImage.DataContext as BLRItem), SecondaryBarrelImage, SecondaryStockImage, item);
                    }
                }

                if (image.Name.Contains("Helmet") && item.Category == ImportSystem.HELMETS_CATEGORY)
                { image.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Helmet:" + item.Name + " with ID:" + ImportSystem.GetIDOfItem(item) + " Set!"); }
                if (image.Name.Contains("UpperBody") && item.Category == ImportSystem.UPPER_BODIES_CATEGORY)
                { image.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("UpperBody:" + item.Name + " with ID:" + ImportSystem.GetIDOfItem(item) + " Set!"); }
                if (image.Name.Contains("LowerBody") && item.Category == ImportSystem.LOWER_BODIES_CATEGORY)
                { image.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("LowerBody:" + item.Name + " with ID:" + ImportSystem.GetIDOfItem(item) + " Set!"); }

                if (image.Name.Contains("Avatar") && item.Category == ImportSystem.AVATARS_CATEGORY)
                { image.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Avatar:" + item.Name + " with ID:" + ImportSystem.GetIDOfItem(item) + " Set!"); }

                if (image.Name.Contains("Trophy") && item.Category == ImportSystem.BADGE_CATEGORY)
                { image.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Trophy:" + item.Name + " with ID:" + ImportSystem.GetIDOfItem(item) + " Set!"); }

                if (image.Name.Contains("Gear") && item.Category == ImportSystem.ATTACHMENTS_CATEGORY && (image.IsEnabled || !updateLoadout))
                { image.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Gear:" + item.Name + " with ID:" + ImportSystem.GetIDOfItem(item) + " Set!"); }
                if (image.Name.Contains("Tactical") && item.Category == ImportSystem.TACTICAL_CATEGORY)
                { image.DataContext = item; if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo("Tactical:" + item.Name + " with ID:" + ImportSystem.GetIDOfItem(item) + " Set!"); }
            }
            UpdateArmorStats();
            if (image.Name.Contains("Primary"))
            {
                UpdatePrimaryStats();
            }
            else
            {
                UpdateSecondaryStats();
            }
            if(updateLoadout)
                UpdateActiveLoadout();
        }

        public void UpdateArmorStats()
        {
            var helmet = (HelmetImage.DataContext as BLRItem);
            var upperBody = (UpperBodyImage.DataContext as BLRItem);
            var lowerBody = (LowerBodyImage.DataContext as BLRItem);
            UpdateHealth(helmet, upperBody, lowerBody);
            UpdateHeadProtection(helmet);
            UpdateRun(helmet, upperBody, lowerBody);
            UpdateHRV(helmet);
            UpdateHRVRecharge(helmet);
            UpdateGearSlots(upperBody, lowerBody);
        }



        public void UpdateHealth(BLRItem helmet, BLRItem upperBody, BLRItem lowerBody)
        {
            double allHealth = (helmet?.PawnModifiers.Health ?? 0);
            allHealth += (upperBody?.PawnModifiers.Health ?? 0);
            allHealth += (lowerBody?.PawnModifiers.Health ?? 0);
            allHealth = Math.Min(Math.Max((int)allHealth, -100), 100);

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
            ArmorHeadProtectionLabel.Content = currentHSProt.ToString("0") + '%';
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
        public void UpdateHRV(BLRItem helmet)
        {
            double currentHealth = (helmet?.PawnModifiers.HRVDuration ?? 0);
            ArmorHRVLabel.Content = currentHealth.ToString("0.0") + 'u';
        }
        public void UpdateHRVRecharge(BLRItem helmet)
        {
            double currentHealth = (helmet?.PawnModifiers.HRVRechargeRate ?? 0);
            ArmorHRVRechargeLabel.Content = currentHealth.ToString("0.0") + "u/s";
        }
        public void UpdateGearSlots(BLRItem upperBody, BLRItem lowerBody)
        { 
            double currentGearSlots = (upperBody?.PawnModifiers?.GearSlots ?? 0) + (lowerBody?.PawnModifiers?.GearSlots ?? 0);
            
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

        private static void SetStock(BLRItem reciever, Image barrel, Image stock, BLRItem item)
        {
            if (reciever != null)
            {
                if (CheckForPistolAndBarrel(reciever))
                {
                    if (barrel.DataContext is BLRItem Barrel)
                    {
                        if (!string.IsNullOrEmpty(Barrel.Name) && Barrel.Name != "No Barrel Mod")
                        {
                            stock.DataContext = item;
                        }
                        else
                        {
                            stock.DataContext = MagiCowsWeapon.GetDefaultSetupOfReciever(reciever).GetStock();
                        }
                    }
                }
                else
                {
                    stock.DataContext = item;
                }
            }
        }

        private void FillEmptyPrimaryMods(BLRItem reciever)
        {
            FillEmptyMods(reciever, PrimaryMuzzleImage, PrimaryBarrelImage, PrimaryMagazineImage, PrimaryScopeImage, PrimaryCrosshairImage, PrimaryStockImage, PrimaryCamoWeaponImage, PrimaryTagImage);
        }

        private void FillEmptySecondaryMods(BLRItem reciever)
        {
            FillEmptyMods(reciever, SecondaryMuzzleImage, SecondaryBarrelImage, SecondaryMagazineImage, SecondaryScopeImage, SecondaryCrosshairImage, SecondaryStockImage, SecondaryCamoWeaponImage, SecondaryTagImage);
        }

        private static void FillEmptyMods(BLRItem reciever, Image muzzle, Image barrel, Image magazine, Image scope, Image crosshair, Image stock, Image camo, Image tag)
        {
            if (reciever.Tooltip != "Depot Item!")
            {
                MagiCowsWeapon weapon = MagiCowsWeapon.GetDefaultSetupOfReciever(reciever);
                if (muzzle.DataContext == null || (muzzle.DataContext as BLRItem).Name == MagiCowsWeapon.NoMuzzle)
                {
                    muzzle.DataContext = weapon.GetMuzzle();
                }
                if (barrel.DataContext == null || (barrel.DataContext as BLRItem).Name == MagiCowsWeapon.NoBarrel)
                {
                    barrel.DataContext = weapon.GetBarrel();
                    if (CheckForPistolAndBarrel(reciever))
                    {
                        stock.DataContext = null;
                    }
                }
                if (stock.DataContext == null || (barrel.DataContext as BLRItem).Name == MagiCowsWeapon.NoStock)
                {
                    SetStock(reciever, barrel, stock, weapon.GetStock());
                }
                if (magazine.DataContext == null)
                {
                    magazine.DataContext = weapon.GetMagazine();
                }
                if (scope.DataContext == null)
                {
                    scope.DataContext = weapon.GetScope();
                    crosshair.DataContext = weapon.GetScope();
                }
                if (camo.DataContext == null)
                {
                    camo.DataContext = weapon.GetCamo();
                }
                if (tag.DataContext == null)
                {
                    var newTag = weapon.GetTag();
                    if (newTag == null)
                    {
                        tag.DataContext = Hangers[rng.Next(0, Hangers.Count)];
                    }
                    else
                    {
                        tag.DataContext = newTag;
                    }
                }
            }
            else
            {
                muzzle.DataContext = null;
                barrel.DataContext=null;
                magazine.DataContext=null;
                scope.DataContext=null;
                stock.DataContext=null;
                crosshair.DataContext=null;
            }
        }

        private void UpdateActiveLoadout()
        {
            UpdateLoadoutWeapon(ActiveLoadout.Primary, PrimaryRecieverImage.DataContext as BLRItem, PrimaryMuzzleImage.DataContext as BLRItem, PrimaryBarrelImage.DataContext as BLRItem, PrimaryMagazineImage.DataContext as BLRItem, PrimaryScopeImage.DataContext as BLRItem, PrimaryStockImage.DataContext as BLRItem, PrimaryTagImage.DataContext as BLRItem, PrimaryCamoWeaponImage.DataContext as BLRItem);
            UpdateLoadoutWeapon(ActiveLoadout.Secondary, SecondaryRecieverImage.DataContext as BLRItem, SecondaryMuzzleImage.DataContext as BLRItem, SecondaryBarrelImage.DataContext as BLRItem, SecondaryMagazineImage.DataContext as BLRItem, SecondaryScopeImage.DataContext as BLRItem, SecondaryStockImage.DataContext as BLRItem, SecondaryTagImage.DataContext as BLRItem, SecondaryCamoWeaponImage.DataContext as BLRItem);
            if (GearImage1.IsEnabled)
            { ActiveLoadout.Gear1 = ImportSystem.GetIDOfItem(GearImage1.DataContext as BLRItem); }
            else
            { ActiveLoadout.Gear1 = 0; }

            if (GearImage2.IsEnabled)
            { ActiveLoadout.Gear2 = ImportSystem.GetIDOfItem(GearImage2.DataContext as BLRItem); }
            else
            { ActiveLoadout.Gear2 = 0; }

            if (GearImage3.IsEnabled)
            { ActiveLoadout.Gear3 = ImportSystem.GetIDOfItem(GearImage3.DataContext as BLRItem); }
            else
            { ActiveLoadout.Gear3 = 0; }

            if (GearImage4.IsEnabled)
            { ActiveLoadout.Gear4 = ImportSystem.GetIDOfItem(GearImage4.DataContext as BLRItem); }
            else
            { ActiveLoadout.Gear4 = 0; }

            ActiveLoadout.Tactical = ImportSystem.GetIDOfItem(TacticalImage.DataContext as BLRItem);
            ActiveLoadout.Helmet = ImportSystem.GetIDOfItem(HelmetImage.DataContext as BLRItem);
            ActiveLoadout.UpperBody = ImportSystem.GetIDOfItem(UpperBodyImage.DataContext as BLRItem);
            ActiveLoadout.LowerBody = ImportSystem.GetIDOfItem(LowerBodyImage.DataContext as BLRItem);
            ActiveLoadout.Camo = ImportSystem.GetIDOfItem((PlayerCamoBodyImage.DataContext as BLRItem));
            ActiveLoadout.Skin = ImportSystem.GetIDOfItem(AvatarImage.DataContext as BLRItem);
            ActiveLoadout.Trophy = ImportSystem.GetIDOfItem(TrophyImage.DataContext as BLRItem);
        }

        private static void UpdateLoadoutWeapon(MagiCowsWeapon weapon, BLRItem reciever, BLRItem muzzle, BLRItem barrel, BLRItem magazine, BLRItem scope, BLRItem stock, BLRItem tag, BLRItem camo)
        {
            weapon.Receiver = reciever?.Name ?? "Assault Rifle";
            weapon.Muzzle = ImportSystem.GetIDOfItem(muzzle);
            weapon.Barrel = barrel?.Name ?? "No Barrel Mod";
            weapon.Magazine = ImportSystem.GetIDOfItem(magazine);
            weapon.Scope = scope?.Name ?? "No Optic Mod";
            weapon.Stock = stock?.Name ?? "No Stock";
            weapon.Tag = ImportSystem.GetIDOfItem(tag);
            weapon.Camo = ImportSystem.GetIDOfItem(camo);
        }

        private void UpdatePrimaryImages(Image image)
        {
            FilterWeapon = PrimaryRecieverImage.DataContext as BLRItem;

            if (image.Name.Contains("Reciever"))
            {
                SetItemList(ImportSystem.PRIMARY_CATEGORY);
                LastSelectedImage = PrimaryRecieverImage;
                return;
            }

            if (image.Name.Contains("Muzzle"))
            {
                if (!(PrimaryRecieverImage.DataContext as BLRItem).IsValidModType("muzzle"))
                { return; }
                SetItemList(ImportSystem.MUZZELS_CATEGORY);
                LastSelectedImage = image;
                return;
            }
            if (image.Name.Contains("Crosshair"))
            {
                var item = (PrimaryScopeImage.DataContext as BLRItem);
                item.LoadCrosshair(true);
                ItemList.ItemsSource = new BLRItem[] { item };
                LastSelectedImage = PrimaryScopeImage;
                return;
            }
            UpdateImages(image);
        }

        private void UpdateSecondaryImages(Image image)
        {
            FilterWeapon = SecondaryRecieverImage.DataContext as BLRItem;
            
            if (image.Name.Contains("Reciever"))
            {
                SetItemList(ImportSystem.SECONDARY_CATEGORY);
                LastSelectedImage = SecondaryRecieverImage;
                return;
            }

            if (image.Name.Contains("Muzzle"))
            {
                if (!(SecondaryRecieverImage.DataContext as BLRItem).IsValidModType("muzzle"))
                { return; }
                SetItemList(ImportSystem.MUZZELS_CATEGORY);
                LastSelectedImage = image;
                return;
            }
            if (image.Name.Contains("Crosshair"))
            {
                var item = (SecondaryScopeImage.DataContext as BLRItem);
                item.LoadCrosshair(false);
                ItemList.ItemsSource = new BLRItem[] { item };
                LastSelectedImage = SecondaryScopeImage;
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
            if (image.Name.Contains("Grip"))
            {
                SetItemList(ImportSystem.GRIPS_CATEGORY);
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
                    case ImportSystem.BADGE_CATEGORY:
                        SetSortingType(typeof(ImportNoStatsSortingType));
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
                    CurrentSortingPropertyName = Enum.GetName(SortComboBox1.SelectedItem.GetType(), SortComboBox1.SelectedItem);
                    view.SortDescriptions.Add(new SortDescription(CurrentSortingPropertyName, SortDirection));
                }
            }
        }

        private void SetSortingType(Type SortingEnumType)
        {
            SortComboBox1.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Source = Enum.GetValues(SortingEnumType) });
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
                        SetItemList(ImportSystem.BADGE_CATEGORY);
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
            //Set Armor first to get gear slot amount for gear
            SetItemToImage(HelmetImage, loadout.GetHelmet(), false);
            SetItemToImage(UpperBodyImage, loadout.GetUpperBody(), false);
            SetItemToImage(LowerBodyImage, loadout.GetLowerBody(), false);

            SetItemToImage(AvatarImage, loadout.GetSkin(), false);

            SetItemToImage(TrophyImage, loadout.GetTrophy(), false);

            SetItemToImage(PlayerCamoBodyImage, loadout.GetCamo(), false);
            
            SetItemToImage(GearImage1, MagiCowsLoadout.GetGear(loadout.Gear1), false);
            SetItemToImage(GearImage2, MagiCowsLoadout.GetGear(loadout.Gear2), false);
            SetItemToImage(GearImage3, MagiCowsLoadout.GetGear(loadout.Gear3), false);
            SetItemToImage(GearImage4, MagiCowsLoadout.GetGear(loadout.Gear4), false);
            SetItemToImage(TacticalImage, loadout.GetTactical(), false);

            IsFemaleCheckBox.DataContext = loadout;

            SetPrimary(loadout.Primary, false);
            SetSecondary(loadout.Secondary, false);

        }

        public void SetPrimary(MagiCowsWeapon primary, bool updateLoadout = true)
        {
            var reciever = primary.GetReciever();
            SetItemToImage(PrimaryRecieverImage, reciever, updateLoadout);
            FilterWeapon = reciever;
            SetItemToImage(PrimaryBarrelImage, primary.GetBarrel(), updateLoadout);
            SetItemToImage(PrimaryMuzzleImage, primary.GetMuzzle(), updateLoadout);
            SetItemToImage(PrimaryStockImage, primary.GetStock(), updateLoadout);
            SetItemToImage(PrimaryMagazineImage, primary.GetMagazine(), updateLoadout);
            SetItemToImage(PrimaryScopeImage, primary.GetScope(), updateLoadout);
            SetItemToImage(PrimaryCrosshairImage, primary.GetScope(), updateLoadout);
            SetItemToImage(PrimaryTagImage, primary.GetTag(), updateLoadout);
            SetItemToImage(PrimaryCamoWeaponImage, primary.GetCamo(), updateLoadout);
            UpdatePrimaryStats();
        }

        public void SetSecondary(MagiCowsWeapon secondary, bool updateLoadout = true)
        {
            SetItemToImage(SecondaryRecieverImage, secondary.GetReciever(), updateLoadout);
            SetItemToImage(SecondaryBarrelImage, secondary.GetBarrel(), updateLoadout);
            SetItemToImage(SecondaryMuzzleImage, secondary.GetMuzzle(), updateLoadout);
            SetItemToImage(SecondaryStockImage, secondary.GetStock(), updateLoadout);
            SetItemToImage(SecondaryMagazineImage, secondary.GetMagazine(), updateLoadout);
            SetItemToImage(SecondaryScopeImage, secondary.GetScope(), updateLoadout);
            SetItemToImage(SecondaryCrosshairImage, secondary.GetScope(), updateLoadout);
            SetItemToImage(SecondaryTagImage, secondary.GetTag(), updateLoadout);
            SetItemToImage(SecondaryCamoWeaponImage, secondary.GetCamo(), updateLoadout);
            UpdateSecondaryStats();
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
                    IsFemaleCheckBox.DataContext = profile;
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
            //Update upper body armor image
            if (UpperBodyImage.DataContext is BLRItem upper)
            {
                UpperBodyImage.DataContext = null;
                UpperBodyImage.DataContext = upper;
            }
            //update lower body armor image
            if (LowerBodyImage.DataContext is BLRItem lower)
            {
                LowerBodyImage.DataContext = null;
                LowerBodyImage.DataContext = lower;
            }
            
            //reset item list
            var source = ItemList.ItemsSource;
            ItemList.ItemsSource = null;
            ItemList.ItemsSource = source;
        }

        private void IsFemaleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //Update upper body armor image
            if (UpperBodyImage.DataContext is BLRItem upper)
            {
                UpperBodyImage.DataContext = null;
                UpperBodyImage.DataContext = upper;
            }
            //update lower body armor image
            if (LowerBodyImage.DataContext is BLRItem lower)
            {
                LowerBodyImage.DataContext = null;
                LowerBodyImage.DataContext = lower;
            }

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


        static readonly List<BLRItem> Tacticals = ImportSystem.GetItemListOfType(ImportSystem.TACTICAL_CATEGORY);
        static readonly List<BLRItem> Attachments = ImportSystem.GetItemListOfType(ImportSystem.ATTACHMENTS_CATEGORY);
        static readonly List<BLRItem> CamosBody = ImportSystem.GetItemListOfType(ImportSystem.CAMOS_BODIES_CATEGORY);
        static readonly List<BLRItem> Helmets = ImportSystem.GetItemListOfType(ImportSystem.HELMETS_CATEGORY);
        static readonly List<BLRItem> UpperBodies = ImportSystem.GetItemListOfType(ImportSystem.UPPER_BODIES_CATEGORY);
        static readonly List<BLRItem> LowerBodies = ImportSystem.GetItemListOfType(ImportSystem.LOWER_BODIES_CATEGORY);
        static readonly List<BLRItem> Avatars = ImportSystem.GetItemListOfType(ImportSystem.AVATARS_CATEGORY);
        private static MagiCowsLoadout RandomizeLoadout()
        {
            MagiCowsLoadout loadout = new()
            {
                Primary = MagiCowsWeapon.GetDefaultSetupOfReciever(Primaries[rng.Next(0, Primaries.Count)])
            };

            loadout.Secondary.Stock = null;
            loadout.Secondary.Barrel = null;
            loadout.Secondary.Scope = null;
            loadout.Secondary.Muzzle = 0;
            loadout.Secondary.Magazine = 0;

            BLRItem secon = Secondaries[rng.Next(0, Secondaries.Count)];

            if (MagiCowsWeapon.GetDefaultSetupOfReciever(secon) != null)
            {
                loadout.Secondary = MagiCowsWeapon.GetDefaultSetupOfReciever(secon);
            }
            else
            {
                loadout.Secondary.Receiver = secon.Name;
            }




            loadout.Tactical = rng.Next(0, Tacticals.Count);
            
            loadout.Gear1 = rng.Next(0, Attachments.Count);
            loadout.Gear2 = rng.Next(0, Attachments.Count);
            loadout.Gear3 = rng.Next(0, Attachments.Count);
            loadout.Gear4 = rng.Next(0, Attachments.Count);

            loadout.Camo = rng.Next(0, CamosBody.Count);

            loadout.Helmet = rng.Next(0, Helmets.Count);
            loadout.UpperBody = rng.Next(0, UpperBodies.Count);
            loadout.LowerBody = rng.Next(0, LowerBodies.Count);
            loadout.Skin = rng.Next(0, Avatars.Count);


            int i = rng.Next(0, 2);
            if (i > 0)
            {
                loadout.IsFemale = false;
            }
            else
            {
                loadout.IsFemale = true;
            }

            loadout.Primary = RandomizeWeapon(loadout.Primary.GetReciever());
            if (loadout.Secondary.Magazine != 0)
            {
                loadout.Secondary = RandomizeWeapon(loadout.Secondary.GetReciever(), true);
            }
            return loadout;
        }

        private static MagiCowsWeapon RandomizeWeapon(BLRItem Weapon, bool IsSecondary = false)
        {
            MagiCowsWeapon weapon = MagiCowsWeapon.GetDefaultSetupOfReciever(Weapon);
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

            if (FilteredCamos.Length > 0)
            {
                weapon.Camo = ImportSystem.GetIDOfItem(FilteredCamos[rng.Next(0, FilteredCamos.Length)]);
            }

            weapon.IsHealthOkAndRepair();

            return weapon;
        }
    }
}
