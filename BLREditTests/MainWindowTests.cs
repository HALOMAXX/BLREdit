using Microsoft.VisualStudio.TestTools.UnitTesting;
using BLREdit;
using BLREdit.UI;
using System;
using System.Diagnostics;

namespace BLREditTests
{
    [TestClass]
    public class MainWindowTests
    {
        static App app;
        [AssemblyInitialize, STAThread]
        public static void TestIni(TestContext context)
        {
            app = new App();
            app.InitializeComponent();
            app.Run();
        }


        [TestMethod]
        public void Primaries()
        {
            foreach (ImportItem primary in ImportSystem.Weapons.primary)
            { 
                MainWindow.self.SetItemToImage(MainWindow.self.PrimaryRecieverImage ,primary);
            }
        }

        [TestMethod]
        public void Secondaries()
        {
            foreach (ImportItem secondary in ImportSystem.Weapons.secondary)
            {
                MainWindow.self.SetItemToImage(MainWindow.self.SecondaryRecieverImage, secondary);
            }
        }

        [TestMethod]
        public void PrimariesAndMods()
        {
            foreach (ImportItem reciever in ImportSystem.Weapons.primary)
            {
                MainWindow.self.SetItemToImage(MainWindow.self.PrimaryRecieverImage, reciever);

                foreach (ImportItem barrel in ImportSystem.Mods.barrels)
                {
                    MainWindow.self.SetItemToImage(MainWindow.self.PrimaryBarrelImage, barrel);
                }

                foreach (ImportItem muzzle in ImportSystem.Mods.muzzles)
                {
                    MainWindow.self.SetItemToImage(MainWindow.self.PrimaryMuzzleImage, muzzle);
                }

                foreach (ImportItem magazine in ImportSystem.Mods.magazines)
                {
                    MainWindow.self.SetItemToImage(MainWindow.self.PrimaryMagazineImage, magazine);
                }

                foreach (ImportItem scope in ImportSystem.Mods.scopes)
                {
                    MainWindow.self.SetItemToImage(MainWindow.self.PrimaryScopeImage, scope);
                }

                foreach (ImportItem stock in ImportSystem.Mods.stocks)
                {
                    MainWindow.self.SetItemToImage(MainWindow.self.PrimaryStockImage, stock);
                }

                foreach (ImportItem camo in ImportSystem.Mods.camosBody)
                {
                    MainWindow.self.SetItemToImage(MainWindow.self.PrimaryCamoWeaponImage, camo);
                }

                foreach (ImportItem hanger in ImportSystem.Gear.hangers)
                {
                    MainWindow.self.SetItemToImage(MainWindow.self.PrimaryTagImage, hanger);
                }
            }
        }

        [TestMethod]
        public void SecondariesAndMods()
        {
            foreach (ImportItem reciever in ImportSystem.Weapons.secondary)
            {
                MainWindow.self.SetItemToImage(MainWindow.self.SecondaryRecieverImage, reciever);

                foreach (ImportItem barrel in ImportSystem.Mods.barrels)
                {
                    MainWindow.self.SetItemToImage(MainWindow.self.SecondaryBarrelImage, barrel);
                }

                foreach (ImportItem muzzle in ImportSystem.Mods.muzzles)
                {
                    MainWindow.self.SetItemToImage(MainWindow.self.SecondaryMuzzleImage, muzzle);
                }

                foreach (ImportItem magazine in ImportSystem.Mods.magazines)
                {
                    MainWindow.self.SetItemToImage(MainWindow.self.SecondaryMagazineImage, magazine);
                }

                foreach (ImportItem scope in ImportSystem.Mods.scopes)
                {
                    MainWindow.self.SetItemToImage(MainWindow.self.SecondaryScopeImage, scope);
                }

                foreach (ImportItem stock in ImportSystem.Mods.stocks)
                {
                    MainWindow.self.SetItemToImage(MainWindow.self.SecondaryStockImage, stock);
                }

                foreach (ImportItem camo in ImportSystem.Mods.camosBody)
                {
                    MainWindow.self.SetItemToImage(MainWindow.self.SecondaryCamoWeaponImage, camo);
                }

                foreach (ImportItem hanger in ImportSystem.Gear.hangers)
                {
                    MainWindow.self.SetItemToImage(MainWindow.self.SecondaryTagImage, hanger);
                }
            }
        }
    }
}
