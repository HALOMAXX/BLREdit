using System;
using System.ComponentModel;
using System.Diagnostics;

namespace BLREdit.Core;

public static class PropertyChangedNotificationInterceptor
{
    private static bool PauseNotify = true;
    public static void Intercept(object target, Action onPropertyChangedAction, string propertyName, object before, object after)
    {
        if (PauseNotify || onPropertyChangedAction is null) return;
        //TODO Integrate Undo Redo System Might add something that focuses the UI Element when it's underlying value changes so if you undo something it switches to that element
        Debug.WriteLine($"{target}.{propertyName} was {before} is now {after}");
        onPropertyChangedAction();
    }

    public static void InitializationFinished()
    { 
        PauseNotify = false;
    }
}
