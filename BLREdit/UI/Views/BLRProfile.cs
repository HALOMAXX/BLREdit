﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BLREdit.UI.Views;

public sealed class BLRProfile
{
    private IBLRProfile? _profile;

    #region Event
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion Event

    public UIBool IsAdvanced { get; set; } = new UIBool(false);
    public BLREditLoadout Loadout1 { get; set; }
    public BLREditLoadout Loadout2 { get; set; }
    public BLREditLoadout Loadout3 { get; set; }


    private bool isChanged;
    [JsonIgnore] public bool IsChanged { 
        get { return isChanged; } 
        set { isChanged = value; OnPropertyChanged(); } }

    public BLRProfile() {
        IsAdvanced.PropertyChanged += AdvancedChanged;
        
        Loadout1 = new(this);
        Loadout2 = new(this);
        Loadout3 = new(this);

        Loadout1.PropertyChanged += LoadoutChanged;
        Loadout2.PropertyChanged += LoadoutChanged;
        Loadout3.PropertyChanged += LoadoutChanged;

        Loadout1.Primary.PropertyChanged += LoadoutChanged;
        Loadout1.Secondary.PropertyChanged += LoadoutChanged;
        Loadout2.Primary.PropertyChanged += LoadoutChanged;
        Loadout2.Secondary.PropertyChanged += LoadoutChanged;
        Loadout3.Primary.PropertyChanged += LoadoutChanged;
        Loadout3.Secondary.PropertyChanged += LoadoutChanged;
    }

    void AdvancedChanged(object sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName == nameof(IsAdvanced.Is))
            IsChanged = true;
    }

    void LoadoutChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsChanged))
            IsChanged = true;
    }

    public static void UpdateSearchAndFilter()
    {
        MainWindow.Instance?.ApplySearchAndFilter();
    }

    public void CalculateStats()
    {
        Loadout1.CalculateStats();
        Loadout2.CalculateStats();
        Loadout3.CalculateStats();
    }

    public void SetProfile(IBLRProfile? profile, bool registerReadBackEvent = false)
    {
        if (_profile is not null) { _profile.WasWrittenTo -= ReadCallback; }
        _profile = profile;
        if (_profile is not null)
        {
            Loadout1.SetLoadout(_profile.GetLoadout(0), registerReadBackEvent);
            Loadout2.SetLoadout(_profile.GetLoadout(1), registerReadBackEvent);
            Loadout3.SetLoadout(_profile.GetLoadout(2), registerReadBackEvent);
            if (registerReadBackEvent) { _profile.WasWrittenTo += ReadCallback; }
        }
        else 
        {
            Loadout1.SetLoadout(null, registerReadBackEvent);
            Loadout2.SetLoadout(null, registerReadBackEvent);
            Loadout3.SetLoadout(null, registerReadBackEvent);
        }
    }

    private void ReadCallback(object sender, EventArgs e)
    {
        if (sender != this)
        {
            Read();
        }
    }

    public void Write()
    {
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteProfile)) return;
        _profile?.Write(this);
    }

    public void Write(IBLRProfile profile)
    {
        if (profile == null) { LoggingSystem.FatalLog("IBLRProfile is null"); return; }
        profile.Write(this);
    }

    public void Read()
    {
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.ReadProfile)) return;
        _profile?.Read(this);
    }

    public void Read(IBLRProfile profile)
    {
        if (profile == null) { LoggingSystem.FatalLog("IBLRProfile is null"); return; }
        profile.Read(this);
    }
}

public interface IBLRProfile
{
    public event EventHandler? WasWrittenTo;
    public IBLRLoadout GetLoadout(int index);
    public void Read(BLRProfile profile);
    public void Write(BLRProfile profile);
}

public interface IBLRLoadout
{
    public event EventHandler? WasWrittenTo;
    public IBLRWeapon GetPrimaryWeaponInterface();
    public IBLRWeapon GetSecondaryWeaponInterface();
    public void Read(BLREditLoadout loadout);
    public void Write(BLREditLoadout loadout);
}

public interface IBLRWeapon
{
    public event EventHandler? WasWrittenTo;
    public void Read(BLREditWeapon weapon);
    public void Write(BLREditWeapon weapon);
}