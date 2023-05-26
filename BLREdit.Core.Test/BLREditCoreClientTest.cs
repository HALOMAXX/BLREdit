using BLREdit.Core.API.REST.ServerUtils;
using BLREdit.Core.Models.BLR.Client;
using BLREdit.Core.Models.BLR.Server;
using BLREdit.Core.Utils;

namespace BLREdit.Core.Test;

[TestClass]
public class BLREditCoreClientTest
{
    const string FOLDER_NAME = "Clients";
    static string InputFolder = $"Data\\{FOLDER_NAME}";
    static string OutputFolder = $"Data\\{FOLDER_NAME}\\out";
    static readonly DirectoryInfo InputListFolder = new($"{InputFolder}\\List");
    static readonly DirectoryInfo OutputListFolder = new($"{OutputFolder}\\List");


    [TestMethod]
    public void DeserializeClientList()
    {
        List<BLRClient> clients = IOResources.DeserializeDirectory<BLRClient>(InputListFolder);

        Assert.IsTrue(clients.Any(), "No Clients got Deserialized");
    }

    [TestMethod]
    public void SerializeClientList()
    {
        List<BLRClient> clients = IOResources.DeserializeDirectory<BLRClient>(InputListFolder);
        IOResources.SerializeFile($"{OutputListFolder.FullName}\\List.json", clients);
        IOResources.SerializeFile($"{OutputListFolder.FullName}\\ListCompact.json", clients, true);
        List<BLRClient> check = IOResources.DeserializeDirectory<BLRClient>(OutputListFolder);

        Assert.IsTrue(clients.Count * 2 == check.Count, $"Source:{clients.Count}*2 = {clients.Count*2} and Output:{check.Count} are not the same");
        //TODO Compare Clients with each other after serialization
    }
}
