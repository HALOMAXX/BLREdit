using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace BLREdit.UI;

public sealed class UIKeys
{
    public static Dictionary<Key, UIBool> Keys { get; } = [];
    public static UIKeys Instance { get; } = new();

    public static void SetAll(bool pressed)
    {
        foreach (var key in Keys)
        {
            key.Value.Set(pressed);
        }
    }

    static UIKeys() 
    {
        foreach (Key k in (Key[])Enum.GetValues(typeof(Key)))
        {
            if (!Keys.TryGetValue(k, out _))
            {
                Keys.Add(k, new(false));
            }
        }
    }

    private UIKeys() { }

    public void KeyDown(object sender, KeyEventArgs e)
    {
        if (e is null) return;
        if (e.Key == Key.System)
        {
            SetKey(e.SystemKey, true);
        }
        else
        {
            SetKey(e.Key, true);
        }
    }

    public void KeyUp(object sender, KeyEventArgs e)
    {
        if (e is null) return;
        if (e.Key == Key.System)
        {
            SetKey(e.SystemKey, false);
        }
        else
        {
            SetKey(e.Key, false);
        }
    }

    static void SetKey(Key key, bool down)
    {
        if (Keys.TryGetValue(key, out UIBool b))
        {
            b.Set(down);
        }
    }
}
