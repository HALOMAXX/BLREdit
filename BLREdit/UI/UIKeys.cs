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
    public static Dictionary<Key, UIBool> Keys { get; }

    static UIKeys() 
    {
        Keys = new Dictionary<Key, UIBool>();
        foreach (Key k in (Key[])Enum.GetValues(typeof(Key)))
        {
            if (!Keys.TryGetValue(k, out _))
            {
                Keys.Add(k, new(false));
            }
        }
    }

    public static void KeyDown(object sender, KeyEventArgs e)
    {
        if (Keys.TryGetValue(e.Key, out UIBool b))
        {
            b.SetBool(true);
        }
    }

    public static void KeyUp(object sender, KeyEventArgs e)
    {
        if (Keys.TryGetValue(e.Key, out UIBool b))
        {
            b.SetBool(false);
        }
    }
}
