using System;
using System.Diagnostics;
using System.Windows.Input;

namespace BLREdit.Game;

sealed class RelayCommand : ICommand
{
    #region Fields

    readonly Action<object>? _execute;
    readonly Predicate<object>? _canExecute;

    #endregion // Fields

    #region Constructors

    /// <summary>
    /// Creates a new command that can always execute.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    public RelayCommand(Action<object> execute)
        : this(execute, null)
    {
    }

    /// <summary>
    /// Creates a new command.
    /// </summary>
    /// <param name="execute">The execution logic.</param>
    /// <param name="canExecute">The execution status logic.</param>
    public RelayCommand(Action<object> execute, Predicate<object>? canExecute)
    {
        if (execute is not null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
    }

    #endregion // Constructors

    #region ICommand Members

    [DebuggerStepThrough]
    public bool CanExecute(object parameters)
    {
        return _canExecute == null || _canExecute(parameters);
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public void Execute(object parameters)
    {   
        if(_execute is not null)
            _execute(parameters);
    }

    #endregion // ICommand Members
}
