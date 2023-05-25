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
        //TODO Intigrate Undo Redo System
        Debug.WriteLine($"{target}.{propertyName} was {before} is now {after}");
        onPropertyChangedAction();
    }

    public static void InitializationFinished()
    { 
        PauseNotify = false;
    }
}
