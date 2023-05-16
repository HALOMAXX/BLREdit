using System;
using System.ComponentModel;
using System.Diagnostics;

namespace BLREdit;

public static class PropertyChangedNotificationInterceptor
{
    public static bool PauseNotify = true;
    public static void Intercept(object target, Action onPropertyChangedAction, string propertyName, object before, object after)
    {
        if (PauseNotify) return;
        //TODO Intigrate Undo Redo System
        Debug.WriteLine($"{target}.{propertyName} was {before} is now {after}");
        onPropertyChangedAction();
    }
}
