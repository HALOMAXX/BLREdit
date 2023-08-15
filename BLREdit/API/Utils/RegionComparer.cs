using System;
using System.Collections.Generic;

namespace BLREdit.API.Utils;

public sealed class RegionComparer : IComparer<string>
{
    public static readonly RegionComparer Instance = new RegionComparer();

    Dictionary<string, double> RegionDistances = new Dictionary<string, double>() { { "EU-AU", 14092.82 } };

    int IComparer<string>.Compare(string x, string y)
    {
        throw new NotImplementedException();
        //TODO: Not to sure how to do Matchmaking based on Region
    }
}
