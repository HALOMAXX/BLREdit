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

            if (LoggingSystem.IsDebuggingEnabled) LoggingSystem.LogInfo(context.ToString());

            app.InitializeComponent();
        }

        [TestMethod]
        public void PrimariesAndMods()
        {
            MainWindow window = new MainWindow();
            foreach (BLRItem reciever in ImportSystem.GetItemListOfType("primary"))
            {
                FullRecieverTest(reciever, window, window.PrimaryRecieverImage, window.PrimaryBarrelImage, window.PrimaryStockImage, window.PrimaryScopeImage, window.PrimaryMuzzleImage, window.PrimaryMagazineImage, window.PrimaryCamoWeaponImage, window.PrimaryTagImage);
            }
        }

        [TestMethod]
        public void SecondariesAndMods()
        {
            MainWindow window = new MainWindow();
            foreach (BLRItem reciever in ImportSystem.GetItemListOfType("secondary"))
            {
                FullRecieverTest(reciever, window, window.SecondaryRecieverImage, window.SecondaryBarrelImage, window.SecondaryStockImage, window.SecondaryScopeImage, window.SecondaryMuzzleImage, window.SecondaryMagazineImage, window.SecondaryCamoWeaponImage, window.SecondaryTagImage);
            }
        }

        public void FullRecieverTest(BLRItem reciever, MainWindow window, Image RecieverImg, Image BarrelImg, Image StockImg, Image ScopeImg, Image MuzzleImg, Image MagazineImg, Image CamoImg, Image HangerImg)
        {
            window.SetItemToImage(RecieverImg, reciever);
            foreach (BLRItem scope in ImportSystem.GetItemListOfType("scopes"))
            {
                window.SetItemToImage(ScopeImg, scope);
            }

            foreach (BLRItem muzzle in ImportSystem.GetItemListOfType("muzzles"))
            {
                window.SetItemToImage(MuzzleImg, muzzle);
            }

            foreach (BLRItem barrel in ImportSystem.GetItemListOfType("barrels"))
            {
                window.SetItemToImage(BarrelImg, barrel);
                foreach (BLRItem stock in ImportSystem.GetItemListOfType("stocks"))
                {
                    window.SetItemToImage(StockImg, stock);
                }
            }

            foreach (BLRItem magazine in ImportSystem.GetItemListOfType("magazines"))
            {
                window.SetItemToImage(MagazineImg, magazine);
            }

            foreach (BLRItem camo in ImportSystem.GetItemListOfType("camosBody"))
            {
                window.SetItemToImage(CamoImg, camo);
            }

            foreach (BLRItem hanger in ImportSystem.GetItemListOfType("hangers"))
            {
                window.SetItemToImage(HangerImg, hanger);
            }
        }
    }
}
