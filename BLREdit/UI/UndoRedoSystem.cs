using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

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
    private static BlockEvents currentlyBlockedEvents = BlockEvents.None;
    public static BlockEvents CurrentlyBlockedEvents { get { return currentlyBlockedEvents; } set { BlockedEventHistory.Push(currentlyBlockedEvents); currentlyBlockedEvents = value; } }
    private static Stack<BlockEvents> BlockedEventHistory { get; } = new();

    public static void RestoreBlockedEvents()
    {
        if (BlockedEventHistory.Count <= 0)
        {
            CurrentlyBlockedEvents = BlockEvents.None;
            LoggingSystem.Log("Tried to Restore BlockedEventState but no History is left! returning to Blank State");
        }
        else
        {
            var was = currentlyBlockedEvents;
            currentlyBlockedEvents = BlockedEventHistory.Pop();
            if (BLREditSettings.Settings.Debugging.Is) { LoggingSystem.Log($"Restored BlockedEventState was[{was}] now is[{currentlyBlockedEvents}]!"); }
        }
    }

    /// <summary>
    /// Undoes one Action Chain
    /// </summary>
    public static void Undo()
    {
        if (UndoStack.Count <= 0) { MainWindow.ShowAlert($"No Undo's left"); return; }
        //UndoRedoSystemWorking = true;
        var action = UndoStack.Pop();
        foreach (var sub in action.Actions)
        {
            CurrentlyBlockedEvents = sub.BlockedEvents;
            sub.PropertyInfo.SetValue(sub.Target, sub.Before, null);
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
        //UndoRedoSystemWorking = true;
        var action = RedoStack.Pop();
        foreach (var sub in action.Actions)
        {
            CurrentlyBlockedEvents = sub.BlockedEvents;
            sub.PropertyInfo.SetValue(sub.Target, sub.After);
            RestoreBlockedEvents();
        }
        UndoStack.Push(action);
        MainWindow.ShowAlert($"Redone! ({UndoStack.Count}/{RedoStack.Count})");
        UndoRedoSystemWorking = false;
    }

    /// <summary>
    /// Ends an Action Chain
    /// </summary>
    public static void EndAction(bool ignoreAfterActions = false)
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
    public static void DoAction(object? after, PropertyInfo propertyInfo, object? target, BlockEvents blockedEvents = BlockEvents.None, [CallerMemberName] string? callName = null)
    {
        if (CurrentAction.Actions is null) { LoggingSystem.Log("CurrentAction is null which should never happen!"); return; }
        CurrentlyBlockedEvents = blockedEvents;
        object before = propertyInfo.GetValue(target);
        CurrentAction.Actions.Add(new SubUndoRedoAction(before, after, propertyInfo, target, blockedEvents, callName));
        propertyInfo.SetValue(target, after);
        RestoreBlockedEvents();
    }

    public static void DoActionAfter(object? after, PropertyInfo propertyInfo, object? target, BlockEvents blockedEvents = BlockEvents.None, [CallerMemberName] string? callName = null)
    {
        CurrentlyBlockedEvents = blockedEvents;
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
    public static void CreateAction(object? before, object? after, PropertyInfo propertyInfo, object? target, BlockEvents blockedEvents = BlockEvents.None, [CallerMemberName] string? callName = null)
    {
        if (CurrentAction.Actions is null) { LoggingSystem.Log("CurrentAction is null which should never happen!"); return; }
        CurrentlyBlockedEvents = blockedEvents;
        CurrentAction.Actions.Add(new SubUndoRedoAction(before, after, propertyInfo, target, blockedEvents, callName));
        RestoreBlockedEvents();
    }
}

public struct UndoRedoAction
{
    internal List<SubUndoRedoAction> Actions { get; private set; }
    public UndoRedoAction()
    { 
        Actions = new List<SubUndoRedoAction>();
    }
}

public struct SubUndoRedoAction(object? before, object? after, PropertyInfo propertyInfo, object? target, BlockEvents blockedEvents, string? callName)
{
    internal object? Before { get; private set; } = before;
    internal object? After { get; private set; } = after;
    internal PropertyInfo PropertyInfo { get; private set; } = propertyInfo;
    internal object? Target { get; private set; } = target;
    internal BlockEvents BlockedEvents { get; private set; } = blockedEvents;
    internal string? CallName { get; private set; } = callName;
}

[Flags]
public enum BlockEvents
{ 
    None = 0,
    All = ~0,
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
}