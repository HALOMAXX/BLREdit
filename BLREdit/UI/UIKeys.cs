using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BLREdit.UI;

public sealed class UIKeys
{
    public static Dictionary<Key, UIBool> Keys { get; } = new Dictionary<Key, UIBool>();
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
        if (Keys.TryGetValue(e.Key, out UIBool b))
        {
            b.Set(true);
        }
    }

    public void KeyUp(object sender, KeyEventArgs e)
    {
        if (Keys.TryGetValue(e.Key, out UIBool b))
        {
            b.Set(false);
        }
    }
}
