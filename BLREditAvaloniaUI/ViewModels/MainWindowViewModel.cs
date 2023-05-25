using BLREdit.Core.Models.BLR.Client;
using BLREdit.Core.Models.BLR.Item;
using BLREdit.Core.Models.BLR.Server;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.ObjectModel;
using System.Reactive;

namespace BLREdit.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    [Reactive] public string WindowTitle { get; set; } = "BLREdit";
    public RangeObservableCollection<BLRClient> Clients { get { return BLRClient.Clients; } }
    public RangeObservableCollection<BLRServer> Servers { get { return BLRServer.Servers; } }
    public RangeObservableCollection<BLRItem> Primaries { get { return BLRItemList.ItemLists["0f4a732484f566d928c580afdae6ef01c002198dd7158cb6de29b9a4960064c7"].Categories[19]; } }
    public ReactiveCommand<Unit, Unit> ButtonCommand { get; set; }

    public MainWindowViewModel() 
    {
        ButtonCommand = ReactiveCommand.Create(() => { WindowTitle = Random.Shared.NextDouble().ToString(); foreach (var client in BLRClient.Clients) { client.OriginalFile.UpdateFileHash(); } });
    }
}