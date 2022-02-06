using Microsoft.VisualStudio.TestTools.UnitTesting;
using BLREdit;
using BLREdit.UI;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace BLREditTests
{
    [TestClass]
    public class MainWindowTests
    {
        static App app;

        [AssemblyInitialize]
        public static void TestIni(TestContext context)
        {
            app = new App();

            LoggingSystem.LogInfo(context.ToString());

            app.InitializeComponent();
        }

        [TestMethod]
        public void PrimariesAndMods()
        {
            MainWindow window = new MainWindow();
            foreach (ImportItem reciever in ImportSystem.Weapons.primary)
            {
                FullRecieverTest(reciever, window, window.PrimaryRecieverImage, window.PrimaryBarrelImage, window.PrimaryStockImage, window.PrimaryScopeImage, window.PrimaryMuzzleImage, window.PrimaryMagazineImage, window.PrimaryCamoWeaponImage, window.PrimaryTagImage);
            }
        }

        [TestMethod]
        public void SecondariesAndMods()
        {
            MainWindow window = new MainWindow();
            foreach (ImportItem reciever in ImportSystem.Weapons.secondary)
            {
                FullRecieverTest(reciever, window, window.SecondaryRecieverImage, window.SecondaryBarrelImage, window.SecondaryStockImage, window.SecondaryScopeImage, window.SecondaryMuzzleImage, window.SecondaryMagazineImage, window.SecondaryCamoWeaponImage, window.SecondaryTagImage);
            }
        }

        public void FullRecieverTest(ImportItem reciever, MainWindow window, Image RecieverImg, Image BarrelImg, Image StockImg, Image ScopeImg, Image MuzzleImg, Image MagazineImg, Image CamoImg, Image HangerImg)
        {
            window.SetItemToImage(RecieverImg, reciever);
            foreach (ImportItem scope in ImportSystem.Mods.scopes)
            {
                window.SetItemToImage(ScopeImg, scope);
            }

            foreach (ImportItem muzzle in ImportSystem.Mods.muzzles)
            {
                window.SetItemToImage(MuzzleImg, muzzle);
            }

            foreach (ImportItem barrel in ImportSystem.Mods.barrels)
            {
                window.SetItemToImage(BarrelImg, barrel);
                foreach (ImportItem stock in ImportSystem.Mods.stocks)
                {
                    window.SetItemToImage(StockImg, stock);
                }
            }

            foreach (ImportItem magazine in ImportSystem.Mods.magazines)
            {
                window.SetItemToImage(MagazineImg, magazine);
            }

            foreach (ImportItem camo in ImportSystem.Mods.camosBody)
            {
                window.SetItemToImage(CamoImg, camo);
            }

            foreach (ImportItem hanger in ImportSystem.Gear.hangers)
            {
                window.SetItemToImage(HangerImg, hanger);
            }
        }
    }
}
