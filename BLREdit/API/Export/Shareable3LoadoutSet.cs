using BLREdit.UI;
using BLREdit.UI.Views;

using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.Export;

public sealed class Shareable3LoadoutSet : IBLRProfile
{
    [JsonPropertyName("A")] public UIBool IsAdvanced { get; set; } = new(false);
    [JsonPropertyName("L1")] public ShareableLoadout Loadout1 { get; set; } = new();
    [JsonPropertyName("L2")] public ShareableLoadout Loadout2 { get; set; } = new();
    [JsonPropertyName("L3")] public ShareableLoadout Loadout3 { get; set; } = new();

    public Shareable3LoadoutSet() { }
    public Shareable3LoadoutSet(BLRProfile profile)
    {
        if (profile is null) { LoggingSystem.FatalLog("profile was null in Shareable3LoadoutSet constructor"); return; }
        IsAdvanced.Set(profile.IsAdvanced.Is);
        Loadout1 = new ShareableLoadout(profile.Loadout1);
        Loadout2 = new ShareableLoadout(profile.Loadout2);
        Loadout3 = new ShareableLoadout(profile.Loadout3);
    }

    public event EventHandler? WasWrittenTo;

    public BLRProfile ToBLRProfile()
    {
        var profile = new BLRProfile();
        profile.IsAdvanced.Set(IsAdvanced.Is);
        profile.Loadout1 = Loadout1.ToBLRLoadout(profile);
        profile.Loadout2 = Loadout2.ToBLRLoadout(profile);
        profile.Loadout3 = Loadout3.ToBLRLoadout(profile);
        return profile;
    }

    public IBLRLoadout GetLoadout(int index)
    {
        return index switch
        {
            1 => Loadout2,
            2 => Loadout3,
            _ => Loadout1,
        };
    }

    public void Read(BLRProfile profile)
    {
        if (profile is null) return;
        LoggingSystem.ResetWatch();
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.ReadProfile)) return;
        UndoRedoSystem.CurrentlyBlockedEvents.Value = BlockEvents.All;
        profile.IsAdvanced.Set(IsAdvanced.Is);
        Loadout1.Read(profile.Loadout1);
        Loadout2.Read(profile.Loadout2);
        Loadout3.Read(profile.Loadout3);
        UndoRedoSystem.RestoreBlockedEvents();
        LoggingSystem.PrintElapsedTime("Profile Read took {0}ms (3L)");
    }

    public void Write(BLRProfile profile)
    {
        if (profile is null) return;
        LoggingSystem.ResetWatch();
        if (UndoRedoSystem.CurrentlyBlockedEvents.Value.HasFlag(BlockEvents.WriteProfile)) return;
        IsAdvanced.Set(profile.IsAdvanced.Is);
        Loadout1.Write(profile.Loadout1);
        Loadout2.Write(profile.Loadout2);
        Loadout3.Write(profile.Loadout3);
        if (WasWrittenTo is not null && !UndoRedoSystem.UndoRedoSystemWorking) { WasWrittenTo(profile, EventArgs.Empty); }
        LoggingSystem.PrintElapsedTime("Profile Write took {0}ms (3L)");
    }
}
