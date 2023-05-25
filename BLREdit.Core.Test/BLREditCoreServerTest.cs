using BLREdit.Core.API.REST.ServerUtils;
using BLREdit.Core.Models.BLR.Server;
using BLREdit.Core.Utils;

namespace BLREdit.Core.Test;

[TestClass]
public class BLREditCoreServerTest
{
    [TestMethod]
    public void DeserializeServerList()
    {
        List<BLRServer>? servers = IOResources.DeserializeFile<List<BLRServer>>("Data\\Servers\\List\\ServerList.json");

        Assert.IsNotNull(servers, "Server List was null");
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
    public void DeserializeServerInfo()
    {
        List<ServerInfo>? infos = IOResources.DeserializeDirectory<ServerInfo>(new DirectoryInfo("Data\\Servers\\Info"));

        Assert.IsNotNull(infos, "Server Info List was null!");
        Assert.IsTrue(infos.Any(), "Server Info List was Empty!");
        Assert.IsTrue(infos.Count == 2, "Server Info List has more or less entries then expected");
        Assert.IsTrue(infos[0].Equals(infos[1]), "Server Info is not Equal");
    }
}
