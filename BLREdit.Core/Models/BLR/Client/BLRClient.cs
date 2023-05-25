using BLREdit.Core.Models.BLR.Item;
using BLREdit.Core.Utils;
using BLREdit.Models.BLReviveSDK;

using PeNet;

using PropertyChanged;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace BLREdit.Core.Models.BLR.Client;

[JsonConverter(typeof(JsonBLRClientConverter))]
public sealed class BLRClient : ModelBase
{
    public static DirectoryInfo ClientInfoLocation { get; }
    public static RangeObservableCollection<BLRClient> Clients { get; } = new();

    static BLRClient()
    {
        ClientInfoLocation = new DirectoryInfo("Data\\Clients");
        IOResources.DeserializeDirectoryInto(Clients, ClientInfoLocation);
    }

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

    [JsonIgnore] public string ClientVersion { get { if (VersionHashes.TryGetValue(OriginalFile?.FileHash ?? "", out var version)) { return version; } else { return "Unknown"; } } }
    public FileInfoBLR OriginalFile { get; set; }
    public FileInfoBLR PatchedFile { get; set; }

    private string? _proxyConfigFolder;
    private string? _gameConfigFolder;
    private string? _proxyModuleFolder;
    private string? _proxyLogFolder;

    [JsonIgnore] public string BasePath { get; }
    [JsonIgnore] public string ProxyConfigFolder { get { _proxyConfigFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Config\\BLRevive\\").FullName; return _proxyConfigFolder; } }
    [JsonIgnore] public string GameConfigFolder { get { _gameConfigFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Config\\PCConsole\\Cooked\\").FullName; return _gameConfigFolder; } }
    [JsonIgnore] public string ProxyModuleFolder { get { _proxyModuleFolder ??= Directory.CreateDirectory($"{BasePath}Binaries\\Win32\\Modules\\").FullName; return _proxyModuleFolder; } }
    [JsonIgnore] public string ProxyLogFolder { get { _proxyLogFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Logs\\").FullName; return _proxyLogFolder; } }

    public RangeObservableCollection<BLRClientPatch> InstalledPatches { get; set; } = new();
    public RangeObservableCollection<BLReviveSDKPlugin> InstalledPlugins { get; set; } = new();

    [JsonConstructor]
    public BLRClient(FileInfoBLR originalFile)
    {
        OriginalFile = originalFile;
        if (string.IsNullOrEmpty(OriginalFile.FileHash)) { OriginalFile.UpdateFileHash(); }
        var dirInfo = OriginalFile.Info?.FullName.Split('\\');
        string basePath = "";
        for (int i = dirInfo?.Length-4 ?? 0; i >= 0; i--)
        {
            basePath = $"{dirInfo?[i]}\\{basePath}";
        }
        BasePath = basePath;
        PatchedFile = new($"{BasePath}Binaries\\Win32\\{OriginalFile.Name}-BLREdit-Patched{OriginalFile.Extension}");
    }

    public void BinaryPatchClient(RangeObservableCollection<BLRClientPatch>? patches = null)
    {
        patches ??= BLRClientPatch.ClientPatches[OriginalFile.FileHash] ?? new();
        OriginalFile.Info.CopyTo(PatchedFile.FullName, true);
        using var stream = PatchedFile.Info.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        byte[] rawFile = new byte[stream.Length];
        stream.Read(rawFile ,0,(int)stream.Length);
        stream.Position = 0;

        foreach (var patch in patches)
        {
            foreach (var part in patch.PatchParts)
            {
                OverwriteBytes(rawFile, part.Position, part.BytesToOverwrite);
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
        PatchedFile.UpdateFileHash();
    }

    private static void OverwriteBytes(byte[] bytes, int offsetFromBegining, IEnumerable<byte> bytesToWrite)
    {
        int i2 = 0;
        for (int i = offsetFromBegining; i < bytes.Length && i2 < bytesToWrite.Count(); i++)
        {
            bytes[i] = bytesToWrite.ElementAt(i2);
            i2++;
        }
    }

}

public class JsonBLRClientConverter : JsonGenericConverter<BLRClient> { }