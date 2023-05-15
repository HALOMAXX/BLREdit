using Avalonia.Controls;
using Avalonia.Controls.Templates;

using BLREdit.Models.BLR;
using BLREdit.ViewModels;
using BLREdit.Views;

namespace BLREdit.DataTemplates;

public sealed class BLRClientDataTemplate : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is BLRClient client)
        { return new BLRClientView() { DataContext = client }; }
        else
        { return null; }
       
    }

    public bool Match(object? data)
    {
        return data is BLRClient;
    }
}
