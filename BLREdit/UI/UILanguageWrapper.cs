using BLREdit.API.ImportSystem;

namespace BLREdit.UI
{
    public static class UILanguageWrapper
    {
        #region ProfileSetup
        public static string BtnPSAddProfile { get { return LanguageSet.GetWordUI("Add Profile"); } }
        public static string BtnPSCopyToClipboard { get { return LanguageSet.GetWordUI("CopyToClipboard"); } }
        public static string BtnPSDuplicateProfile { get { return LanguageSet.GetWordUI("Duplicate"); } }
        public static string BtnPSAddRandomLoadout { get { return LanguageSet.GetWordUI("RandomLoadout"); } }

        #region Loadouts
        public static string BtnLoadout1 { get { return LanguageSet.GetWordUI("Loadout 1"); } }
        public static string BtnLoadout2 { get { return LanguageSet.GetWordUI("Loadout 2"); } }
        public static string BtnLoadout3 { get { return LanguageSet.GetWordUI("Loadout 3"); } }
        #endregion Loadouts

        #region WeaponStats
        public static string LblPSDamage { get { return LanguageSet.GetWordUI("Dmage:"); } }
        public static string LblPSFirerate { get { return LanguageSet.GetWordUI("Firerate:"); } }
        public static string LblPSAmmo { get { return LanguageSet.GetWordUI("Ammo:"); } }
        public static string LblPSReloadPartial { get { return LanguageSet.GetWordUI("Reload:"); } }
        public static string LblPSReloadEmpty { get { return LanguageSet.GetWordUI("Empty:"); } }
        public static string LblPSSpreadAim { get { return LanguageSet.GetWordUI("Aim:"); } }
        public static string LblPSSpreadHip { get { return LanguageSet.GetWordUI("Hip:"); } }
        public static string LblPSSpreadMove { get { return LanguageSet.GetWordUI("Move:"); } }
        public static string LblPSRecoilHip { get { return LanguageSet.GetWordUI("Recoil Hip:"); } }
        public static string LblPSRecoilAim { get { return LanguageSet.GetWordUI("Recoil Aim:"); } }
        public static string LblPSZoom { get { return LanguageSet.GetWordUI("Zoom:"); } }
        public static string LblPSScopeInTime { get { return LanguageSet.GetWordUI("Scope In:"); } }
        public static string LblPSRange { get { return LanguageSet.GetWordUI("Range:"); } }
        #endregion WeaponStats

        public static string LblPSRun { get { return LanguageSet.GetWordUI("Run:"); } }

        #region ArmorStats
        public static string LblPSHealth { get { return LanguageSet.GetWordUI("Health:"); } }
        public static string LblPSHeadArmor { get { return LanguageSet.GetWordUI("Head Armor:"); } }
        public static string LblPSHRV { get { return LanguageSet.GetWordUI("HRV:"); } }
        public static string LblPSRecharge { get { return LanguageSet.GetWordUI("Recharge:"); } }
        public static string LblPSGearSlots { get { return LanguageSet.GetWordUI("Gear Slots:"); } }
        #endregion ArmorStats

        public static string LblPSGenderToggle { get { return LanguageSet.GetWordUI("Female"); } }

        #endregion ProfileSetup

        #region ItemList
        public static string BtnItemList { get { return LanguageSet.GetWordUI("Item List"); } }
        #endregion

        #region Launcher
        public static string BtnLauncher { get { return LanguageSet.GetWordUI("Launcher"); } }
        public static string LblLauRemove { get { return LanguageSet.GetWordUI("X"); } }

        #region Server List
        public static string BtnLauServerList { get { return LanguageSet.GetWordUI("Server List"); } }
        public static string LblLauServerAddress { get { return LanguageSet.GetWordUI("Server Address"); } }
        public static string LblLauServerPort { get { return LanguageSet.GetWordUI("Port"); } }
        public static string LblLauIsServerOnline { get { return LanguageSet.GetWordUI("IsOnline"); } }
        public static string LblLauServerPing { get { return LanguageSet.GetWordUI("Ping"); } }
        public static string LblLauConnectToServer { get { return LanguageSet.GetWordUI("Launch"); } }
        public static string LblLauIsServerDefault { get { return LanguageSet.GetWordUI("Default"); } }
        public static string BtnLauAddServer { get { return LanguageSet.GetWordUI("Add New Server"); } }
        public static string BtnLauRefreshPing { get { return LanguageSet.GetWordUI("Refresh Ping"); } }
        #endregion Server List

