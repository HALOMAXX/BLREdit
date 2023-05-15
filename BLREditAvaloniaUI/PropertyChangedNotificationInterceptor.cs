using System;
using System.ComponentModel;
using System.Diagnostics;

namespace BLREdit;

public static class PropertyChangedNotificationInterceptor
{
    public static void Intercept(object target, Action onPropertyChangedAction, string propertyName, object before, object after)
    {
        //TODO Intigrate Undo Redo System
        Debug.WriteLine($"{propertyName} was {before} is now {after}");
        onPropertyChangedAction();
    }
}
