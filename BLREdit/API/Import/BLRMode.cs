namespace BLREdit.Import;

public sealed class BLRMode
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string PlaylistName { get; set; } = "";
    public string ModeName { get; set; } = "";
    public bool IsTeammode { get; set; } = true;
    public bool IsAvailable { get; set; } = true;

    public static BLRMode? FindPlaylistName(string modeName)
    {
        if (string.IsNullOrEmpty(modeName)) return null;
        foreach (var mode in DataStorage.Modes)
        {
            if (mode.PlaylistName == modeName) return mode;
        }
        return null;
    }
}
