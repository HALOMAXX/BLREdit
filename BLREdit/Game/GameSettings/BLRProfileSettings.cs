using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.Game;

public sealed class BLRProfileSettings
{
    public int Owner { get; set; }
    public BLRProfileSetting? ProfileSetting { get; set; }
}


public class BLRProfileSetting
{
    public int AdvertisementType { get; set; }
    public BLRProfileSettingData? Data { get; set; }
    public int PropertyId { get; set; }
}

public class BLRProfileSettingData
{
    public int Type { get; set; }
    public int Value1 { get; set; }
}

