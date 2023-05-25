using Avalonia.Controls;
using Avalonia.Controls.Templates;

using BLREdit.Core.Models.BLR.Client;
using BLREdit.Core.Models.BLR.Item;
using BLREdit.Core.Models.BLR.Server;
using BLREdit.Views;

namespace BLREdit.DataTemplates.BLR;

public sealed class BLRDataTemplate : IDataTemplate
{
    public Control? Build(object? param)
    {
        return param switch
        {
            BLRClient client => new BLRClientView() { DataContext = client },
            BLRServer server => new BLRServerView() { DataContext = server },
            BLRItem item => new BLRItemView() { DataContext = item },
            _ => null,
        };
    }

    public bool Match(object? data)
    {
        return data is BLRClient || data is BLRServer|| data is BLRItem;
    }
}
