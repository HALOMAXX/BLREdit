using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace BLREdit.UI
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool IsUnitTest = false;
        public static Image LastSelectedImage { get; private set; } = null;
        private ImportItem FilterWeapon = null;
        public static MagiCowsLoadout ActiveLoadout { get; set; } = null;
        public static MainWindow self { get; private set; } = null;



        public MainWindow()
        {
            profilechanging = true;
            textchnaging = true;

            InitializeComponent();
            self = this;
            ItemList.Items.Filter += new Predicate<object>(o =>
            {
                if (o != null)
                {
                    return ((ImportItem)o).IsValidFor(FilterWeapon);
                }
                else
                {
                    return false;
                }
            });

            //SortComboBox1.ItemsSource = Enum.GetValues(typeof(ImportItemSortingType));

            PlayerNameTextBox.Text = ExportSystem.ActiveProfile.PlayerName;
            ProfileComboBox.ItemsSource = ExportSystem.Profiles;
            ProfileComboBox.SelectedIndex = 0;

            SetLoadout(ExportSystem.ActiveProfile.Loadout1);

            Loadout1Button.IsEnabled = false;

            profilechanging = false;
            textchnaging = false;

            SetItemList(ImportSystem.Weapons.primary);
            LastSelectedImage = PrimaryRecieverImage;
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

        private static bool CheckCalculationReady(ImportItem item)
        {
            return item != null && item.IniStats != null && item.stats != null;
        }

        private static void UpdateStat(ImportItem[] items, ref double ROF, ref double Reload, ref double Swap, ref double Zoom, ref double ScopeIn, ref double Run)
        {
            foreach (ImportItem item in items)
            {
                ROF += item?.IniStats?.ROF ?? 0;
                Reload += item?.WikiStats?.reload ?? 0;
                Swap += item?.WikiStats?.swaprate ?? 0;
                Zoom += item?.WikiStats?.zoom ?? 0;
                ScopeIn += item?.WikiStats?.scopeInTime ?? 0;
                Run += item?.WikiStats?.run ?? 0;
            }
        }

        private static void UpdateStats(ImportItem Reciever, ImportItem Barrel, ImportItem Magazine, ImportItem Muzzle, ImportItem Scope, ImportItem Stock, Label DamageLabel, Label ROFLabel, Label AmmoLabel, Label ReloadLabel, Label SwapLabel, Label AimLabel, Label HipLabel, Label MoveLabel, Label RecoilLabel, Label ZoomRecoilLabel, Label ZoomLabel, Label ScopeInLabel, Label RangeLabel, Label RunLabel, Label Descriptor)
        {
            var watch = LoggingSystem.LogInfo("Updating Stats", "");

            double Damage = 0, DamageFar = 0, ROF = 0, AmmoMag = 0, AmmoRes = 0, Reload = 0, Swap = 0, Aim = 0, Hip = 0, Move = 0, Recoil = 0, RecoilZoom = 0, Zoom = 0, ScopeIn = 0, RangeClose = 0, RangeFar = 0, RangeMax = 0, Run = 0, MoveSpeed = 0, CockRateMultiplier = 0, ReloadRateMultiplier = 0;

            string barrelVSmag = "", stockVSmuzzle = "", weaponDescriptor = "";

            if (CheckCalculationReady(Reciever))
            {
                List<ImportItem> items = new List<ImportItem>();
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
                if (Stock != null)
                    items.Add(Stock);

                UpdateStat(items.ToArray(), ref ROF, ref Reload, ref Swap, ref Zoom, ref ScopeIn, ref Run);

                AmmoMag = Reciever.IniStats.MagSize + Magazine?.weaponModifiers?.ammo ?? 0; 
                AmmoRes = AmmoMag * Reciever.IniStats.InitialMagazines;

                double allRecoil = Barrel?.weaponModifiers?.recoil ?? 0;
                allRecoil += Muzzle?.weaponModifiers?.recoil ?? 0;
                allRecoil += Stock?.weaponModifiers?.recoil ?? 0;
                allRecoil += Scope?.weaponModifiers?.recoil ?? 0;
                allRecoil += Magazine?.weaponModifiers?.recoil ?? 0;
                allRecoil /= 100.0f;
                allRecoil = Math.Min(Math.Max(allRecoil, -1.0f), 1.0f);
                Recoil = CalculateRecoil(Reciever, allRecoil);
                RecoilZoom = Recoil * Reciever.IniStats.RecoilZoomMultiplier * 0.8;


                double allDamage = Barrel?.weaponModifiers?.damage ?? 0;
                allDamage += Muzzle?.weaponModifiers?.damage ?? 0;
                allDamage += Stock?.weaponModifiers?.damage ?? 0;
                allDamage += Scope?.weaponModifiers?.damage ?? 0;
                allDamage += Magazine?.weaponModifiers?.damage ?? 0;
                allDamage /= 100.0f;
                allDamage = Math.Min(Math.Max(allDamage, -1.0f), 1.0f);
                var damages = CalculateDamage(Reciever, allDamage);
                Damage = damages[0];
                DamageFar = damages[1];

                double allRange = Barrel?.weaponModifiers?.range ?? 0;
                allRange += Muzzle?.weaponModifiers?.range ?? 0;
                allRange += Stock?.weaponModifiers?.range ?? 0;
                allRange += Scope?.weaponModifiers?.range ?? 0;
                allRange += Magazine?.weaponModifiers?.range ?? 0;
                allRange /= 100.0f;
                allRange = Math.Min(Math.Max(allRange, -1.0f), 1.0f);
                var ranges = CalculateRange(Reciever, allRange);
                RangeClose = ranges[0];
                RangeFar = ranges[1];
                RangeMax = ranges[2];

                double allMovementSpread = Barrel?.weaponModifiers?.movementSpeed ?? 0;  // For specifically my move spread change, hence no mag/scope added.
                allMovementSpread += Muzzle?.weaponModifiers?.movementSpeed ?? 0;
                allMovementSpread += Stock?.weaponModifiers?.movementSpeed ?? 0;
                allMovementSpread /= 100.0f;
                allMovementSpread = Math.Min(Math.Max(allMovementSpread, -1.0f), 1.0f);

                double allAccuracy = Barrel?.weaponModifiers?.accuracy ?? 0;
                allAccuracy += Muzzle?.weaponModifiers?.accuracy ?? 0;
                allAccuracy += Stock?.weaponModifiers?.accuracy ?? 0;
                allAccuracy += Scope?.weaponModifiers?.accuracy ?? 0;
                allAccuracy += Magazine?.weaponModifiers?.accuracy ?? 0;
                allAccuracy /= 100.0f;
                allAccuracy = Math.Min(Math.Max(allAccuracy,-1.0f),1.0f);
                var spreads = CalculateSpread(Reciever, allAccuracy, allMovementSpread);
                Aim = spreads[0];
                Hip = spreads[1];
                Move = spreads[2];

                double allMovementSpeed = Barrel?.weaponModifiers?.movementSpeed ?? 0;
                allMovementSpeed += Muzzle?.weaponModifiers?.movementSpeed ?? 0;
                allMovementSpeed += Stock?.weaponModifiers?.movementSpeed ?? 0;
                allMovementSpeed += Scope?.weaponModifiers?.movementSpeed ?? 0;
                allMovementSpeed += Magazine?.weaponModifiers?.movementSpeed ?? 0;
                allMovementSpeed /= 100.0f;
                allMovementSpeed = Math.Min(Math.Max(allMovementSpeed, -1.0f), 1.0f);
                MoveSpeed = CalculateSpeed(Reciever, allMovementSpeed);

                double allReloadSpeed = Magazine?.weaponModifiers?.reloadSpeed ?? 0;
                allReloadSpeed /= 100.0f;
                allReloadSpeed = Math.Min(Math.Max(allReloadSpeed, -1.0f), 1.0f);

                CockRateMultiplier = CalculateCockRate(Reciever, allRecoil);
                ReloadRateMultiplier = CalculateReloadRate(Reciever, allRecoil, allReloadSpeed);

                List<ImportItem> mods = new List<ImportItem>();
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
            ScopeInLabel.Content = ScopeIn.ToString("0.000") + "s";
            RangeLabel.Content = RangeClose.ToString("0.0") + " / " + RangeFar.ToString("0.0") + " / " + RangeMax.ToString("0");
            //RunLabel.Content = Run.ToString("0.00");
            RunLabel.Content = MoveSpeed.ToString("0.00");
            Descriptor.Content = barrelVSmag + " " + stockVSmuzzle + " " + weaponDescriptor;
            LoggingSystem.LogInfoAppend(watch);
        }

        private static double TotalPoints(IEnumerable<ImportItem> items)
        {
            double points = 0;
            foreach (ImportItem item in items)
            {
                points += item.weaponModifiers.rating;
            }
            return points;
        }

        private static string CompareItemDescriptor1(ImportItem item1, ImportItem item2)
        {
            if (item1 == null && item2 != null)
            {
                return item2.descriptorName;
            }
            else if (item1 != null && item2 == null)
            {
                return item1.descriptorName;
            }
            else if (item1 == null && item2 == null)
            {
                return "Standard";
            }

            if (item1.weaponModifiers.rating > item2.weaponModifiers.rating)
            {
                return item1.descriptorName;
            }
            else
            {
                return item2.descriptorName;
            }
        }

        private static string CompareItemDescriptor2(ImportItem item1, ImportItem item2, ImportItem item3)
        {
            if (item1 == null && item2 == null && item3 == null)
            {
                return "Basic";
            }

            if (item1 == null && item2 != null)
            {
                return item2.descriptorName;
            }
            else if (item1 != null && item2 == null)
            {
                return item1.descriptorName;
            }
            else if (item1 == null && item2 == null && item3 != null)
            {
                return item3.descriptorName;
            }

            if ( (item1.weaponModifiers.rating >= item2.weaponModifiers.rating) && (item1.weaponModifiers.rating >= item3.weaponModifiers.rating) )
            {
                if (item1.weaponModifiers.rating > 0)
                {
                    return item1.descriptorName;
                }
                return "Basic";
            }
            else if ( (item2.weaponModifiers.rating >= item1.weaponModifiers.rating) && (item2.weaponModifiers.rating >= item3.weaponModifiers.rating) )
            {
                return item2.descriptorName;
            }
            else if ( (item3.weaponModifiers.rating >= item1.weaponModifiers.rating) && (item3.weaponModifiers.rating >= item2.weaponModifiers.rating) )
            {
                return item3.descriptorName;
            }

            return item1.descriptorName;
        }

        public static double[] CalculateRange(ImportItem Reciever, double allRange)
        {
            if (Reciever != null && Reciever.IniStats != null)
            {
                double idealRange;
                double maxRange;
                double alpha = Math.Abs(allRange);
                if (allRange > 0)
                {
                    idealRange = (int)Lerp(Reciever?.IniStats?.ModificationRangeIdealDistance.Z ?? 0, Reciever?.IniStats?.ModificationRangeIdealDistance.Y ?? 0, alpha);
                    maxRange = Lerp(Reciever?.IniStats?.ModificationRangeMaxDistance.Z ?? 0, Reciever?.IniStats?.ModificationRangeMaxDistance.Y ?? 0, alpha);
                }
                else
                {
                    idealRange = (int)Lerp(Reciever?.IniStats?.ModificationRangeIdealDistance.Z ?? 0, Reciever?.IniStats?.ModificationRangeIdealDistance.X ?? 0, alpha);
                    maxRange = Lerp(Reciever?.IniStats?.ModificationRangeMaxDistance.Z ?? 0, Reciever?.IniStats?.ModificationRangeMaxDistance.X ?? 0, alpha);
                }

                return new double[] { idealRange / 100, maxRange / 100, (Reciever?.IniStats?.MaxTraceDistance ?? 0) / 100 };
            }
            else
            {
                return new double[] { 0, 0, 0 };
            }
        }

        public static double[] CalculateSpread(ImportItem Reciever, double allAccuracy, double allMovementSpread)
        {
            if (Reciever != null && Reciever.IniStats != null)
            {
                double accuracyBaseModifier;
                double accuracyTABaseModifier;
                double alpha = Math.Abs(allAccuracy);
                if (allAccuracy > 0)
                {
                    accuracyBaseModifier = Lerp(Reciever?.IniStats?.ModificationRangeBaseSpread.Z ?? 0, Reciever?.IniStats?.ModificationRangeBaseSpread.Y ?? 0, alpha);
                    accuracyTABaseModifier = Lerp(Reciever?.IniStats?.ModificationRangeTABaseSpread.Z ?? 0, Reciever?.IniStats?.ModificationRangeTABaseSpread.Y ?? 0, alpha);
                }
                else
                {
                    accuracyBaseModifier = Lerp(Reciever?.IniStats?.ModificationRangeBaseSpread.Z ?? 0, Reciever?.IniStats?.ModificationRangeBaseSpread.X ?? 0, alpha);
                    accuracyTABaseModifier = Lerp(Reciever?.IniStats?.ModificationRangeTABaseSpread.Z ?? 0, Reciever?.IniStats?.ModificationRangeTABaseSpread.X ?? 0, alpha);
                }

                double hip = accuracyBaseModifier * (180 / Math.PI);
                double aim = (accuracyBaseModifier * Reciever.IniStats.ZoomSpreadMultiplier) * (180 / Math.PI);
                if (Reciever.IniStats.UseTABaseSpread)
                {
                    aim = accuracyTABaseModifier * (float)(180 / Math.PI);
                }

                double weight_alpha = Math.Abs(Reciever.IniStats.Weight / 80.0);
                double weight_clampalpha = Math.Min(Math.Max(weight_alpha, -1.0), 1.0); // Don't ask me why they clamp the absolute value with a negative, I have no idea.
                double weight_multiplier;
                if (Reciever.IniStats.Weight > 0)   // It was originally supposed to compare the total weight of equipped mods, but from what I can currently gather from the scripts, nothing modifies weapon weight so I'm just comparing base weight for now.
                {
                    weight_multiplier = Lerp(1.0, 0.5, weight_clampalpha);  // Originally supposed to be a weapon specific range, but they all set the same values so it's not worth setting elsewhere.
                }
                else
                {
                    weight_multiplier = Lerp(1.0, 2.0, weight_clampalpha);
                }

                double move_alpha = Math.Abs(allMovementSpread);
                double move_multiplier; // Applying movement to it like this isn't how it's done to my current knowledge, but seems to be consistently closer to how it should be in most cases so far.
                if (allMovementSpread > 0)
                {
                    move_multiplier = Lerp(1.0, 0.5, move_alpha);
                }
                else
                {
                    move_multiplier = Lerp(1.0, 2.0, move_alpha);
                }

                double movemultiplier_current = 1.0 + ((Reciever.IniStats.MovementSpreadMultiplier - 1.0) * (weight_multiplier * move_multiplier));
                double moveconstant_current = Reciever.IniStats.MovementSpreadConstant * (weight_multiplier * move_multiplier);

                double move = ((accuracyBaseModifier + moveconstant_current) * (180 / Math.PI)) * movemultiplier_current;
                //double move = (accuracyBaseModifier * Reciever.IniStats.MovementSpreadMultiplier) * (180 / Math.PI);  // Old.

                return new double[] { aim, hip, move };
            }
            else
            {
                return new double[] { 0, 0, 0 };
            }
        }

        public static double CalculateSpeed(ImportItem Reciever, double allMovementSpeed)
        {
            if (Reciever != null && Reciever.IniStats != null)
            {
                double move_alpha = Math.Abs(allMovementSpeed);
                double move_modifier;
                if (allMovementSpeed > 0)
                {
                    move_modifier = Lerp(Reciever.IniStats.ModificationRangeMoveSpeed.Z, Reciever.IniStats.ModificationRangeMoveSpeed.Y, move_alpha);
                }
                else
                {
                    move_modifier = Lerp(Reciever.IniStats.ModificationRangeMoveSpeed.Z, Reciever.IniStats.ModificationRangeMoveSpeed.X, move_alpha);
                }
                double speed = (765 + move_modifier) / 100.0f;
                return speed;
            }
            return 0;
        }

        public static double[] CalculateDamage(ImportItem Reciever, double allDamage)
        {
            if (Reciever != null && Reciever.IniStats != null)
            {
                double damageModifier;
                double alpha = Math.Abs(allDamage);
                if (allDamage > 0)
                {
                    damageModifier = Lerp(Reciever?.IniStats?.ModificationRangeDamage.Z ?? 0, Reciever?.IniStats?.ModificationRangeDamage.Y ?? 0, alpha);
                }
                else
                {
                    damageModifier = Lerp(Reciever?.IniStats?.ModificationRangeDamage.Z ?? 0, Reciever?.IniStats?.ModificationRangeDamage.X ?? 0, alpha);
                }

                return new double[] { damageModifier, damageModifier * (Reciever?.IniStats?.MaxRangeDamageMultiplier ?? 0.1d) };
            }
            else
            {
                return new double[] { 0, 0 };
            }
        }

        public static double CalculateRecoil(ImportItem Reciever, double allRecoil)
        {
            if (Reciever != null && Reciever.IniStats != null)
            {
                double recoilModifier;
                double alpha = Math.Abs(allRecoil);
                if (allRecoil > 0)
                {
                    recoilModifier = Lerp(Reciever?.IniStats?.ModificationRangeRecoil.Z ?? 0, Reciever?.IniStats?.ModificationRangeRecoil.Y ?? 0, alpha);
                }
                else
                {
                    recoilModifier = Lerp(Reciever?.IniStats?.ModificationRangeRecoil.Z ?? 0, Reciever?.IniStats?.ModificationRangeRecoil.X ?? 0, alpha);
                }
                if (Reciever?.IniStats.MagSize > 0)
                {
                    double averageShotCount = Math.Min(Reciever?.IniStats.MagSize ?? 0, 15.0f);
                    Vector3 averageRecoil = new Vector3(0, 0, 0);

                    for (int shot = 1; shot <= averageShotCount; shot++)
                    {
                        Vector3 newRecoil = new Vector3(0, 0, 0)
                        {
                            X = (Reciever.IniStats.RecoilVector.X * Reciever.IniStats.RecoilVectorMultiplier.X) / 4.0f,
                            Y = (Reciever.IniStats.RecoilVector.Y * Reciever.IniStats.RecoilVectorMultiplier.Y) / 2.0f
                        };

                        double previousMultiplier = Reciever.IniStats.RecoilSize * Math.Pow(shot / Reciever.IniStats.Burst, (Reciever.IniStats.RecoilAccumulation * Reciever.IniStats.RecoilAccumulationMultiplier));
                        double currentMultiplier = Reciever.IniStats.RecoilSize * Math.Pow(shot / Reciever.IniStats.Burst + 1.0f, (Reciever.IniStats.RecoilAccumulation * Reciever.IniStats.RecoilAccumulationMultiplier));
                        double multiplier = currentMultiplier - previousMultiplier;
                        newRecoil *= (float)multiplier;
                        averageRecoil += newRecoil;
                    }

                    if (averageShotCount > 0)
                    {
                        averageRecoil /= (float)averageShotCount;
                    }
                    if (Reciever.IniStats.ROF > 0 && Reciever.IniStats.ApplyTime > 60 / Reciever.IniStats.ROF)
                    {
                        averageRecoil *= (float)(60 / (Reciever.IniStats.ROF * Reciever.IniStats.ApplyTime));
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

        public static double CalculateCockRate(ImportItem Reciever, double allRecoil)
        {
            double alpha = Math.Abs(allRecoil);
            double cockrate;
            if (Reciever.IniStats.ModificationRangeCockRate.Z != 0)
            {
                if (allRecoil > 0)
                {
                    cockrate = Lerp(Reciever.IniStats.ModificationRangeCockRate.Z, Reciever.IniStats.ModificationRangeCockRate.Y, alpha);
                }
                else
                {
                    cockrate = Lerp(Reciever.IniStats.ModificationRangeCockRate.Z, Reciever.IniStats.ModificationRangeCockRate.X, alpha);
                }
                if (cockrate > 0)
                {
                    cockrate = 1.0 / cockrate;
                }
                return cockrate;
            }
            return 1.0;
        }

        public static double CalculateReloadRate(ImportItem Reciever, double allRecoil, double allReloadSpeed)
        {
            double WeaponReloadRate = 1.0;
            double rate_alpha;

            if (Reciever.IniStats.ModificationRangeReloadRate.Z > 0)
            {
                rate_alpha = Math.Abs(allReloadSpeed);
                if (allReloadSpeed > 0)
                {
                    WeaponReloadRate = Lerp(Reciever.IniStats.ModificationRangeReloadRate.Z, Reciever.IniStats.ModificationRangeReloadRate.X, rate_alpha);
                }
                else
                {
                    WeaponReloadRate = Lerp(Reciever.IniStats.ModificationRangeReloadRate.Z, Reciever.IniStats.ModificationRangeReloadRate.Y, rate_alpha);
                }
            }
            
            if (Reciever.IniStats.ModificationRangeRecoilReloadRate.Z == 1)
            {
                rate_alpha = Math.Abs(allRecoil);
                if (allRecoil > 0)
                {
                    WeaponReloadRate = Lerp(Reciever.IniStats.ModificationRangeRecoilReloadRate.Z, Reciever.IniStats.ModificationRangeRecoilReloadRate.X, rate_alpha);
                }
                else
                {
                    WeaponReloadRate = Lerp(Reciever.IniStats.ModificationRangeRecoilReloadRate.Z, Reciever.IniStats.ModificationRangeRecoilReloadRate.Y, rate_alpha);
                }
            }
            return WeaponReloadRate;
        }

        private void CheckPrimaryModsForValidity(ImportItem primary)
        {
            CheckValidity(PrimaryBarrelImage, primary);

            CheckValidity(PrimaryMagazineImage, primary);

            CheckValidity(PrimaryMuzzleImage, primary);

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

        private static void CheckValidity(Image image, ImportItem item)
        {
            if(image.DataContext is ImportItem importItem)
            {
                if (!importItem.IsValidFor(item) || !item.IsValidModType(importItem.Category))
                { LoggingSystem.LogInfo(importItem.name + " was not a Valid Mod for " + item.name); image.DataContext = null; }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsUnitTest) { this.Close(); }
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
            LoggingSystem.LogInfo("MouseDown in ListView/StackPanel");
            var pt = e.GetPosition(ItemList);
            var result = VisualTreeHelper.HitTest(ItemList, pt);
            if (result.VisualHit is Image image)
            {
                if (image.DataContext is ImportItem item)
                {
                    if (e.ClickCount >= 2)
                    {
                        SetItemToImage(LastSelectedImage, item);
                    }
                    else
                    {
                        LoggingSystem.LogInfo("Sending:" + item.name);
                        DragDrop.DoDragDrop(image, item, DragDropEffects.Copy);
                    }
                }
            }
            else if (result.VisualHit is StackPanel panel)
            {
                LoggingSystem.LogInfo(panel.DataContext.ToString());
                foreach (var child in panel.Children)
                {
                    if (child is Image imageChild)
                    {
                        if (imageChild.DataContext is ImportItem item)
                        {
                            if (e.ClickCount >= 2)
                            {
                                SetItemToImage(LastSelectedImage, item);
                            }
                            else
                            {
                                LoggingSystem.LogInfo("Sending:" + item.name);
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
                if (e.Data.GetData(typeof(ImportItem)) is ImportItem item)
                {
                    LoggingSystem.LogInfo("Recieving:" + item.name);
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

        public void SetItemToImage(Image image, ImportItem item, bool updateLoadout = true)
        {
            if (item == null)
            { image.DataContext = null; return; }
            if (image.Name.Contains("Primary"))
            {
                if (image.Name.Contains("Scope") || image.Name.Contains("Crosshair"))
                {
                    if (ImportSystem.Mods.scopes.Contains(item))
                    {
                        PrimaryScopeImage.DataContext = item; LoggingSystem.LogInfo(item.name + " Set!");
                        PrimaryCrosshairImage.DataContext = item; LoggingSystem.LogInfo(item.name + "Preview Set!");
                    }
                }
            }
            else
            {
                if (image.Name.Contains("Scope") || image.Name.Contains("Crosshair"))
                {
                    if (ImportSystem.Mods.scopes.Contains(item))
                    {
                        SecondaryScopeImage.DataContext = item; LoggingSystem.LogInfo(item.name + " Set!");
                        SecondaryCrosshairImage.DataContext = item; LoggingSystem.LogInfo(item.name + "Preview Set!");
                    }
                }
            }
            if (image.Name.Contains("Reciever"))
            {
                if (image.Name.Contains("Primary") && ImportSystem.Weapons.primary.Contains(item))
                {
                    image.DataContext = item;
                    LoggingSystem.LogInfo(item.name + " Set!");
                    CheckPrimaryModsForValidity(item);
                    FillEmptyPrimaryMods(item);
                    UpdatePrimaryStats();
                    if(updateLoadout)
                        UpdateActiveLoadout();
                    return;
                }
                if (image.Name.Contains("Secondary") && ImportSystem.Weapons.secondary.Contains(item))
                {
                    image.DataContext = item;
                    LoggingSystem.LogInfo(item.name + " Set!");
                    CheckSecondaryModsForValidity(item);
                    FillEmptySecondaryMods(item);
                    UpdateSecondaryStats();
                    if (updateLoadout)
                        UpdateActiveLoadout();
                    return;
                }
                LoggingSystem.LogInfo("Not a Valid Primary or Secondary!");
            }
            else
            {
                if (image.Name.Contains("Primary") && !item.IsValidFor(PrimaryRecieverImage.DataContext as ImportItem))
                { LoggingSystem.LogInfo(item.name + " wasn't a Valid Mod for " + (PrimaryRecieverImage.DataContext as ImportItem).name); return; }

                if (image.Name.Contains("Secondary") && !item.IsValidFor(SecondaryRecieverImage.DataContext as ImportItem))
                { LoggingSystem.LogInfo(item.name + " wasn't a Valid Mod for " + (SecondaryRecieverImage.DataContext as ImportItem).name); return; }

                if (image.Name.Contains("Muzzle") && ImportSystem.Mods.muzzles.Contains(item))
                { image.DataContext = item; LoggingSystem.LogInfo("Muzzle:" + item.name + " with ID:" + ImportSystem.GetMuzzleID(item) + " Set!"); }
                if (image.Name.Contains("Barrel") && ImportSystem.Mods.barrels.Contains(item))
                {
                    if (image.Name.Contains("Secondary"))
                    {
                        if (SecondaryRecieverImage.DataContext is ImportItem reciever)
                        {
                            if (CheckForPistolAndBarrel(reciever))
                            {
                                    if (item.name == MagiCowsWeapon.NoBarrel)
                                    {
                                        SecondaryStockImage.DataContext = MagiCowsWeapon.GetDefaultSetupOfReciever(reciever).GetStock();
                                    }
                            }
                        }
                    }
                    image.DataContext = item;
                }
                if (image.Name.Contains("Magazine") && ImportSystem.Mods.magazines.Contains(item))
                { image.DataContext = item; LoggingSystem.LogInfo("Magazine:" + item.name + " with ID:" + ImportSystem.GetMagazineID(item) + " Set!"); }
                if (image.Name.Contains("Tag") && ImportSystem.Gear.hangers.Contains(item))
                { image.DataContext = item; LoggingSystem.LogInfo("Hanger:" + item.name + " with ID:" + ImportSystem.GetTagID(item) + " Set!"); }
                if (image.Name.Contains("CamoWeapon") && ImportSystem.Mods.camosBody.Contains(item))
                { image.DataContext = item; LoggingSystem.LogInfo("Camo:" + item.name + " with ID:" + ImportSystem.GetCamoBodyID(item) + " Set!"); }
                if (image.Name.Contains("CamoBody") && ImportSystem.Mods.camosBody.Contains(item))
                { image.DataContext = item; LoggingSystem.LogInfo("Camo:" + item.name + " with ID:" + ImportSystem.GetCamoBodyID(item) + " Set!"); }
                if (image.Name.Contains("Stock") && ImportSystem.Mods.stocks.Contains(item))
                {
                    if (image.Name.Contains("Primary"))
                    {
                        SetStock((PrimaryRecieverImage.DataContext as ImportItem), PrimaryBarrelImage, PrimaryStockImage, item);
                    }
                    else
                    {
                        SetStock((SecondaryRecieverImage.DataContext as ImportItem), SecondaryBarrelImage, SecondaryStockImage, item);
                    }
                }

                if (image.Name.Contains("Helmet") && ImportSystem.Gear.helmets.Contains(item))
                { image.DataContext = item;  }
                if (image.Name.Contains("UpperBody") && ImportSystem.Gear.upperBodies.Contains(item))
                { image.DataContext = item; }
                if (image.Name.Contains("LowerBody") && ImportSystem.Gear.lowerBodies.Contains(item))
                { image.DataContext = item; }
                if (image.Name.Contains("Gear") && ImportSystem.Gear.attachments.Contains(item) && (image.IsEnabled || !updateLoadout))
                { image.DataContext = item; }
                if (image.Name.Contains("Tactical") && ImportSystem.Gear.tactical.Contains(item))
                { image.DataContext = item; }
                LoggingSystem.LogInfo(item.name + " Set!");
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
            var helmet = (HelmetImage.DataContext as ImportItem);
            var upperBody = (UpperBodyImage.DataContext as ImportItem);
            var lowerBody = (LowerBodyImage.DataContext as ImportItem);
            UpdateHealth(helmet, upperBody, lowerBody);
            UpdateHeadProtection(helmet);
            UpdateRun(helmet, upperBody, lowerBody);
            UpdateHRV(helmet);
            UpdateHRVRecharge(helmet);
            UpdateGearSlots(upperBody, lowerBody);
        }



        public void UpdateHealth(ImportItem helmet, ImportItem upperBody, ImportItem lowerBody)
        {
            double allHealth = (helmet?.pawnModifiers.Health ?? 0);
            allHealth += (upperBody?.pawnModifiers.Health ?? 0);
            allHealth += (lowerBody?.pawnModifiers.Health ?? 0);
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
        public void UpdateHeadProtection(ImportItem helmet)
        {
            double currentHSProt = (helmet?.pawnModifiers.HelmetDamageReduction ?? 0);
            ArmorHeadProtectionLabel.Content = currentHSProt.ToString("0") + '%';
        }
        public void UpdateRun(ImportItem helmet, ImportItem upperBody, ImportItem lowerBody)
        {
            double finalRun = GetMoveSpeedArmor(helmet, upperBody, lowerBody) / 100.0;
            ArmorRunLabel.Content = finalRun.ToString("0.00");
        }

        public static double GetMoveSpeedArmor(ImportItem helmet, ImportItem upperBody, ImportItem lowerBody)
        {
            double allRun = (helmet?.pawnModifiers.MovementSpeed ?? 0);
            allRun += (upperBody?.pawnModifiers.MovementSpeed ?? 0);
            allRun += (lowerBody?.pawnModifiers.MovementSpeed ?? 0);
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
        public void UpdateHRV(ImportItem helmet)
        {
            double currentHealth = (helmet?.pawnModifiers.HRVDuration ?? 0);
            ArmorHRVLabel.Content = currentHealth.ToString("0.0") + 'u';
        }
        public void UpdateHRVRecharge(ImportItem helmet)
        {
            double currentHealth = (helmet?.pawnModifiers.HRVRechargeRate ?? 0);
            ArmorHRVRechargeLabel.Content = currentHealth.ToString("0.0") + "u/s";
        }
        public void UpdateGearSlots(ImportItem upperBody, ImportItem lowerBody)
        { 
            double currentGearSlots = (upperBody?.pawnModifiers?.GearSlots ?? 0) + (lowerBody?.pawnModifiers?.GearSlots ?? 0);
            
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

        private static bool CheckForPistolAndBarrel(ImportItem item)
        {
            return item.name == "Light Pistol" || item.name == "Heavy Pistol" || item.name == "Prestige Light Pistol";
        }

        private static void SetStock(ImportItem reciever, Image barrel, Image stock, ImportItem item)
        {
            if (reciever != null)
            {
                if (CheckForPistolAndBarrel(reciever))
                {
                    if (barrel.DataContext is ImportItem Barrel)
                    {
                        if (!string.IsNullOrEmpty(Barrel.name) && Barrel.name != "No Barrel Mod")
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

        private void FillEmptyPrimaryMods(ImportItem reciever)
        {
            FillEmptyMods(reciever, PrimaryMuzzleImage, PrimaryBarrelImage, PrimaryMagazineImage, PrimaryScopeImage, PrimaryCrosshairImage, PrimaryStockImage, PrimaryCamoWeaponImage, PrimaryTagImage);
        }

        private void FillEmptySecondaryMods(ImportItem reciever)
        {
            FillEmptyMods(reciever, SecondaryMuzzleImage, SecondaryBarrelImage, SecondaryMagazineImage, SecondaryScopeImage, SecondaryCrosshairImage, SecondaryStockImage, SecondaryCamoWeaponImage, SecondaryTagImage);
        }

        private static void FillEmptyMods(ImportItem reciever, Image muzzle, Image barrel, Image magazine, Image scope, Image crosshair, Image stock, Image camo, Image tag)
        {
            if (reciever.tooltip != "Depot Item!")
            {
                MagiCowsWeapon weapon = MagiCowsWeapon.GetDefaultSetupOfReciever(reciever);
                if (muzzle.DataContext == null || (muzzle.DataContext as ImportItem).name == MagiCowsWeapon.NoMuzzle)
                {
                    muzzle.DataContext = weapon.GetMuzzle();
                }
                if (barrel.DataContext == null || (barrel.DataContext as ImportItem).name == MagiCowsWeapon.NoBarrel)
                {
                    barrel.DataContext = weapon.GetBarrel();
                    if (CheckForPistolAndBarrel(reciever))
                    {
                        stock.DataContext = null;
                    }
                }
                if (stock.DataContext == null || (barrel.DataContext as ImportItem).name == MagiCowsWeapon.NoStock)
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
                    tag.DataContext = weapon.GetTag();   
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
            UpdateLoadoutWeapon(ActiveLoadout.Primary, PrimaryRecieverImage.DataContext as ImportItem, PrimaryMuzzleImage.DataContext as ImportItem, PrimaryBarrelImage.DataContext as ImportItem, PrimaryMagazineImage.DataContext as ImportItem, PrimaryScopeImage.DataContext as ImportItem, PrimaryStockImage.DataContext as ImportItem, PrimaryTagImage.DataContext as ImportItem, PrimaryCamoWeaponImage.DataContext as ImportItem);
            UpdateLoadoutWeapon(ActiveLoadout.Secondary, SecondaryRecieverImage.DataContext as ImportItem, SecondaryMuzzleImage.DataContext as ImportItem, SecondaryBarrelImage.DataContext as ImportItem, SecondaryMagazineImage.DataContext as ImportItem, SecondaryScopeImage.DataContext as ImportItem, SecondaryStockImage.DataContext as ImportItem, SecondaryTagImage.DataContext as ImportItem, SecondaryCamoWeaponImage.DataContext as ImportItem);
            if (GearImage1.IsEnabled)
            { ActiveLoadout.Gear1 = ImportSystem.GetGearID(GearImage1.DataContext as ImportItem); }
            else
            { ActiveLoadout.Gear1 = 0; }

            if (GearImage2.IsEnabled)
            { ActiveLoadout.Gear2 = ImportSystem.GetGearID(GearImage2.DataContext as ImportItem); }
            else
            { ActiveLoadout.Gear2 = 0; }

            if (GearImage3.IsEnabled)
            { ActiveLoadout.Gear3 = ImportSystem.GetGearID(GearImage3.DataContext as ImportItem); }
            else
            { ActiveLoadout.Gear3 = 0; }

            if (GearImage4.IsEnabled)
            { ActiveLoadout.Gear4 = ImportSystem.GetGearID(GearImage4.DataContext as ImportItem); }
            else
            { ActiveLoadout.Gear4 = 0; }

            ActiveLoadout.Tactical = ImportSystem.GetTacticalID(TacticalImage.DataContext as ImportItem);
            ActiveLoadout.Helmet = ImportSystem.GetHelmetID(HelmetImage.DataContext as ImportItem);
            ActiveLoadout.UpperBody = ImportSystem.GetUpperBodyID(UpperBodyImage.DataContext as ImportItem);
            ActiveLoadout.LowerBody = ImportSystem.GetLowerBodyID(LowerBodyImage.DataContext as ImportItem);
            ActiveLoadout.Camo = ImportSystem.GetCamoBodyID((PlayerCamoBodyImage.DataContext as ImportItem));
        }

        private static void UpdateLoadoutWeapon(MagiCowsWeapon weapon, ImportItem reciever, ImportItem muzzle, ImportItem barrel, ImportItem magazine, ImportItem scope, ImportItem stock, ImportItem tag, ImportItem camo)
        {
            weapon.Receiver = reciever?.name ?? "Assault Rifle";
            weapon.Muzzle = ImportSystem.GetMuzzleID(muzzle);
            weapon.Barrel = barrel?.name ?? "No Barrel Mod";
            weapon.Magazine = ImportSystem.GetMagazineID(magazine);
            weapon.Scope = scope?.name ?? "No Optic Mod";
            weapon.Stock = stock?.name ?? "No Stock";
            weapon.Tag = ImportSystem.GetTagID(tag);
            weapon.Camo = ImportSystem.GetCamoBodyID(camo);
        }

        private void UpdatePrimaryImages(Image image)
        {
            FilterWeapon = PrimaryRecieverImage.DataContext as ImportItem;
            LoggingSystem.LogInfo("ItemList Filter set Primary:" + (FilterWeapon?.name ?? "None"));


            if (image.Name.Contains("Reciever"))
            {
                SetItemList(ImportSystem.Weapons.primary);
                LastSelectedImage = PrimaryRecieverImage;
                LoggingSystem.LogInfo("ItemList Set for Primary Reciever");
                return;
            }

            if (image.Name.Contains("Muzzle"))
            {
                if (!(PrimaryRecieverImage.DataContext as ImportItem).IsValidModType("muzzle"))
                { return; }
                SetItemList(ImportSystem.Mods.muzzles);
                LastSelectedImage = image;
                LoggingSystem.LogInfo("ItemList Set for Muzzles");
                return;
            }
            if (image.Name.Contains("Crosshair"))
            {
                var item = (PrimaryScopeImage.DataContext as ImportItem);
                item.LoadCrosshair();
                ItemList.ItemsSource = new ImportItem[] { item };
                LastSelectedImage = PrimaryScopeImage;
                LoggingSystem.LogInfo("ItemList Set for Scopes");
                return;
            }
            UpdateImages(image);
        }

        private void UpdateSecondaryImages(Image image)
        {
            FilterWeapon = SecondaryRecieverImage.DataContext as ImportItem;
            LoggingSystem.LogInfo("ItemList Filter set Secondary:" + (FilterWeapon?.name ?? "None"));

            if (image.Name.Contains("Reciever"))
            {
                SetItemList(ImportSystem.Weapons.secondary);
                LastSelectedImage = SecondaryRecieverImage;
                LoggingSystem.LogInfo("ItemList Set for Secondary Reciever");
                return;
            }

            if (image.Name.Contains("Muzzle"))
            {
                SetItemList(ImportSystem.Mods.muzzles);
                LastSelectedImage = image;
                LoggingSystem.LogInfo("ItemList Set for Muzzles");
                return;
            }
            if (image.Name.Contains("Crosshair"))
            {
                var item = (SecondaryScopeImage.DataContext as ImportItem);
                item.LoadCrosshair();
                ItemList.ItemsSource = new ImportItem[] { item };
                LastSelectedImage = SecondaryScopeImage;
                LoggingSystem.LogInfo("ItemList Set for Scopes");
                return;
            }
            UpdateImages(image);
        }

        private void UpdateImages(Image image)
        {
            LastSelectedImage = image;
            
            if (image.Name.Contains("Reciever"))
            {
                SetItemList(ImportSystem.Weapons.primary);
                LoggingSystem.LogInfo("ItemList Set for Primary Reciever");
                return;
            }
            if (image.Name.Contains("Magazine"))
            {
                SetItemList(ImportSystem.Mods.magazines);
                LoggingSystem.LogInfo("ItemList Set for Magazines");
                return;
            }
            if (image.Name.Contains("Stock"))
            {
                SetItemList(ImportSystem.Mods.stocks);
                LoggingSystem.LogInfo("ItemList Set for Stocks");
                return;
            }
            if (image.Name.Contains("Scope"))
            {
                foreach (var item in ImportSystem.Mods.scopes)
                {
                    item.RemoveCrosshair();
                }
                SetItemList(ImportSystem.Mods.scopes);
                LoggingSystem.LogInfo("ItemList Set for Scopes");
                return;
            }
            if (image.Name.Contains("Barrel"))
            {
                SetItemList(ImportSystem.Mods.barrels);
                LoggingSystem.LogInfo("ItemList Set for Barrels");
                return;
            }
            if (image.Name.Contains("Grip"))
            {
                SetItemList(ImportSystem.Mods.grips);
                LoggingSystem.LogInfo("ItemList Set for Grips");
                return;
            }
            if (image.Name.Contains("Tag"))
            {
                SetItemList(ImportSystem.Gear.hangers);
                LoggingSystem.LogInfo("ItemList Set for Tags");
                return;
            }
            if (image.Name.Contains("CamoWeapon"))
            {
                SetItemList(ImportSystem.Mods.camosBody);
                LoggingSystem.LogInfo("ItemList Set for 'Weapon' Camos");
                return;
            }
        }

        public void SetItemList(ImportItem[] list)
        {
            ItemList.ItemsSource = list;
            ApplySorting();
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
                            SetItemList(ImportSystem.Gear.attachments);
                            LastSelectedImage = img;
                            LoggingSystem.LogInfo("ItemList Set for Gear");
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
                        SetItemList(ImportSystem.Gear.tactical);
                        LastSelectedImage = image;
                        LoggingSystem.LogInfo("ItemList Set for Tactical");
                        return;
                    }
                    if (image.Name.Contains("CamoBody"))
                    {
                        SetItemList(ImportSystem.Mods.camosBody);
                        LastSelectedImage = image;
                        LoggingSystem.LogInfo("ItemList Set for Body Camos");
                        return;
                    }
                    if (image.Name.Contains("Helmet"))
                    {
                        SetItemList(ImportSystem.Gear.helmets);
                        LastSelectedImage = image;
                        LoggingSystem.LogInfo("ItemList Set for Helemts");
                        return;
                    }
                    if (image.Name.Contains("UpperBody"))
                    {
                        SetItemList(ImportSystem.Gear.upperBodies);
                        LastSelectedImage = image;
                        LoggingSystem.LogInfo("ItemList Set for UpperBodies");
                        return;
                    }
                    if (image.Name.Contains("LowerBody"))
                    {
                        SetItemList(ImportSystem.Gear.lowerBodies);
                        LastSelectedImage = image;
                        LoggingSystem.LogInfo("ItemList Set for LowerBodies");
                        return;
                    }
                    LoggingSystem.LogInfo("ItemList Din't get set");
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
            SetItemToImage(PrimaryRecieverImage, primary.GetReciever(), updateLoadout);
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
        bool textchnaging = false;
        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!textchnaging)
            {
                profilechanging = true;
                ExportSystem.ActiveProfile = ProfileComboBox.SelectedValue as MagiCowsProfile;
                PlayerNameTextBox.Text = ExportSystem.ActiveProfile.PlayerName;

                IsFemaleCheckBox.DataContext = ExportSystem.ActiveProfile;

                SetLoadout(ExportSystem.ActiveProfile.Loadout1);
                profilechanging = false;
            }
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
            ExportSystem.CreateSEProfile(ExportSystem.ActiveProfile);
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

        private void IsFemaleCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var upper = UpperBodyImage.DataContext as ImportItem;
            var lower = LowerBodyImage.DataContext as ImportItem;
            UpperBodyImage.DataContext = null;
            LowerBodyImage.DataContext = null;
            UpperBodyImage.DataContext = upper;
            LowerBodyImage.DataContext = lower;
            var source = ItemList.ItemsSource;
            ItemList.ItemsSource = null;
            ItemList.ItemsSource = source;
        }

        private void IsFemaleCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var upper = UpperBodyImage.DataContext as ImportItem;
            var lower = LowerBodyImage.DataContext as ImportItem;
            UpperBodyImage.DataContext = null;
            LowerBodyImage.DataContext = null;
            UpperBodyImage.DataContext = upper;
            LowerBodyImage.DataContext = lower;
            var source = ItemList.ItemsSource;
            ItemList.ItemsSource = null;
            ItemList.ItemsSource = source;
        }

        public void ApplySorting()
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ItemList.ItemsSource);
            if (view != null)
            {
                view.SortDescriptions.Clear();
                switch ((ImportItemSortingType)SortComboBox1.SelectedItem)
                {
                    case ImportItemSortingType.NameAsc:
                        view.SortDescriptions.Add(new System.ComponentModel.SortDescription("name", System.ComponentModel.ListSortDirection.Ascending));
                        break;
                    case ImportItemSortingType.NameDesc:
                        view.SortDescriptions.Add(new System.ComponentModel.SortDescription("name", System.ComponentModel.ListSortDirection.Descending));
                        break;

                    case ImportItemSortingType.DamageAsc:
                        view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Damage", System.ComponentModel.ListSortDirection.Ascending));
                        break;
                    case ImportItemSortingType.DamageDesc:
                        view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Damage", System.ComponentModel.ListSortDirection.Descending));
                        break;

                    case ImportItemSortingType.SpreadAsc:
                        view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Spread", System.ComponentModel.ListSortDirection.Ascending));
                        break;
                    case ImportItemSortingType.SpreadDesc:
                        view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Spread", System.ComponentModel.ListSortDirection.Descending));
                        break;

                    case ImportItemSortingType.RecoilAsc:
                        view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Recoil", System.ComponentModel.ListSortDirection.Ascending));
                        break;
                    case ImportItemSortingType.RecoilDesc:
                        view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Recoil", System.ComponentModel.ListSortDirection.Descending));
                        break;


                    case ImportItemSortingType.RangeAsc:
                        view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Range", System.ComponentModel.ListSortDirection.Ascending));
                        break;
                    case ImportItemSortingType.RangeDesc:
                        view.SortDescriptions.Add(new System.ComponentModel.SortDescription("Range", System.ComponentModel.ListSortDirection.Descending));
                        break;
                }
            }
        }

        private void SortComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySorting();
        }
    }
}
