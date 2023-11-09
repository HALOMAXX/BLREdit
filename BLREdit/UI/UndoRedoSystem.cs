using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BLREdit.UI;

public static class UndoRedoSystem
{
    private static readonly Stack<UndoRedoAction> UndoStack = new();
    private static readonly Stack<UndoRedoAction> RedoStack = new();
    private static UndoRedoAction CurrentAction = new();
    private static readonly List<SubUndoRedoAction> AfterActions = new();
    public static int CurrentActionCount { get { return CurrentAction.Actions.Count; } }
    public static int AfterActionCount { get { return AfterActions.Count; } }
    public static bool UndoRedoSystemWorking { get; private set; } = false;
    private static ThreadLocal<BlockEvents> currentlyBlockedEvents = new(() => { return BlockEvents.None; });
    public static ThreadLocal<BlockEvents> CurrentlyBlockedEvents { get { return currentlyBlockedEvents; } set { BlockedEventHistory.Value.Push(currentlyBlockedEvents.Value); currentlyBlockedEvents = value; } }
    private static ThreadLocal<Stack<BlockEvents>> BlockedEventHistory { get; } = new(() => { return new(); });

    public static void RestoreBlockedEvents()
    {
        if (BlockedEventHistory.Value.Count <= 0) { CurrentlyBlockedEvents.Value = BlockEvents.None; }
        else
        {
            var was = currentlyBlockedEvents.Value;
            currentlyBlockedEvents.Value = BlockedEventHistory.Value.Pop();
            if (DataStorage.Settings.Debugging.Is) { LoggingSystem.Log($"Thread[{Environment.CurrentManagedThreadId}]: Restored BlockedEventState was[{was}] now is[{currentlyBlockedEvents.Value}]!"); }
        }
    }

    /// <summary>
    /// Undoes one Action Chain
    /// </summary>
    public static void Undo()
    {
        if (UndoStack.Count <= 0) { MainWindow.ShowAlert($"No Undo's left"); return; }
        UndoRedoSystemWorking = true;
        var action = UndoStack.Pop();
        foreach (var sub in action.Actions)
        {
            CurrentlyBlockedEvents.Value = sub.BlockedEvents;
            switch(sub.Type)
            {
                case 0:
                    sub.PropertyInfo.SetValue(sub.Target, sub.Before, null);
                    break;
                case 1:
                    sub.UndoAction?.Invoke();
                    break;
            }
            
            RestoreBlockedEvents();
        }
        RedoStack.Push(action);
        MainWindow.ShowAlert($"Undone! ({UndoStack.Count}/{RedoStack.Count})");
        UndoRedoSystemWorking = false;
    }

    /// <summary>
    /// Redoes one Action Chain
    /// </summary>
    public static void Redo()
    {
        if (RedoStack.Count <= 0) { MainWindow.ShowAlert($"No Redo's left"); return; }
        UndoRedoSystemWorking = true;
        var action = RedoStack.Pop();
        foreach (var sub in action.Actions)
        {
            CurrentlyBlockedEvents.Value = sub.BlockedEvents;
            switch (sub.Type)
            {
                case 0:
                    sub.PropertyInfo.SetValue(sub.Target, sub.After);
                    break;
                case 1:
                    sub.DoAction?.Invoke();
                    break;
            }
            
            RestoreBlockedEvents();
        }
        UndoStack.Push(action);
        MainWindow.ShowAlert($"Redone! ({UndoStack.Count}/{RedoStack.Count})");
        UndoRedoSystemWorking = false;
    }

    public static void ClearUndoRedoStack()
    { 
        UndoStack.Clear();
        RedoStack.Clear();
    }

    /// <summary>
    /// Ends an Action Chain
    /// </summary>
    public static void EndUndoRecord(bool ignoreAfterActions = false)
    {
        if (CurrentAction.Actions is not null)
        {
            if (!ignoreAfterActions) { CurrentAction.Actions.AddRange(AfterActions); }
            AfterActions.Clear();
            RedoStack.Clear();
            UndoStack.Push(CurrentAction);
        }
        else
        {
            LoggingSystem.Log("CurrentAction.actions is null which should not happen");
        }
        CurrentAction = new();
    }

