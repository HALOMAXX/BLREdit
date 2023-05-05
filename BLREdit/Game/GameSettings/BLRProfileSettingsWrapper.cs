﻿using BLREdit.UI;
using BLREdit.UI.Views;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BLREdit.Game;

public sealed class BLRProfileSettingsWrapper : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion Events

    static readonly BLRProfileSettings[] defaultProfile = IOResources.DeserializeFile<BLRProfileSettings[]>($"{IOResources.ASSET_DIR}{IOResources.JSON_DIR}defaultProfile.json");

    public Dictionary<int, BLRProfileSettings> Settings { get; } = new();
    public BLRKeyBindings KeyBindings { get; private set; }
    public string ProfileName { get; private set; }

    public static Dictionary<string,PropertyInfo> SettingPropertiesString { get; } = GetSettingsProperties();
    public static Dictionary<int, PropertyInfo> SettingPropertiesInt { get; } = GetSettingsPropertiesID();
    private static Dictionary<string, PropertyInfo> GetSettingsProperties()
    {
        var list = typeof(BLRProfileSettingsWrapper).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ProfileSettingAttribute))).ToArray();
        var dict = new Dictionary<string, PropertyInfo>();
        foreach (var sett in list)
        { 
            dict.Add(sett.Name, sett);
        }
        return dict;
    }
    private static Dictionary<int, PropertyInfo> GetSettingsPropertiesID()
    {
        var list = typeof(BLRProfileSettingsWrapper).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ProfileSettingAttribute))).ToArray();
        var dict = new Dictionary<int, PropertyInfo>();
        foreach (var sett in list)
        {
            dict.Add(sett.GetCustomAttribute<ProfileSettingAttribute>().ID, sett);
        }
        return dict;
    }

    public BLRProfileSettingsWrapper(string profileName ,BLRProfileSettings[]? settings, BLRKeyBindings? keyBindings)
    {
        settings ??= defaultProfile;
        keyBindings ??= new();

        foreach (var setting in settings)
        {
            Settings.Add(setting.ProfileSetting.PropertyId, setting);
        }

        ProfileName = profileName;
        KeyBindings= keyBindings;
    }

    public void UpdateFromFile(BLRProfileSettings[] settings)
    {
        foreach (var setting in settings)
        {
            SetValueOf(setting.ProfileSetting.Data.Value1, SettingPropertiesInt[setting.ProfileSetting.PropertyId].Name);
        }
    }

    public BLRProfileSettings[] GetSettings()
    {
        List<BLRProfileSettings> settings = new();
        foreach (var setting in Settings)
        {
            settings.Add(setting.Value);
        }
        return settings.ToArray();
    }

    private int GetValueOf([CallerMemberName] string name = null)
    {
        if (string.IsNullOrEmpty(name)) return 0;

        var property = SettingPropertiesString[name];
        var attribute = property.GetCustomAttribute<ProfileSettingAttribute>();
        var setting = Settings[attribute.ID];

        return setting.ProfileSetting.Data.Value1;
    }

    private void SetValueOf(int Value, [CallerMemberName] string name = null)
    {

        if (string.IsNullOrEmpty(name)) return;
        var property = SettingPropertiesString[name];
        var attribute = property.GetCustomAttribute<ProfileSettingAttribute>();
        var setting = Settings[attribute.ID];
        setting.ProfileSetting.Data.Value1 = Value;
        OnPropertyChanged(name);
    }

    public static int Clamp(int input, int min, int max)
    {
        return Math.Min(Math.Max(input, min), max);
    }

    #region Settings

    /// <summary>
    /// AutoReload Toggle ID:33
    /// </summary>
    [ProfileSetting(33)]public bool AutoReload { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// AutoWeaponSwitch Toggle ID:34
    /// </summary>
    [ProfileSetting(34)] public bool AutoWeaponSwitch { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// Gamepad Sensitivity ID:35 ValueRange 17000-159500 Base = ?? (0=0+Base / 25=50000+Base / 50=100000+Base)
    /// 
    /// Ingame Settings Scale 2x
    /// </summary>
    [ProfileSetting(35)] public int GamepadSensitivity { get { return GetValueOf(); } set { SetValueOf(Clamp(value, 17000, 159500)); } }
    /// <summary>
    /// SwapSticks Toggle ID:40
    /// </summary>
    [ProfileSetting(40)] public bool SwapSticks { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// PlayTime Alive ID:68 Seconds
    /// </summary>
    [ProfileSetting(68)] public int PlayTime { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Crouch Toggle ID:90
    /// </summary>
    [ProfileSetting(90)] public bool CrouchToggle { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// Invert Look ID:91
    /// </summary>
    [ProfileSetting(91)] public bool InvertLook { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// TightAim Toggle ID:92
    /// </summary>
    [ProfileSetting(92)] public bool TightAimToggle { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// Gamepad TightAim Sensitivity ID:94 ValueRange 1000-216000 Base = 1000 (0=0+Base / 25=50000+Base / 50=100000+Base)
    /// 
    /// Ingame Settings Scale 2x
    /// </summary>
    [ProfileSetting(94)] public int GamepadTightAimSensitivity { get { return GetValueOf(); } set { SetValueOf(Clamp(value, 1000, 216000)); } }
    /// <summary>
    /// Sprint Toggle ID:95
    /// </summary>
    [ProfileSetting(95)] public bool SprintToggle { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// HRV Toggle ID:96
    /// </summary>
    [ProfileSetting(96)] public bool HRVToggle { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// Mouse Acceleration Toggle ID:99
    /// </summary>
    [ProfileSetting(99)] public bool MouseAcceleration { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// Scoreboard Toggle ID:101
    /// </summary>
    [ProfileSetting(101)] public bool ScoreboardToggle { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// Smart Reticle Opacity ID:102 ValueRange 0-100
    /// </summary>
    [ProfileSetting(102)] public int SmartReticleOpacity { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Tick Marks ID:103 ValueRange 0-12
    /// </summary>
    [ProfileSetting(103)] public int TickMarks { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Crosshair Rotation ID:104 Stride=45 ValueRange 0-315
    /// </summary>
    [ProfileSetting(104)] public int CrosshairRotation { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Crosshair Opacity ID:105 ValueRange 0-100
    /// </summary>
    [ProfileSetting(105)] public int CrosshairOpacity { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Tick Length ID:106 ValueRange 0-20
    /// </summary>
    [ProfileSetting(106)] public int TickLength { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Neutral Color ID:107 Colors 0-?(21)
    /// </summary>
    [ProfileSetting(107)] public int NeutralColor { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Friendly Color ID:108 Colors 0-?(21)
    /// </summary>
    [ProfileSetting(108)] public int FriendlyColor { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Enemy Color ID:109 Colors 0-?(21)
    /// </summary>
    [ProfileSetting(109)] public int EnemyColor { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// EnemyFar Color ID:110 Colors 0-?(21)
    /// </summary>
    [ProfileSetting(110)] public int EnemyFarColor { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// FOV(Y) ID:111 ValueRange 30-100 can go above will reset instantly when opening Game settings menu
    /// </summary>
    [ProfileSetting(111)] public int FieldOfView { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Gore ID:112 None=0 Mild=1 Full=2
    /// </summary>
    [ProfileSetting(112)] public int Gore { get { return GetValueOf(); } set { SetValueOf(Clamp(value, 0, 2)); } }
    /// <summary>
    /// Hide HUD ID:113
    /// </summary>
    [ProfileSetting(113)] public bool HideHUD { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// AllNotifications ID:114 Off=0 TopRight=1 TopLeft=2 BottomLeft=3 BottomRight=4
    /// </summary>
    [ProfileSetting(114)] public int AllNotifications { get { return GetValueOf(); } set { SetValueOf(Clamp(value, 0, 4)); } }
    /// <summary>
    /// Static Crosshair ID:115
    /// </summary>
    [ProfileSetting(115)] public bool StaticCrosshair { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// Depot Ping ID:117
    /// </summary>
    [ProfileSetting(117)] public bool DepotPing { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// SpawnEffects ID:119
    /// </summary>
    [ProfileSetting(119)] public bool SpawnEffects { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// Music Volume ID:121 ValueRange 0-100
    /// </summary>
    [ProfileSetting(121)] public int MusicVolume { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Effect Volume ID:122 ValueRange 0-100
    /// </summary>
    [ProfileSetting(122)] public int EffectsVolume { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Dialog Volume ID:123 ValueRange 0-100
    /// </summary>
    [ProfileSetting(123)] public int DialogVolume { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Hit Volume ID:129 ValueRange 0-100
    /// </summary>
    [ProfileSetting(129)] public int HitVolume { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Damage Volume ID:130 ValueRange 0-100
    /// </summary>
    [ProfileSetting(130)] public int DamageVolume { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Stamina Audio Toggle ID:131
    /// </summary>
    [ProfileSetting(131)] public bool StaminaAudio { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// Emote Audio Toggle ID:133
    /// </summary>
    [ProfileSetting(133)] public bool EmoteAudio { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// Brightness ID:150 ValueRange 0-100
    /// </summary>
    [ProfileSetting(150)] public int Brightness { get { return GetValueOf(); } set { SetValueOf(value); } }
    /// <summary>
    /// Letterbox Toggle ID:152
    /// </summary>
    [ProfileSetting(152)] public bool Letterbox { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// WeatherEffects Toggle ID:153
    /// </summary>
    [ProfileSetting(153)] public bool WeatherEffects { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1:0); } }
    /// <summary>
    /// TightAim Blur ID:154
    /// </summary>
    [ProfileSetting(154)] public bool TightAimBlur { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// Decline Clan Invites ID:200 On=4 Off=0
    /// </summary>
    [ProfileSetting(200)] public int DeclineClanInvites { get { return GetValueOf(); } set { SetValueOf(Clamp(value, 0, 4)); } }
    /// <summary>
    /// ThirdPerson Taunts ID:680
    /// </summary>
    [ProfileSetting(680)] public bool ThirdPersonTaunts { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// Mouse Sensitivity ID:717 ValueRange 5000-105000 Base = 5000 (0=0+Base / 25=50000+Base / 50=100000+Base)
    /// Best to keep it at or above 1000 below it skips mouse input
    /// Ingame Settings Scale 2x
    /// </summary>
    [ProfileSetting(717)] public int MouseSensitivity { get { return GetValueOf(); } set { SetValueOf(Clamp(value, 1000, 105000)); } }
    /// <summary>
    /// Mouse Tight Aim (ADS) Sensitivity ID:718 ValueRange 1000-151000 Base = 1000 (0=0+Base / 25=75000+Base / 50=150000+Base)
    /// Best to keep it at or above 5000 below it skips mouse input
    /// Ingame Settings Scale 3x
    /// </summary>
    [ProfileSetting(718)] public int MouseTightAimSensitivity { get { return GetValueOf(); } set { SetValueOf(Clamp(value, 5000, 105000)); } }
    /// <summary>
    /// Swap Loadout With Movement Keys ID:721
    /// </summary>
    [ProfileSetting(721)] public bool SwapLoadoutWithMovementKeys { get { return GetValueOf() > 0; } set { SetValueOf(value ? 1 : 0); } }
    /// <summary>
    /// Friend & Clan Status ID:1201  4=Both Off 7=Both On 5=Clan Off 6=Friend Off
    /// </summary>
    [ProfileSetting(1201)] public int FriendClanStatus { get { return GetValueOf(); } set { SetValueOf(Clamp(value, 4, 7)); } }
    /// <summary>
    /// LeftStick Deadzone ID:1300 ValueRange 1500-55000 Base = ?? (0=0+Base / 25=75000+Base / 50=150000+Base)
    /// 
    /// Ingame Settings Scale 3x
    /// </summary>
    [ProfileSetting(1300)] public int LeftStickDeadzone { get { return GetValueOf(); } set { SetValueOf(Clamp(value, 5000, 105000)); } }
    /// <summary>
    /// LeftStick Deadzone ID:1301 ValueRange 1500-55000 Base = ?? (0=0+Base / 25=75000+Base / 50=150000+Base)
    /// 
    /// Ingame Settings Scale 3x
    /// </summary>
    [ProfileSetting(1301)] public int RightStickDeadzone { get { return GetValueOf(); } set { SetValueOf(Clamp(value, 5000, 105000)); } }
    #endregion Settings


}
