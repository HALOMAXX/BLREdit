using System.ComponentModel;

namespace BLREdit.Core;

public abstract class ModelBase : INotifyPropertyChanged
{
#pragma warning disable CS0067 // The event 'ModelBase.PropertyChanged' is never used
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067 // The event 'ModelBase.PropertyChanged' is never used
}
