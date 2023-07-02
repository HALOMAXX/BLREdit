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
    public static RangeObservableCollection<BLRClient> Clients { get { return BLRClient.Clients; } }
    public static RangeObservableCollection<BLRServer> Servers { get { return BLRServer.Servers; } }
    public static RangeObservableCollection<BLRItem> Primaries { get { return BLRItemList.ItemLists["v302"].Categories[19]; } }
    public ReactiveCommand<Unit, Unit> ButtonCommand { get; set; }

    public MainWindowViewModel() 
    {
        ButtonCommand = ReactiveCommand.Create(() => { WindowTitle = Random.Shared.NextDouble().ToString(); foreach (var client in BLRClient.Clients) { client.OriginalFile.UpdateFileHash(); } });
    }
}