using BLREdit.Core.Utils;
using BLREdit.Models.BLReviveSDK;

using PeNet;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace BLREdit.Core.Models.BLR.Client;

public sealed class BLRClient : ModelBase
{
    public static DirectoryInfo ClientInfoLocation { get; }
    public static RangeObservableCollection<BLRClient> Clients { get; } = new();

    static BLRClient()
    {
        ClientInfoLocation = new DirectoryInfo("Data\\Clients\\List");
        IOResources.DeserializeDirectoryInto(Clients, ClientInfoLocation);
    }

    public static void Save()
    {
        Debug.WriteLine($"Saving Clients:{Clients.Count}");
        IOResources.SerializeFile($"{ClientInfoLocation.FullName}\\List.json", Clients);
    }

    public static string HashToVersion(string? hash)
    {
        return hash switch
        {
            "0f4a732484f566d928c580afdae6ef01c002198dd7158cb6de29b9a4960064c7" => "v302",
            "de08147e419ed89d6db050b4c23fa772338132587f6b533b6233733f9bce46c3" => "v301",
            "1742df917761f9dc01b079ae2aad78ef2ff17562af1dad6ad6ea7cf3622fe7f6" => "v300",
            "4032ed1c45e717757a280e4cfe2408bb0c4e366676b785f0ffd177c3054c13a5" => "v140",
            "01890318303354f588d9b89bb1a34c5c49ff881d2515388fcc292b54eb036b58" => "v130",
            "d4f9cec736a83f7930f04438344d35ff9f0e57212755974bd51f48ff89d303c4" => "v120",
            "d0bc0ae14ab4dd9f407de400da4f333ee0b6dadf6d68b7504db3fc46c4baa59f" => "v1100",
            "9200705daddbbc10fee56db0586a20df1abf4c57a9384a630c578f772f1bd116" => "v0993",
            _ => "Unknown",
        };
    }

    public static string VersionToHash(string? version)
    {
        return version switch
        {
            "v302" => "0f4a732484f566d928c580afdae6ef01c002198dd7158cb6de29b9a4960064c7",
            "v301" => "de08147e419ed89d6db050b4c23fa772338132587f6b533b6233733f9bce46c3",
            "v300" => "1742df917761f9dc01b079ae2aad78ef2ff17562af1dad6ad6ea7cf3622fe7f6",
            "v140" => "4032ed1c45e717757a280e4cfe2408bb0c4e366676b785f0ffd177c3054c13a5",
            "v130" => "01890318303354f588d9b89bb1a34c5c49ff881d2515388fcc292b54eb036b58",
            "v120" => "d4f9cec736a83f7930f04438344d35ff9f0e57212755974bd51f48ff89d303c4",
            "v1100" => "d0bc0ae14ab4dd9f407de400da4f333ee0b6dadf6d68b7504db3fc46c4baa59f",
            "v0993" => "9200705daddbbc10fee56db0586a20df1abf4c57a9384a630c578f772f1bd116",
            _ => "Unknown",
        };
    }

    #region Overrides
    public override bool Equals(object? obj)
    {
        if (obj is BLRClient client)
        {
            return
                OriginalFile.Equals(client.OriginalFile) &&
                PatchedFile.Equals(client.PatchedFile) &&
                InstalledPatches.Equals(client.InstalledPatches) &&
                InstalledPlugins.Equals(client.InstalledPlugins);
        }
        return false;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(OriginalFile);
        hash.Add(PatchedFile);
        hash.Add(InstalledPatches);
        hash.Add(InstalledPlugins);

        return hash.ToHashCode();
    }
    #endregion Overrides

    [JsonIgnore] public string ClientVersion { get { return HashToVersion(OriginalFile?.FileHash); } }
    private FileInfoBLR _originalFile;
    private FileInfoBLR? _patchedFile;

    public FileInfoBLR OriginalFile { get { return _originalFile; } set { _originalFile = value; } }
    public FileInfoBLR PatchedFile { get { _patchedFile ??= GetPatchedFilePath(); return _patchedFile; } set { _patchedFile = value; } }

    private string? _basePath;
    private string? _proxyConfigFolder;
    private string? _gameConfigFolder;
    private string? _proxyModuleFolder;
    private string? _proxyLogFolder;


    [JsonIgnore] public string BasePath { get { _basePath ??= GetBasePath(); return _basePath; } }
    [JsonIgnore] public string ProxyConfigFolder { get { _proxyConfigFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Config\\BLRevive\\").FullName; return _proxyConfigFolder; } }
    [JsonIgnore] public string GameConfigFolder { get { _gameConfigFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Config\\PCConsole\\Cooked\\").FullName; return _gameConfigFolder; } }
    [JsonIgnore] public string ProxyModuleFolder { get { _proxyModuleFolder ??= Directory.CreateDirectory($"{BasePath}Binaries\\Win32\\Modules\\").FullName; return _proxyModuleFolder; } }
    [JsonIgnore] public string ProxyLogFolder { get { _proxyLogFolder ??= Directory.CreateDirectory($"{BasePath}FoxGame\\Logs\\").FullName; return _proxyLogFolder; } }

    public RangeObservableCollection<BLRClientPatch> InstalledPatches { get; set; } = new();
    public RangeObservableCollection<BLReviveSDKPlugin> InstalledPlugins { get; set; } = new();

    [JsonConstructor]
    public BLRClient(FileInfoBLR originalFile)
    {
        _originalFile = originalFile;
        if (string.IsNullOrEmpty(OriginalFile.FileHash)) { OriginalFile.UpdateFileHash(); }
    }

    private string GetBasePath()
    {
        var dirInfo = OriginalFile.Info?.FullName.Split('\\');
        string basePath = "";
        for (int i = dirInfo?.Length - 4 ?? 0; i >= 0; i--)
        {
            basePath = $"{dirInfo?[i]}\\{basePath}";
        }
        return basePath;
    }

    private FileInfoBLR GetPatchedFilePath()
    {
        return new($"{BasePath}Binaries\\Win32\\{OriginalFile.Name}-BLREdit-Patched{OriginalFile.Extension}");
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
            if (patch.ShouldBeInstalled)
            {
                foreach (var part in patch.PatchParts)
                {
                    OverwriteBytes(rawFile, part.Position, part.BytesToOverwrite);
                }
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

public sealed class JsonBLRClientConverter : JsonGenericConverter<BLRClient> 
{
    static JsonBLRClientConverter()
    {
        Default = new BLRClient(new FileInfoBLR("test.test"));
    }
}