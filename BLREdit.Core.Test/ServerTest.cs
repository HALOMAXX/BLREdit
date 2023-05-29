using BLREdit.Core.API.REST.ServerUtils;
using BLREdit.Core.Models.BLR.Server;
using BLREdit.Core.Utils;

namespace BLREdit.Core.Test;

[TestClass]
public class ServerTest
{
    const string FOLDER_NAME = "Servers";
    static string InputFolder = $"Data\\{FOLDER_NAME}";
    static string OutputFolder = $"Data\\{FOLDER_NAME}\\out";
    static readonly DirectoryInfo InputListFolder = new($"{InputFolder}\\List");
    static readonly DirectoryInfo OutputListFolder = new($"{OutputFolder}\\List");
    static readonly DirectoryInfo InputInfoFolder = new($"{InputFolder}\\Info");
    static readonly DirectoryInfo OutputInfoFolder = new($"{OutputFolder}\\Info");

    [TestMethod]
    public void SerializeServerList()
    {
        Assert.IsTrue(BLRServer.Servers.Any(), "No Servers got Deserialized");

        IOResources.SerializeFile($"{OutputListFolder.FullName}\\List.json", BLRServer.Servers);
        IOResources.SerializeFile($"{OutputListFolder.FullName}\\ListCompact.json", BLRServer.Servers, true);
        List<BLRServer> check = IOResources.DeserializeDirectory<BLRServer>(OutputListFolder);

        Assert.IsTrue(BLRServer.Servers.Count * 2 == check.Count, $"Source:{BLRServer.Servers.Count}*2 = {BLRServer.Servers.Count * 2} and Output:{check.Count} are not the same");

        for (int i = 0; i < BLRServer.Servers.Count; i++)
        {
            Assert.IsTrue(BLRServer.Servers[i].Equals(check[i]), $"Source[{i}] != Serialized[{i}]");
            Assert.IsTrue(BLRServer.Servers[i].Equals(check[i + BLRServer.Servers.Count]), $"Source[{i}] != Serialized[{i + BLRServer.Servers.Count}](Compact)");
        }
    }


    [TestMethod]
    public void SerializeServerInfo()
    {
        List<ServerInfo> infos = IOResources.DeserializeDirectory<ServerInfo>(InputInfoFolder);

        Assert.IsTrue(infos.Any(), "Server Info List was Empty!");
        Assert.IsTrue(infos.Count == 2, "Server Info List has more or less entries then expected");
        Assert.IsTrue(infos[0].Equals(infos[1]), "Server Info is not Equal");

        IOResources.SerializeFile($"{OutputInfoFolder.FullName}\\List.json", infos);
        IOResources.SerializeFile($"{OutputInfoFolder.FullName}\\ListCompact.json", infos, true);
        List<ServerInfo> check = IOResources.DeserializeDirectory<ServerInfo>(OutputInfoFolder);

        Assert.IsTrue(infos.Count * 2 == check.Count, $"Source:{infos.Count}*2 = {infos.Count * 2} and Output:{check.Count} are not the same");
        for (int i = 0; i < infos.Count; i++)
        {
            Assert.IsTrue(infos[i].Equals(check[i]), $"Source[{i}] != Serialized[{i}]");
            Assert.IsTrue(infos[i].Equals(check[i + infos.Count]), $"Source[{i}] != Serialized[{i + infos.Count}](Compact)");
        }
    }
}
