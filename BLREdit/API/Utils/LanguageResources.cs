using BLREdit.Properties;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.API.Utils;

public sealed class LanguageResources
{
    private static ObservableCollection<string> EmptyStringCollection { get; } = new();
    private static Dictionary<string, ObservableCollection<string>> EnumList { get; } = new();

    public static ObservableCollection<string> GetWordsOfEnum(Type enumType)
    {
        if(!enumType.IsEnum) return EmptyStringCollection;
        if (EnumList.TryGetValue(enumType.Name, out ObservableCollection<string> value)) { return value; }
        ObservableCollection<string> words = new();
        foreach (var num in Enum.GetValues(enumType))
        {
            string enumName = Enum.GetName(enumType, num);
            words.Add(Resources.ResourceManager.GetString($"enum_{enumName}") ?? $"Missing: {enumName}");
        }
        EnumList.Add(enumType.Name, words);
        return words;
    }
}
