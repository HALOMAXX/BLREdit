using BLREdit.Core.API.REST.ServerUtils;
using BLREdit.Core.Models.BLR.Server;
using BLREdit.Core.Utils;

namespace BLREdit.Core.Test;

[TestClass]
public class BLREditCoreServerTest
{
    const string FOLDER_NAME = "Servers";
    static string InputFolder = $"Data\\{FOLDER_NAME}";
    static string OutputFolder = $"Data\\{FOLDER_NAME}\\out";
    static readonly DirectoryInfo InputListFolder = new($"{InputFolder}\\List");
    static readonly DirectoryInfo OutputListFolder = new($"{OutputFolder}\\List");
    static readonly DirectoryInfo InputInfoFolder = new($"{InputFolder}\\Info");
    static readonly DirectoryInfo OutputInfoFolder = new($"{OutputFolder}\\Info");

    [TestMethod]
    public void DeserializeServerList()
    {
        List<BLRServer> servers = IOResources.DeserializeDirectory<BLRServer>(InputListFolder);

        Assert.IsTrue(servers.Any(), "No Servers got Deserialized");
        Assert.IsTrue(servers.Count == 9, "Not all Servers got Deserialized");
        Assert.IsTrue(servers[0].Equals(new BLRServer("mooserver.ddns.net")), "DM mooserver.ddns.net failed!");
        Assert.IsTrue(servers[1].Equals(new BLRServer("mooserver.ddns.net") { ServerPort = 7779, InfoPort = 7780 }), "OS mooserver.ddns.net failed!");
        Assert.IsTrue(servers[2].Equals(new BLRServer("blrevive.northamp.fr") { ServerPort = 7777, InfoPort = 80 }), "blrevive.northamp.fr failed!");
        Assert.IsTrue(servers[3].Equals(new BLRServer("aegiworks.com")), "aegiworks.com failed!");
        Assert.IsTrue(servers[4].Equals(new BLRServer("kameron.cloud")), "kameron.cloud failed!");
        Assert.IsTrue(servers[5].Equals(new BLRServer("blr.akoot.me")), "blr.akoot.me failed");
        Assert.IsTrue(servers[6].Equals(new BLRServer("blr.753z.net")), "blr.753z.net failed!");
        Assert.IsTrue(servers[7].Equals(new BLRServer("localhost")), "localhost failed!");
        Assert.IsTrue(servers[8].Equals(new BLRServer("127.0.0.1")), "127.0.0.1 failed!");
    }

    [TestMethod]
    public void SerializeServerList()
    {
        List<BLRServer> servers = IOResources.DeserializeDirectory<BLRServer>(InputListFolder);
        IOResources.SerializeFile($"{OutputListFolder.FullName}\\List.json", servers);
        IOResources.SerializeFile($"{OutputListFolder.FullName}\\ListCompact.json", servers, true);
        List<BLRServer> check = IOResources.DeserializeDirectory<BLRServer>(OutputListFolder);

        Assert.IsTrue(servers.Count * 2 == check.Count, $"Source:{servers.Count}*2 = {servers.Count * 2} and Output:{check.Count} are not the same");
    }


    [TestMethod]
    public void DeserializeServerInfo()
    {
        List<ServerInfo> infos = IOResources.DeserializeDirectory<ServerInfo>(InputInfoFolder);

        Assert.IsTrue(infos.Any(), "Server Info List was Empty!");
        Assert.IsTrue(infos.Count == 2, "Server Info List has more or less entries then expected");
        Assert.IsTrue(infos[0].Equals(infos[1]), "Server Info is not Equal");
    }



    [TestMethod]
    public void SerializeServerInfo()
    {
        List<ServerInfo> infos = IOResources.DeserializeDirectory<ServerInfo>(InputInfoFolder);
        IOResources.SerializeFile($"{OutputInfoFolder.FullName}\\List.json", infos);
        IOResources.SerializeFile($"{OutputInfoFolder.FullName}\\ListCompact.json", infos, true);
        List<ServerInfo> check = IOResources.DeserializeDirectory<ServerInfo>(OutputInfoFolder);

        Assert.IsTrue(infos.Count * 2 == check.Count, $"Source:{infos.Count}*2 = {infos.Count * 2} and Output:{check.Count} are not the same");
    }
}
