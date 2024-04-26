using BLREdit.Export;
using BLREdit.UI;
using BLREdit.UI.Views;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BLREdit.API.Export;

public sealed class ShareableProfile(string name = "New Profile") : INotifyPropertyChanged, IBLRProfile
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? WasWrittenTo;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    [JsonIgnore] private string name = name;
    public string Name { get { return name; } set { name = value; OnPropertyChanged(); } }
    public UIBool IsAdvanced { get; set; } = new(false);
    [JsonIgnore] public int TimeOfCreation { get; set; }
    public DateTime LastApplied { get; set; } = DateTime.MinValue;
    public DateTime LastModified { get; set; } = DateTime.MinValue;
    public DateTime LastViewed { get; set; } = DateTime.MinValue;
    public ObservableCollection<ShareableLoadout> Loadouts { get; set; } = [MagiCowsLoadout.DefaultLoadout1.ConvertToShareable(), MagiCowsLoadout.DefaultLoadout2.ConvertToShareable(), MagiCowsLoadout.DefaultLoadout3.ConvertToShareable()];
    

    private static readonly Regex CopyWithCount = new(@".* - Copy \([0-9]*\)$");
    private static readonly Regex CopyWithoutCount = new(@".* - Copy$");

    public void RegisterWithChildren()
    {
        foreach (var loadout in Loadouts)
        {
            loadout.RegisterWithChildren();
        }
    }

    public void RefreshInfo()
    {
        OnPropertyChanged(nameof(Name));
    }

    public BLRProfile ToBLRProfile()
    {
        var profile = new BLRProfile();
        profile.SetProfile(this, true);
        profile.Read();
        profile.CalculateStats();
        return profile;
    }

    public ShareableProfile Clone()
    {
        string name = Name;

        if (CopyWithCount.IsMatch(Name))
        {
            var lastOpeningBracketIndex = Name.LastIndexOf('(');
            var lastClosingBracketIndex = Name.LastIndexOf(')');
            var numberPartOfName = Name.Substring(lastOpeningBracketIndex+1, lastClosingBracketIndex - (lastOpeningBracketIndex+1));
            var copyNumber = int.Parse(numberPartOfName);
            var cutName = Name.Substring(0, lastOpeningBracketIndex);
            name = cutName + $"({++copyNumber})";
        }
        else if (CopyWithoutCount.IsMatch(Name))
        {
            name += " (1)";
        }
        else
        {
            name += " - Copy";
        }
        

        var dup = new ShareableProfile()
        {
            Name = name,
        };
        dup.Loadouts.Clear();
        foreach (var loadout in Loadouts)
        {
            dup.Loadouts.Add(loadout.Clone());
        }
        return dup;
    }

    public ShareableProfile Duplicate()
    {
        var dup = this.Clone();
        //BLRLoadoutStorage.AddNewLoadoutSet("", null, dup);
        return dup;
    }

    public IBLRLoadout GetLoadout(int index)
    {
        if (Loadouts.Count > index)
        { return Loadouts[index]; }
        else
        { return Loadouts[0]; }
    }

    public void Read(BLRProfile profile)
    {
        LoggingSystem.ResetWatch();
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.ReadProfile)) return;
        UndoRedoSystem.CurrentlyBlockedEvents.Value = BlockEvents.All & ~BlockEvents.ReadAll;
        profile.IsAdvanced.Set(IsAdvanced.Is);
        Loadouts[0].Read(profile.Loadout1);
        Loadouts[1].Read(profile.Loadout2);
        Loadouts[2].Read(profile.Loadout3);
        UndoRedoSystem.RestoreBlockedEvents();
        LoggingSystem.PrintElapsedTime($"Profile Read took {"{0}"}ms ({Name})");
    }

    public void Write(BLRProfile profile)
    {
        LoggingSystem.ResetWatch();
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteProfile)) return;
        LastModified = DateTime.Now;
        IsAdvanced.Set(profile.IsAdvanced.Is);
        Loadouts[0].Write(profile.Loadout1);
        Loadouts[1].Write(profile.Loadout2);
        Loadouts[2].Write(profile.Loadout3);
        if (WasWrittenTo is not null && !UndoRedoSystem.UndoRedoSystemWorking) { WasWrittenTo(profile, EventArgs.Empty); }
        LoggingSystem.PrintElapsedTime($"Profile Write took {"{0}"}ms ({Name})");
    }
}