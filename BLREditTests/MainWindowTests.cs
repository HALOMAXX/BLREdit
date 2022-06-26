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
            foreach (BLRItem reciever in ImportSystem.GetItemListOfType(ImportSystem.PRIMARY_CATEGORY))
            {
                FullRecieverTest(reciever, window, window.PrimaryRecieverImage, window.PrimaryBarrelImage, window.PrimaryStockImage, window.PrimaryScopeImage, window.PrimaryMuzzleImage, window.PrimaryMagazineImage, window.PrimaryCamoWeaponImage, window.PrimaryTagImage);
            }
        }

        [TestMethod]
        public void SecondariesAndMods()
        {
            MainWindow window = new MainWindow();
            foreach (BLRItem reciever in ImportSystem.GetItemListOfType(ImportSystem.SECONDARY_CATEGORY))
            {
                FullRecieverTest(reciever, window, window.SecondaryRecieverImage, window.SecondaryBarrelImage, window.SecondaryStockImage, window.SecondaryScopeImage, window.SecondaryMuzzleImage, window.SecondaryMagazineImage, window.SecondaryCamoWeaponImage, window.SecondaryTagImage);
            }
        }

        public void FullRecieverTest(BLRItem reciever, MainWindow window, Image RecieverImg, Image BarrelImg, Image StockImg, Image ScopeImg, Image MuzzleImg, Image MagazineImg, Image CamoImg, Image HangerImg)
        {
            window.SetItemToImage(RecieverImg, reciever);
            foreach (BLRItem scope in ImportSystem.GetItemListOfType(ImportSystem.SCOPES_CATEGORY))
            {
                window.SetItemToImage(ScopeImg, scope);
            }

            foreach (BLRItem muzzle in ImportSystem.GetItemListOfType(ImportSystem.MUZZELS_CATEGORY))
            {
                window.SetItemToImage(MuzzleImg, muzzle);
            }

            foreach (BLRItem barrel in ImportSystem.GetItemListOfType(ImportSystem.BARRELS_CATEGORY))
            {
                window.SetItemToImage(BarrelImg, barrel);
                foreach (BLRItem stock in ImportSystem.GetItemListOfType(ImportSystem.STOCKS_CATEGORY))
                {
                    window.SetItemToImage(StockImg, stock);
                }
            }

            foreach (BLRItem magazine in ImportSystem.GetItemListOfType(ImportSystem.MAGAZINES_CATEGORY))
            {
                window.SetItemToImage(MagazineImg, magazine);
            }

            foreach (BLRItem camo in ImportSystem.GetItemListOfType(ImportSystem.CAMOS_BODIES_CATEGORY))
            {
                window.SetItemToImage(CamoImg, camo);
            }

            foreach (BLRItem hanger in ImportSystem.GetItemListOfType(ImportSystem.HANGERS_CATEGORY))
            {
                window.SetItemToImage(HangerImg, hanger);
            }
        }
    }
}