        #region Client List
        public static string BtnLauClientList { get { return LanguageSet.GetWordUI("Client List"); } }
        public static string LblLauClientOriginalPath { get { return LanguageSet.GetWordUI("Original Path"); } }
        public static string LblLauClientPatchedPath { get { return LanguageSet.GetWordUI("Patched Path"); } }
        public static string LblLauIsClientPatched { get { return LanguageSet.GetWordUI("IsPatched"); } }
        public static string LblLauClientConnectToDefaultServer { get { return LanguageSet.GetWordUI("Launch"); } }
        public static string LblLauIsClientDefault { get { return LanguageSet.GetWordUI("Default"); } }
        public static string LblLauPatchClient { get { return LanguageSet.GetWordUI("Patch"); } }
        public static string LblLauLaunchBotServer { get { return LanguageSet.GetWordUI("Bot Match"); } }
        public static string LblLauLaunchServer { get { return LanguageSet.GetWordUI("Server"); } }
        public static string BtnLauAddClient { get { return LanguageSet.GetWordUI("Add New Game Client"); } }
        #endregion Client List

        #endregion Launcher



        #region AdvancedInfo
        public static string BtnAdvancedInfo { get { return LanguageSet.GetWordUI("Advanced Info"); } }
        public static string LblAdvPrimary { get { return LanguageSet.GetWordUI("Primary"); } }
        public static string LblAdvSecondary { get { return LanguageSet.GetWordUI("Secondary"); } }

        #region Weapon
        public static string LblAdvFragmentsPerShell { get { return LanguageSet.GetWordUI("Fragments Per Shell:"); } }
        public static string LblAdvZoomFireRate { get { return LanguageSet.GetWordUI("Zoom Firerate:"); } }
        public static string LblAdvSpreadCrouchMultiplier { get { return LanguageSet.GetWordUI("Spread Crouch Multiplier:"); } }
        public static string LblAdvSpreadJumpMultiplier { get { return LanguageSet.GetWordUI("Spread Jump Multiplier:"); } }
        public static string LblAdvSpreadCenterWeight { get { return LanguageSet.GetWordUI("Spread Center Weight:"); } }
        public static string LblAdvSpreadCenter { get { return LanguageSet.GetWordUI("Spread Center:"); } }
        public static string LblAdvRecoilVerticalRatio { get { return LanguageSet.GetWordUI("Recoil Vertical Ratio:"); } }
        public static string LblAdvRecoilRecoveryTime { get { return LanguageSet.GetWordUI("Recoil Recovery Time:"); } }
        public static string LblAdvRecoilAccumulation { get { return LanguageSet.GetWordUI("Recoil Accumulation:"); } }
        public static string LblAdvDamage { get { return LanguageSet.GetWordUI("Damage:"); } }
        public static string LblAdvAccuracy { get { return LanguageSet.GetWordUI("Accuracy:"); } }
        public static string LblAdvRange { get { return LanguageSet.GetWordUI("Range:"); } }
        public static string LblAdvReload { get { return LanguageSet.GetWordUI("Reload:"); } }
        public static string LblAdvRecoil { get { return LanguageSet.GetWordUI("Recoil:"); } }
        public static string LblAdvWeaponRun { get { return LanguageSet.GetWordUI("Run:"); } }
        #endregion Weapon

        public static string LblAdvCurrentMods { get { return LanguageSet.GetWordUI("Current Mods:"); } }

        #region Armor
        public static string LblAdvArmorGear { get { return LanguageSet.GetWordUI("Armor / Gear"); } }
        public static string LblAdvElectroProt { get { return LanguageSet.GetWordUI("Electro Protection:"); } }
        public static string LblAdvEplxosiveProt { get { return LanguageSet.GetWordUI("Explosive Protection:"); } }
        public static string LblAdvIncendiaryProt { get { return LanguageSet.GetWordUI("Incendiary Protection:"); } }
        public static string LblAdvInfraredProt { get { return LanguageSet.GetWordUI("Infrared Protection:"); } }
        public static string LblAdvMeleeProt { get { return LanguageSet.GetWordUI("Melee Protection:"); } }
        public static string LblAdvToxicProt { get { return LanguageSet.GetWordUI("Toxic Protection:"); } }
        public static string LblAdvHealth { get { return LanguageSet.GetWordUI("Health:"); } }
        public static string LblAdvHeadArmor { get { return LanguageSet.GetWordUI("Head Armor:"); } }
        public static string LblAdvArmorRun { get { return LanguageSet.GetWordUI("Run:"); } }
        public static string LblAdvHRV { get { return LanguageSet.GetWordUI("HRV:"); } }
        public static string LblAdvRecharge { get { return LanguageSet.GetWordUI("Recharge:"); } }
        public static string LblAdvGearSlots { get { return LanguageSet.GetWordUI("Gear:"); } }
        #endregion Armor

        #endregion AdvancedInfo
    }
}
