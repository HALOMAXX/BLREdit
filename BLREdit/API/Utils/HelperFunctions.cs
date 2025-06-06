﻿using BLREdit.Import;

using System.Linq;

namespace BLREdit.API.Utils;

public static class HelperFunctions
{
    public static int Truth(params bool[] bools)
    {
        return bools.Count(b => b);
    }

    public static int Truth(ItemReport flagsToCount, params ItemReport[] reports)
    {
        return reports.Count((r) => { return r.HasFlag(flagsToCount); });
    }

    public static bool AllTruth(params bool[] bools)
    {
        if (bools is null) { LoggingSystem.LogNull(); return true; }
        return Truth(bools) == bools.Length;
    }

    public static bool AllTruth(ItemReport flagsToCount, params ItemReport[] reports)
    {
        if (reports is null) { LoggingSystem.LogNull(); return true; }
        return Truth(flagsToCount, reports) == reports.Length;
    }

    public static ItemReport ItemCheck(BLREditItem? item, BLREditItem? filter)
    {
        return (BLREditItem.IsValidFor(item, filter) ? ItemReport.Valid : ItemReport.Invalid) | (item is null ? ItemReport.Missing : ItemReport.None);
    }

    public static ItemReport ItemCheck(BLREditItem? item, bool duplicate, bool missing)
    {
        return ItemCheck(item, null) | (duplicate ? ItemReport.Duplicate : ItemReport.None) | (missing ? ItemReport.Missing : ItemReport.None);
    }

    public static bool HasAnyFlags(ItemReport report, params ItemReport[] flags)
    {
        if (flags is null) { LoggingSystem.LogNull(); return false; }
        foreach (var flag in flags)
        { 
            if(report.HasFlag(flag)) return true;
        }
        return false;
    }

    public static bool HasAllFlags(ItemReport report, params ItemReport[] flags)
    {
        if (flags is null) { LoggingSystem.LogNull(); return true; }
        foreach (var flag in flags)
        {
            if (!report.HasFlag(flag)) return false;
        }
        return true;
    }
}
