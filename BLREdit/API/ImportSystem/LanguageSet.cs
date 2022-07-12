using System;
using System.Collections.Generic;

namespace BLREdit.API.ImportSystem;

public class LanguageSet
{
    private static LanguageSet currentLanguageSet = CreateDefaultSet();
    public string LanguageName { get; set; }
    public Dictionary<string, string> Words { get; set; }

    public static string GetWord(string word)
    {
        string outWord;
        if (currentLanguageSet.Words.TryGetValue(word, out outWord))
        {
            return outWord;
        }
        return "--" + word + "--";
    }

    public static List<string> GetWords(Type type)
    {
        List<string> words = new();
        if (type.IsEnum)
        {
            foreach (var num in Enum.GetValues(type)) 
            {
                words.Add(GetWord(Enum.GetName(type, num)));
            }
            return words;
        }
        return words;
    }

    private static LanguageSet CreateDefaultSet()
    {
        LanguageSet languageSet = new()
        {
            LanguageName = "Default",
            Words = new Dictionary<string, string>()
            {
                { LanguageKeys.AMMO, "Ammo" },
                { LanguageKeys.DAMAGE, "Damage" },
                { LanguageKeys.RECOIL, "Recoil" },
                { LanguageKeys.RELOAD, "Reload" },
                { LanguageKeys.RANGE, "Range" },
                { LanguageKeys.RUN, "Run" },
                { LanguageKeys.MOVE, "Move" },
                { LanguageKeys.AIM, "Aim" },
                { LanguageKeys.HIP, "Hip" },
                { LanguageKeys.ACCURACY, "Accuracy" },
                { LanguageKeys.NONE, "None" },
                { LanguageKeys.NAME, "Name" },
                { LanguageKeys.ZOOM, "Zoom" },
                { LanguageKeys.SCOPE_IN_TIME, "Scope In Time" },
                { LanguageKeys.INFRARED, "Infra" },
                { LanguageKeys.INCENDIARY_PROTECTION, "Inc" },
                { LanguageKeys.TOXIC_PROTECTION, "Tox" },
                { LanguageKeys.EXPLOSIVE_PROTECTION, "Exp" },
                { LanguageKeys.ELECTRO_PROTECTION, "Elec" },
                { LanguageKeys.MELEE_PROTECTION, "Melee" },
                { LanguageKeys.INFRARED_PROTECTION, "Infra" },
                { LanguageKeys.HEALTH, "Health" },
                { LanguageKeys.HEAD_PROTECTION, "Head Prot" },
                { LanguageKeys.HRV_DURATION, "HRV Duration" },
                { LanguageKeys.HRV_RECHARGE, "HRV Recharge" },
                { LanguageKeys.GEAR_SLOTS, "Gear Slots" },

            }
        };
        return languageSet;
    }

    private static LanguageSet CreateEmojiSet()
    {
        LanguageSet languageSet = new()
        {
            LanguageName = "Emoji",
            Words = new Dictionary<string, string>()
            {
                { LanguageKeys.AMMO, "🔋" },
                { LanguageKeys.DAMAGE, "⚔" },
                { LanguageKeys.RECOIL, "💨" },
                { LanguageKeys.RELOAD, "🔄" },
                { LanguageKeys.RANGE, "📏" },
                { LanguageKeys.RUN, "🏃" },
                { LanguageKeys.MOVE, "🚶‍🎯" },
                { LanguageKeys.AIM, "🔎🎯" },
                { LanguageKeys.HIP, "🧍‍🎯" },
                { LanguageKeys.ACCURACY, "🎯" },
                { LanguageKeys.NONE, "None" },
                { LanguageKeys.NAME, "Name" },
                { LanguageKeys.ZOOM, "🔎" },
                { LanguageKeys.SCOPE_IN_TIME, "🔎⌚" },
                { LanguageKeys.INFRARED, "🌀" },
                { LanguageKeys.INCENDIARY_PROTECTION, "🔥🔰" },
                { LanguageKeys.TOXIC_PROTECTION, "☣🔰" },
                { LanguageKeys.EXPLOSIVE_PROTECTION, "💥🔰" },
                { LanguageKeys.ELECTRO_PROTECTION, "⚡🔰" },
                { LanguageKeys.MELEE_PROTECTION, "🔪🔰" },
                { LanguageKeys.INFRARED_PROTECTION, "🌀🔰" },
                { LanguageKeys.HEALTH, "❤" },
                { LanguageKeys.HEAD_PROTECTION, "🙂🔰" },
                { LanguageKeys.HRV_DURATION, "⏱" },
                { LanguageKeys.HRV_RECHARGE, "♻" },
                { LanguageKeys.GEAR_SLOTS, "🧯" },
            }
        };
        return languageSet;
    }
}

public static class LanguageKeys
{
    public const string AMMO = "Ammo";
    public const string DAMAGE = "Damage";
    public const string RECOIL = "Recoil";
    public const string RELOAD = "Reload";
    public const string RANGE = "Range";
    public const string RUN = "Run";
    public const string MOVE = "Move";
    public const string AIM = "Aim";
    public const string HIP = "Hip";
    public const string ACCURACY = "Accuracy";
    public const string NONE = "None";
    public const string NAME = "Name";
    public const string ZOOM = "Zoom";
    public const string SCOPE_IN_TIME = "ScopeInTime";
    public const string INFRARED = "Infrared";
    public const string INCENDIARY_PROTECTION = "IncendiaryProtection";
    public const string TOXIC_PROTECTION = "ToxicProtection";
    public const string EXPLOSIVE_PROTECTION = "ExplosiveProtection";
    public const string ELECTRO_PROTECTION = "ElectroProtection";
    public const string MELEE_PROTECTION = "MeleeProtection";
    public const string INFRARED_PROTECTION = "InfraredProtection";
    public const string HEALTH = "Health";
    public const string HEAD_PROTECTION = "HeadProtection";
    public const string HRV_DURATION = "HRVDuration";
    public const string HRV_RECHARGE = "HRVRecharge";
    public const string GEAR_SLOTS = "GearSlots";
}
