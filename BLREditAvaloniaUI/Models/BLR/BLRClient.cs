using DynamicData;

using PeNet;

using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;

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
    public string OriginalFile { get; set; }
    public string PatchedFile { get; set; }
    public string OriginalFileHash { get; set; }
    public string PatchedFileHash { get; set; }

    private FileInfo? _orignalFileInfo;
    private FileInfo? _patchedFileInfo;
    [JsonIgnore] public FileInfo OriginalFileInfo { get { _orignalFileInfo ??= new FileInfo(OriginalFile); return _orignalFileInfo; } }
    [JsonIgnore] public FileInfo PatchedFileInfo { get { _patchedFileInfo ??= new FileInfo(PatchedFile); return _patchedFileInfo; } }

    private string? _proxyConfigFolder;
    private string? _gameConfigFolder;
    private string? _proxyModuleFolder;
    private string? _proxyLogFolder;

    public string BasePath { get; }
    [JsonIgnore] public string ProxyConfigFolder { get { _proxyConfigFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Config\\BLRevive\\").FullName; return _proxyConfigFolder; } }
    [JsonIgnore] public string GameConfigFolder { get { _gameConfigFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Config\\PCConsole\\Cooked\\").FullName; return _gameConfigFolder; } }
    [JsonIgnore] public string ProxyModuleFolder { get { _proxyModuleFolder ??= Directory.CreateDirectory($"{BasePath}Binaries\\Win32\\Modules\\").FullName; return _proxyModuleFolder; } }
    [JsonIgnore] public string ProxyLogFolder { get { _proxyLogFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Logs\\").FullName; return _proxyLogFolder; } }

    public List<BLRClientPatch> InstalledPatches { get; set; } = new();
    public List<object> InstalledModules { get; set; } = new();

    [JsonConstructor]
    public BLRClient(string originalFile, string? originalFileHash = null, string? patchedFileHash = null)
    {
        OriginalFile = originalFile;
        var dirInfo = OriginalFileInfo.Directory?.FullName.Split('\\');
        string basePath = "";
        for (int i = dirInfo?.Length-4 ?? 0; i >= 0; i--)
        {
            basePath = $"{dirInfo?[i]}\\{basePath}";
        }
        BasePath = basePath;
        PatchedFile = $"{BasePath}\\Binaries\\Win32\\{OriginalFileInfo.Name}-BLREdit-Patched.{OriginalFileInfo.Extension}";

        if (originalFileHash is null)
        {
            OriginalFileHash = IOResources.CreateFileHash(originalFile);
        }
        else
        {
            OriginalFileHash = originalFileHash;
        }

        if (patchedFileHash is null)
        {
            PatchedFileHash = "";
        }
        else
        { 
            PatchedFileHash = patchedFileHash;
        }
    }

    public void BinaryPatchClient(List<BLRClientPatch>? patches = null)
    {
        patches ??= BLRClientPatch.ClientBinaryPatches[OriginalFileHash] ?? new();
        File.Copy(OriginalFile, PatchedFile, true);
        using var stream = File.Open(PatchedFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        byte[] rawFile = new byte[stream.Length];
        stream.Read(rawFile ,0,(int)stream.Length);
        stream.Position = 0;

        foreach (var patch in patches)
        {
            foreach (var part in patch.PatchParts)
            {
                OverwriteBytes(rawFile, part.Key, part.Value.ToArray());
            }
        }

        PeFile peFile = new(rawFile);
        peFile.AddImport("Proxy.dll", "InitializeThread");
        stream.SetLength(peFile.RawFile.Length);
        stream.Write(peFile.RawFile.ToArray());
        stream.Flush();
        stream.Close();
        InstalledPatches.Clear();
        InstalledPatches.AddRange(patches);
        PatchedFileHash = IOResources.CreateFileHash(PatchedFile);
    }

    private static void OverwriteBytes(byte[] bytes, int offsetFromBegining, byte[] bytesToWrite)
    {
        int i2 = 0;
        for (int i = offsetFromBegining; i < bytes.Length && i2 < bytesToWrite.Length; i++)
        {
            bytes[i] = bytesToWrite[i2];
            i2++;
        }
    }

}
