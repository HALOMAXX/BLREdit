using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BLREdit.Export;

public sealed class ExportSystemProfile : MagiCowsProfile
{
    [JsonIgnore] public string Name { get { return '(' + Index.ToString() + ')' + PlayerName; } }
    public int Index { get; set; }

    public new ExportSystemProfile Clone()
    {
        ExportSystemProfile duplicate = base.Clone() as ExportSystemProfile;
        duplicate.Index = ExportSystem.Profiles.Count;
        return duplicate;
    }
}