    /// <summary>
    /// Creates an action and does it it will get the before state by itself
    /// </summary>
    /// <param name="after">how it is after this action</param>
    /// <param name="propertyInfo">the property that is modified</param>
    /// <param name="target">the target object if the target is static type then just give it null</param>
    /// <param name="shouldBlockEvent">should it block possible UI Events</param>
    public static void DoValueChange(object? after, PropertyInfo propertyInfo, object? target, BlockEvents blockedEvents = BlockEvents.None, [CallerMemberName] string? callName = null)
    {
        CurrentlyBlockedEvents.Value = blockedEvents;
        object before = propertyInfo.GetValue(target);
        CurrentAction.Actions.Add(new SubUndoRedoAction(before, after, propertyInfo, target, blockedEvents, callName));
        propertyInfo.SetValue(target, after);
        RestoreBlockedEvents();
    }

    /// <summary>
    /// Creates an action and does it it will get the before state by itself
    /// </summary>
    /// <param name="after">how it is after this action</param>
    /// <param name="propertyInfo">the property that is modified</param>
    /// <param name="target">the target object if the target is static type then just give it null</param>
    /// <param name="shouldBlockEvent">should it block possible UI Events</param>
    public static void DoAction(Action DoAction, Action UndoAction, BlockEvents blockedEvents = BlockEvents.None, [CallerMemberName] string? callName = null)
    {
        if (CurrentAction.Actions is null) { LoggingSystem.Log("CurrentAction is null which should never happen!"); return; }
        CurrentlyBlockedEvents.Value = blockedEvents;
        CurrentAction.Actions.Add(new SubUndoRedoAction(null, null, null, null, blockedEvents, callName, 1, DoAction, UndoAction));
        DoAction.Invoke();
        RestoreBlockedEvents();
    }

    public static void DoValueChangeAfter(object? after, PropertyInfo propertyInfo, object? target, BlockEvents blockedEvents = BlockEvents.None, [CallerMemberName] string? callName = null)
    {
        CurrentlyBlockedEvents.Value = blockedEvents;
        object before = propertyInfo.GetValue(target);
        AfterActions.Add(new SubUndoRedoAction(before, after, propertyInfo, target, blockedEvents, callName));
        propertyInfo.SetValue(target, after);
        RestoreBlockedEvents();
    }

    /// <summary>
    /// Creates an action that already happened
    /// </summary>
    /// <param name="before">how it was before this action</param>
    /// <param name="after">how it is after this action</param>
    /// <param name="propertyInfo">the property that is modified</param>
    /// <param name="target">the target object if the target is static type then just give it null</param>
    /// <param name="shouldBlockEvent">should it block possible UI Events</param>
    public static void CreateValueChange(object? before, object? after, PropertyInfo propertyInfo, object? target, BlockEvents blockedEvents = BlockEvents.None, [CallerMemberName] string? callName = null)
    {
        if (CurrentAction.Actions is null) { LoggingSystem.Log("CurrentAction is null which should never happen!"); return; }
        CurrentlyBlockedEvents.Value = blockedEvents;
        CurrentAction.Actions.Add(new SubUndoRedoAction(before, after, propertyInfo, target, blockedEvents, callName));
        RestoreBlockedEvents();
    }
}

public struct UndoRedoAction()
{
    internal List<SubUndoRedoAction> Actions { get; private set; } = new List<SubUndoRedoAction>();
}

public struct SubUndoRedoAction(object? before, object? after, PropertyInfo propertyInfo, object? target, BlockEvents blockedEvents, string? callName, int type = 0, Action? doAction = null, Action? undoAction = null)
{
    internal int Type { get; private set; } = type;
    internal object? Before { get; private set; } = before;
    internal object? After { get; private set; } = after;
    internal PropertyInfo PropertyInfo { get; private set; } = propertyInfo;
    internal object? Target { get; private set; } = target;
    internal BlockEvents BlockedEvents { get; private set; } = blockedEvents;
    internal string? CallName { get; private set; } = callName;
    internal Action? DoAction { get; private set; } = doAction;
    internal Action? UndoAction { get; private set; } = undoAction;
}

[Flags]
public enum BlockEvents
{ 
    None = 0,
    All = ~0,
    AllExceptUpdate = All & ~Update,
    WriteAll = 7,
    WriteProfile = 1,
    WriteLoadout = 2,
    WriteWeapon = 4,
    ReadAll = 56,
    ReadProfile = 8,
    ReadLoadout = 16,
    ReadWeapon = 32,
    Update = 64,
    Calculate = 128,
    Remove = 256,
    AddMissing = 512,
    ScopeUpdate = 1024,
    GenderUpdate = 2048,
    SetValueTest = 4096,
}