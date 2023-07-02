using BLREdit.Core.API.REST.ServerUtils;
using BLREdit.Core.Models.BLR.Client;
using BLREdit.Core.Models.BLR.Server;
using BLREdit.Core.Utils;

namespace BLREdit.Core.Test;

[TestClass]
public class ClientTest
{
    const string FOLDER_NAME = "Clients";
    static readonly string OutputFolder = $"Data\\{FOLDER_NAME}\\out";
    static readonly DirectoryInfo OutputListFolder = new($"{OutputFolder}\\List");

    [TestMethod]
    public void SerializeClientList()
    {
        Assert.IsTrue(BLRClient.Clients.Any(), "No Clients got Deserialized");

        IOResources.SerializeFile($"{OutputListFolder.FullName}\\List.json", BLRClient.Clients);
        IOResources.SerializeFile($"{OutputListFolder.FullName}\\ListCompact.json", BLRClient.Clients, true);
        List<BLRClient> check = IOResources.DeserializeDirectory<BLRClient>(OutputListFolder);

        Assert.IsTrue(BLRClient.Clients.Count * 2 == check.Count, $"Source:{BLRClient.Clients.Count}*2 = {BLRClient.Clients.Count*2} and Output:{check.Count} are not the same");

        for (int i = 0; i < BLRClient.Clients.Count; i++)
        {
            Assert.IsTrue(BLRClient.Clients[i].Equals(check[i]), $"Source[{i}] != Serialized[{i}]");
            Assert.IsTrue(BLRClient.Clients[i].Equals(check[i + BLRClient.Clients.Count]), $"Source[{i}] != Serialized[{i + BLRClient.Clients.Count}](Compact)");
        }
    }
}
