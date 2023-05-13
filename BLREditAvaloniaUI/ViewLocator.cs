using Avalonia.Controls;
using Avalonia.Controls.Templates;

using BLREdit.ViewModels;

using System;
using System.Reflection;

namespace BLREdit;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data?.GetType()?.FullName?.Replace("ViewModel", "View") is string fullViewName)
        {
            Type? type = Type.GetType(fullViewName);

            if (type is not null && Activator.CreateInstance(type) is Control control) { return control; }
        }

        return new TextBlock { Text = "Not Found: " + nameof(data) };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}