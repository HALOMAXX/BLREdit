using BLREdit.Models;
using BLREdit.Models.BLR;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;

namespace BLREdit.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    [Reactive] public string WindowTitle { get; set; } = "BLREdit";
    public RangeObservableCollection<BLRClient> Clients { get { return BLRClient.Clients; } }
    public ReactiveCommand<Unit, Unit> ButtonCommand { get; set; }

    public MainWindowViewModel() 
    {
        ButtonCommand = ReactiveCommand.Create(() => { WindowTitle = Random.Shared.NextDouble().ToString(); });
    }
}