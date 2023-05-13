using DynamicData;

using System.Collections.Generic;
using System.IO;

namespace BLREdit.Models.BLR;

public sealed class BLRClient
{
    public static SourceList<BLRClient> Clients { get; } = new();
    public static Dictionary<string, string> VersionHashes => new()
    {
        {"0f4a732484f566d928c580afdae6ef01c002198dd7158cb6de29b9a4960064c7", "v302"},
        {"de08147e419ed89d6db050b4c23fa772338132587f6b533b6233733f9bce46c3", "v301"},
        {"1742df917761f9dc01b079ae2aad78ef2ff17562af1dad6ad6ea7cf3622fe7f6", "v300"},
        {"4032ed1c45e717757a280e4cfe2408bb0c4e366676b785f0ffd177c3054c13a5", "v140"},
        {"01890318303354f588d9b89bb1a34c5c49ff881d2515388fcc292b54eb036b58", "v130"},
        {"d4f9cec736a83f7930f04438344d35ff9f0e57212755974bd51f48ff89d303c4", "v120"},
        {"d0bc0ae14ab4dd9f407de400da4f333ee0b6dadf6d68b7504db3fc46c4baa59f", "v1100"},
        {"9200705daddbbc10fee56db0586a20df1abf4c57a9384a630c578f772f1bd116", "v0993"}
    };

    public string ClientVersion { get { if (VersionHashes.TryGetValue(OriginalFileHash ?? "", out string? version)) { return version; } else { return "Unknown"; } } }
    public string? OriginalFile { get; set; }
    public string? PatchedFile { get; set; }
    public string? OriginalFileHash { get; set; }
    public string? PatchedFileHash { get; set; }

    private string? _basePath;
    private string? _proxyConfigFolder;
    private string? _gameConfigFolder;
    private string? _proxyModuleFolder;
    private string? _proxyLogFolder;

    public string BasePath { get { _basePath ??= CreateBasePath(this); return _basePath; } set { _basePath = value; } }
    public string ProxyConfigFolder { get { _proxyConfigFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Config\\BLRevive\\").FullName; return _proxyConfigFolder; } }
    public string GameConfigFolder { get { _gameConfigFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Config\\PCConsole\\Cooked\\").FullName; return _gameConfigFolder; } }
    public string ProxyModuleFolder { get { _proxyModuleFolder ??= Directory.CreateDirectory($"{BasePath}Binaries\\Win32\\Modules\\").FullName; return _proxyModuleFolder; } }
    public string ProxyLogFolder { get { _proxyLogFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Logs\\").FullName; return _proxyLogFolder; } }
}
