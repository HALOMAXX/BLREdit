using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit;

public abstract class ModelBase : INotifyPropertyChanged
{
#pragma warning disable CS0067 // The event 'ModelBase.PropertyChanged' is never used
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067 // The event 'ModelBase.PropertyChanged' is never used
}
