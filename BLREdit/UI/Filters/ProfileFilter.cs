using BLREdit.Export;
using BLREdit.Import;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BLREdit.UI;

sealed class ProfileFilter : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion Events

    public event EventHandler? RefreshItemLists;

    public static ProfileFilter Instance { get; private set; } = new ProfileFilter();

    private string searchFilter = "";
    public string SearchFilter { get { return searchFilter; } set { searchFilter = value; RefreshItemLists?.Invoke(this, new EventArgs()); OnPropertyChanged(); } }

    public bool FullFilter(object o)
    {
        if (string.IsNullOrEmpty(SearchFilter)) return true;
        bool contains = false;
        if (o is BLRLoadoutStorage loadout)
        {
            if (loadout.Shareable.Name.ToLower().Contains(SearchFilter.ToLower())) contains = true;
        }
        return contains;
    }
}