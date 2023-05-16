using Avalonia.Controls;
using Avalonia.Controls.Templates;

using BLREdit.Models.BLR;
using BLREdit.ViewModels;
using BLREdit.Views;

namespace BLREdit.DataTemplates.BLR;

public sealed class BLRDataTemplate : IDataTemplate
{
    public Control? Build(object? param)
    {
        switch (param)
        {
            case BLRClient client:
                return new BLRClientView() { DataContext = client };
            case BLRServer server:
                return new BLRServerView() { DataContext = server };
            case BLRItem item:
                return new BLRItemView() { DataContext = item };
            default: return null;
        }       
    }

    public bool Match(object? data)
    {
        return data is BLRClient || data is BLRServer|| data is BLRItem;
    }
}
